Imports System.Runtime.InteropServices
Imports Oracle.LinuxCompatibility.MySQL

''' <summary>
''' Samples CPU / memory / thread count of the mysqld server process on Linux
''' by reading <c>/proc/&lt;pid&gt;/stat</c> and <c>/proc/&lt;pid&gt;/status</c>.
''' The pid is resolved from <see cref="VariablesReader.GetPidFile"/>, with a
''' fallback to <c>pgrep mysqld</c> when the pid file is unavailable.
''' </summary>
Public Class ProcessMonitor

    ReadOnly vars As VariablesReader

    Private _pid As Integer = -1
    Private _clkTck As Integer = 100
    Private _prevUtime As Long = -1
    Private _prevStime As Long = -1
    Private _prevWall As Date = Date.MinValue

    ''' <summary>
    ''' Number of clock ticks per second. Resolved via sysconf on first use.
    ''' </summary>
    Private ReadOnly Property ClkTck As Integer
        Get
            If _clkTck <= 0 Then
                Try
                    _clkTck = CInt(sysconf(&H2)) ' _SC_CLK_TCK = 2
                Catch
                    _clkTck = 100
                End Try
            End If
            Return _clkTck
        End Get
    End Property

    Sub New(vars As VariablesReader)
        Me.vars = vars
    End Sub

    ''' <summary>
    ''' Resolve the mysqld pid. Returns -1 when it cannot be determined.
    ''' </summary>
    Private Function ResolvePid() As Integer
        If _pid > 0 Then
            Return _pid
        End If

        ' 1) From the pid_file variable reported by MySQL.
        Dim pidFile = vars.GetPidFile()
        If Not String.IsNullOrWhiteSpace(pidFile) Then
            Dim file = pidFile.Trim()
            If file.Contains("/") AndAlso IO.File.Exists(file) Then
                Dim txt = IO.File.ReadAllText(file).Trim()
                Dim n As Integer
                If Integer.TryParse(txt, n) AndAlso n > 0 Then
                    _pid = n
                    Return _pid
                End If
            Else
                ' Some installations report only the pid number in pid_file.
                Dim n As Integer
                If Integer.TryParse(pidFile, n) AndAlso n > 0 Then
                    _pid = n
                    Return _pid
                End If
            End If
        End If

        ' 2) Fallback: pgrep mysqld
        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = "pgrep",
                .Arguments = "-o mysqld",
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .CreateNoWindow = True
            }
            Using p = Process.Start(psi)
                Dim out = p.StandardOutput.ReadToEnd().Trim()
                p.WaitForExit()
                Dim n As Integer
                If Integer.TryParse(out, n) AndAlso n > 0 Then
                    _pid = n
                    Return _pid
                End If
            End Using
        Catch
        End Try

        ' 3) Fallback: scan /proc by process comm name (mysqld / mariadbd).
        '    Does not depend on pgrep being installed. Picks the first matching
        '    process that exposes a readable /proc/<pid>/stat.
        Dim procDir = "/proc"
        If IO.Directory.Exists(procDir) Then
            For Each dir As String In IO.Directory.GetDirectories(procDir)
                Dim name = IO.Path.GetFileName(dir)
                Dim n As Integer
                If Not Integer.TryParse(name, n) OrElse n <= 0 Then
                    Continue For
                End If
                If IsMatchingServerProcess(dir) Then
                    _pid = n
                    Return _pid
                End If
            Next
        End If

        Return -1
    End Function

    ''' <summary>
    ''' Returns true when the process directory under /proc points to a MySQL
    ''' server process (mysqld or mariadbd), detected via /proc/&lt;pid&gt;/comm or
    ''' the comm field inside /proc/&lt;pid&gt;/stat.
    ''' </summary>
    Private Shared Function IsMatchingServerProcess(procDir As String) As Boolean
        ' Preferred: /proc/<pid>/comm holds the short executable name.
        Dim commFile = IO.Path.Combine(procDir, "comm")
        If IO.File.Exists(commFile) Then
            Dim comm = IO.File.ReadAllText(commFile).Trim()
            If comm = "mysqld" OrElse comm = "mariadbd" Then
                Return True
            End If
        End If

        ' Fallback: parse comm from /proc/<pid>/stat (between parentheses).
        Dim statFile = IO.Path.Combine(procDir, "stat")
        If IO.File.Exists(statFile) Then
            Try
                Dim stat = IO.File.ReadAllText(statFile)
                Dim lp = stat.IndexOf("("c)
                Dim rp = stat.IndexOf(")"c)
                If lp >= 0 AndAlso rp > lp Then
                    Dim comm = stat.Substring(lp + 1, rp - lp - 1).Trim()
                    If comm = "mysqld" OrElse comm = "mariadbd" Then
                        Return True
                    End If
                End If
            Catch
            End Try
        End If

        Return False
    End Function

    ''' <summary>
    ''' Take one sample of the process resources. The first call establishes a
    ''' baseline and returns a snapshot with CpuPercent = 0 (no delta yet).
    ''' </summary>
    Public Function Sample(Optional deltaSeconds As Double = 1.0) As ProcessSnapshot
        Dim snap As New ProcessSnapshot

        Dim pid = ResolvePid()
        If pid <= 0 Then
            snap.Available = False
            snap.Note = "pid not found"
            _prevUtime = -1
            Return snap
        End If

        Dim statFile = $"/proc/{pid}/stat"
        Dim statusFile = $"/proc/{pid}/status"

        If Not IO.File.Exists(statFile) OrElse Not IO.File.Exists(statusFile) Then
            snap.Available = False
            snap.Note = $"/proc/{pid} missing"
            _prevUtime = -1
            Return snap
        End If

        Try
            ' /proc/<pid>/stat: fields separated by spaces, but the process name
            ' (field 2) may contain spaces and is wrapped in parentheses.
            Dim stat = IO.File.ReadAllText(statFile)
            Dim rparen = stat.IndexOf(")"c)
            ' Fields after ')': pid) comm state ppid pgrp session tty_nr tpgid flags
            ' minflt cminflt majflt cmajflt utime stime ...
            ' utime is the 14th field overall, i.e. (rparen + 3)th token.
            Dim tokens = stat.Substring(rparen + 1).Trim().Split(" "c)
            ' tokens(0) -> comm state -> actually first token after ) is "state"
            ' layout after ): state ppid pgrp session tty_nr tpgid flags minflt cminflt majflt cmajflt utime stime
            ' index:        0     1     2     3       4       5     6     7       8       9      10      11     12
            Dim utime = SafeLong(tokens(11))
            Dim stime = SafeLong(tokens(12))

            ' /proc/<pid>/status: "VmRSS:" and "Threads:" lines.
            Dim status = IO.File.ReadAllText(statusFile)
            Dim vmRss = ParseKb(status, "VmRSS:")
            Dim threads = ParseInt(status, "Threads:")

            Dim now = Date.Now
            snap.MemoryBytes = vmRss * 1024L
            snap.ThreadCount = threads
            snap.Available = True

            If _prevUtime >= 0 AndAlso _prevWall <> Date.MinValue Then
                Dim cpuTicks = CDbl(utime + stime - _prevUtime - _prevStime)
                Dim dt = (now - _prevWall).TotalSeconds
                If dt > 0 Then
                    ' percent of one cpu; multiply by 100. For multi-core this can exceed 100.
                    snap.CpuPercent = (cpuTicks / ClkTck) / dt * 100.0
                End If
            End If

            _prevUtime = utime
            _prevStime = stime
            _prevWall = now
        Catch ex As Exception
            snap.Available = False
            snap.Note = "read error: " & ex.Message
            _prevUtime = -1
        End Try

        Return snap
    End Function

    Private Shared Function SafeLong(s As String) As Long
        Dim v As Long
        If Long.TryParse(s, v) Then
            Return v
        End If
        Return 0
    End Function

    Private Shared Function ParseKb(text As String, key As String) As Long
        Dim idx = text.IndexOf(key, StringComparison.OrdinalIgnoreCase)
        If idx < 0 Then
            Return 0
        End If
        Dim lineEnd = text.IndexOf(Environment.NewLine, idx)
        If lineEnd < 0 Then
            lineEnd = text.Length
        End If
        Dim line = text.Substring(idx + key.Length, lineEnd - (idx + key.Length))
        Dim parts = line.Trim().Split(" "c)
        If parts.Length > 0 Then
            Dim kb As Long
            If Long.TryParse(parts(0).Trim(), kb) Then
                Return kb
            End If
        End If
        Return 0
    End Function

    Private Shared Function ParseInt(text As String, key As String) As Integer
        Dim idx = text.IndexOf(key, StringComparison.OrdinalIgnoreCase)
        If idx < 0 Then
            Return 0
        End If
        Dim lineEnd = text.IndexOf(Environment.NewLine, idx)
        If lineEnd < 0 Then
            lineEnd = text.Length
        End If
        Dim line = text.Substring(idx + key.Length, lineEnd - (idx + key.Length))
        Dim n As Integer
        If Integer.TryParse(line.Trim(), n) Then
            Return n
        End If
        Return 0
    End Function

    ' sysconf from libc (Linux). _SC_CLK_TCK = 2.
    <DllImport("libc", EntryPoint:="sysconf")>
    Private Shared Function sysconf(name As Integer) As Long
    End Function

End Class
