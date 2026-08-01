Imports System.Text
Imports Oracle.LinuxCompatibility.MySQL
Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter

''' <summary>
''' MySqlMonitor - a realtime MySQL performance monitor for the console.
'''
''' Connects to a MySQL server, samples performance counters / process resources
''' on a fixed interval and renders a full-screen ANSI dashboard.
''' </summary>
Module Program

    Private _running As Boolean = True
    Private _mysql As MySqli = Nothing

    Sub Main(args As String())
        Dim opts = MonitorOptions.Parse(args)

        If opts.ShowHelp Then
            MonitorOptions.PrintHelp()
            Return
        End If

        ' Cancel handling: restore the terminal on Ctrl+C instead of leaving it
        ' in the alternate-screen / hidden-cursor state.
        AddHandler Console.CancelKeyPress, AddressOf OnCancel

        Dim uri = opts.BuildConnectionUri()

        Try
            ' The MySqli constructor establishes the connection (and validates it
            ' via a ping). A failure throws and is handled below.
            _mysql = New MySqli(uri)
        Catch ex As Exception
            WriteError("Failed to connect to MySQL server:" &
                       Environment.NewLine & "  " & uri.Replace(opts.Password, "******") &
                       Environment.NewLine & "  " & ex.Message)
            Return
        End Try

        Dim counter = New Counter(_mysql)
        Dim vars = New VariablesReader(_mysql)
        Dim procMon = New ProcessMonitor(vars)
        Dim procList = New ProcessListReader(_mysql)
        Dim dashboard = New Dashboard(opts, vars)

        ' Establish a first baseline snapshot so the first rendered delta is sane.
        Try
            counter.PullNext()
        Catch ex As Exception
            WriteError("Failed to read initial GLOBAL STATUS:" & Environment.NewLine & "  " & ex.Message)
            Return
        End Try

        ' Enter the alternate screen and hide the cursor for a clean dashboard.
        Console.Out.Write(Ansi.AltScreenOn())
        Console.Out.Write(Ansi.HideCursor())

        Dim startTime = Date.Now

        While _running
            ' Sample MySQL counters.
            Dim c As Counter = Nothing
            Try
                c = counter.PullNext()
            Catch ex As Exception
                c = counter
            End Try

            ' Sample the mysqld process resources.
            Dim snap = procMon.Sample(opts.Interval)

            ' Sample slow queries.
            Dim slow As List(Of SlowQueryInfo) = Nothing
            Try
                slow = procList.GetSlowQueries(opts.SlowThreshold, opts.MaxSlowRows)
            Catch ex As Exception
                slow = New List(Of SlowQueryInfo)
            End Try

            ' Render and repaint the whole screen in one write (double buffered).
            Dim frame = dashboard.Render(c, snap, slow, startTime)
            Console.Out.Write(Ansi.Home())
            Console.Out.Write(Ansi.ClearDown())
            Console.Out.Write(frame)
            Console.Out.Flush()

            ' Wait for the next interval without blocking Ctrl+C.
            Dim waited = 0
            While _running AndAlso waited < CInt(opts.Interval * 1000)
                Dim stepMs = Math.Min(100, CInt(opts.Interval * 1000) - waited)
                Threading.Thread.Sleep(stepMs)
                waited += stepMs
            End While
        End While

        ' Restore terminal state.
        Console.Out.Write(Ansi.ShowCursor())
        Console.Out.Write(Ansi.AltScreenOff())
        Console.Out.Flush()
    End Sub

    Private Sub OnCancel(sender As Object, e As ConsoleCancelEventArgs)
        ' Mark for exit; the sleep loop will observe _running and break out.
        e.Cancel = True
        _running = False
    End Sub

    Private Sub WriteError(msg As String)
        Console.Error.WriteLine(Ansi.Fg(255, 95, 86) & msg & Ansi.Reset())
    End Sub

End Module
