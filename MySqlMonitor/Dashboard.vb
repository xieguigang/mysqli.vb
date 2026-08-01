' Dashboard.vb
' Renders the full-screen ANSI dashboard from a snapshot of performance data.
' Pure string building -> caller writes it once (double-buffered) to the console.

Imports System.Text
Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter
Imports Oracle.LinuxCompatibility.MySQL.Uri

Public Class Dashboard

    Private ReadOnly _opts As MonitorOptions
    Private ReadOnly _vars As VariablesReader
    Private _width As Integer = 100

    ' Theme colors
    Private Shared ReadOnly C_BG As (r As Integer, g As Integer, b As Integer) = (11, 14, 20)         ' #0B0E14
    Private Shared ReadOnly C_PANEL As (r As Integer, g As Integer, b As Integer) = (18, 23, 34)      ' #121722
    Private Shared ReadOnly C_ACCENT As (r As Integer, g As Integer, b As Integer) = (63, 182, 201)   ' #3FB6C9
    Private Shared ReadOnly C_BORDER As (r As Integer, g As Integer, b As Integer) = (63, 182, 201)   ' #3FB6C9

    Public Sub New(opts As MonitorOptions, vars As VariablesReader)
        _opts = opts
        _vars = vars
        If Console.BufferWidth > 0 Then _width = Math.Min(Console.BufferWidth, 140)
    End Sub

    ' ---- Format helpers ----
    Private Shared Function FmtRate(v As Double) As String
        If v >= 1000000 Then Return (v / 1000000.0).ToString("F2") & "M"
        If v >= 1000 Then Return (v / 1000.0).ToString("F1") & "K"
        Return v.ToString("F0")
    End Function

    Private Shared Function FmtKBs(bytesPerSec As Double) As String
        Dim kb As Double = bytesPerSec / 1024.0
        If kb >= 1024 Then Return (kb / 1024.0).ToString("F2") & " MB/s"
        Return kb.ToString("F1") & " KB/s"
    End Function

    Private Shared Function FmtBytes(b As Long) As String
        Dim d As Double = CDbl(b)
        If d >= 1024.0 * 1024.0 * 1024.0 Then Return (d / (1024.0 * 1024.0 * 1024.0)).ToString("F2") & " GB"
        If d >= 1024.0 * 1024.0 Then Return (d / (1024.0 * 1024.0)).ToString("F2") & " MB"
        If d >= 1024.0 Then Return (d / 1024.0).ToString("F2") & " KB"
        Return d.ToString("F0") & " B"
    End Function

    Private Shared Function FmtUptime(ts As TimeSpan) As String
        Return String.Format("{0:D2}:{1:D2}:{2:D2}", CInt(ts.TotalHours), ts.Minutes, ts.Seconds)
    End Function

    ' Right-pad / truncate a string to exact width (preserving non-escape content length).
    Private Shared Function Pad(s As String, width As Integer) As String
        If s.Length >= width Then Return s.Substring(0, width)
        Return s & New String(" "c, width - s.Length)
    End Function

    ' ---- Main render ----
    Public Function Render(counter As Counter, proc As ProcessSnapshot, slow As List(Of SlowQueryInfo), startTime As Date, history As MetricHistory) As String
        Dim sb As New StringBuilder()
        Dim w As Integer = _width

        sb.Append(Ansi.HideCursor())
        sb.Append(Ansi.Home())
        sb.Append(Ansi.Bg(C_BG.r, C_BG.g, C_BG.b))
        sb.Append(Ansi.FgReset())
        sb.Append(Ansi.ClearDown())

        ' ===== Top status bar =====
        RenderHeader(sb, startTime, w)

        ' Compute a balanced layout: left column (throughput + io) and right column (buffer pool + connections)
        Dim colGap As Integer = 2
        Dim colW As Integer = (w - colGap) \ 2
        If colW < 38 Then colW = w ' single column fallback for narrow terminals

        Dim left As New StringBuilder()
        RenderThroughput(left, counter, colW, history)
        RenderIoNetwork(left, counter, colW, history)

        Dim right As New StringBuilder()
        RenderBufferPool(right, counter, colW, history)
        RenderConnections(right, counter, proc, colW)

        If colW >= w Then
            sb.Append(left.ToString())
            sb.Append(right.ToString())
        Else
            ' Side-by-side: merge line by line
            MergeColumns(sb, left.ToString(), right.ToString(), colW, colGap, w)
        End If

        ' ===== Slow queries (full width) =====
        RenderSlowQueries(sb, slow, w)

        sb.Append(Ansi.Reset())
        sb.Append(Ansi.ShowCursor())
        Return sb.ToString()
    End Function

    ' ---------- Header ----------
    Private Sub RenderHeader(sb As StringBuilder, startTime As Date, w As Integer)
        Dim title As String = Ansi.Bold(" MySqlMonitor ") & Ansi.FgMuted() & " real-time performance dashboard"
        Dim uri As ConnectionUri = Nothing
        Dim target As String = ""
        Try
            uri = ConnectionUri.TryParsing(_opts.BuildConnectionUri())
            target = uri.IPAddress & ":" & uri.Port
        Catch
            target = "(unknown)"
        End Try
        Dim run As String = FmtUptime(DateTime.Now - startTime)
        Dim nowStr As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        Dim right As String = String.Format("target {0}  uptime {1}  interval {2}s  {3}",
                                            target, run, _opts.Interval, nowStr)

        sb.Append(Ansi.Bg(C_PANEL.r, C_PANEL.g, C_PANEL.b))
        sb.Append(Ansi.Fg(C_ACCENT.r, C_ACCENT.g, C_ACCENT.b))
        sb.Append(Pad(title & Ansi.Reset() & Ansi.FgMuted() & "  " & right, w))
        sb.Append(Ansi.Reset())
        sb.AppendLine()
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append(Ansi.HLine(w))
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    ' ---------- Panel frame ----------
    Private Sub PanelStart(sb As StringBuilder, title As String, w As Integer)
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("╭" & Ansi.HLine(w - 2) & "╮")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
        ' Title row with vertical borders
        Dim inner As String = " " & Ansi.Bold(title) & Ansi.FgMuted()
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.Append(Pad(inner, w - 2))
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    Private Sub PanelEnd(sb As StringBuilder, w As Integer)
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("╰" & Ansi.HLine(w - 2) & "╯")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    Private Sub PanelLine(sb As StringBuilder, text As String, w As Integer)
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.Append(Pad(text, w - 2))
        sb.Append(Ansi.Fg(C_BORDER.r, C_BORDER.g, C_BORDER.b))
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    ' ---------- Throughput ----------
    Private Sub RenderThroughput(sb As StringBuilder, c As Counter, w As Integer)
        PanelStart(sb, "Query Throughput (ops/s)", w)
        Dim items As New List(Of (String, Double, String)) From {
            ("SELECT", c.NumOfSelect, Ansi.Fg(61, 214, 140)),
            ("INSERT", c.NumOfInsert, Ansi.Fg(63, 182, 201)),
            ("UPDATE", c.NumOfUpdate, Ansi.Fg(63, 182, 201)),
            ("DELETE", c.NumOfDelete, Ansi.Fg(242, 92, 84)),
            ("CREATE", c.NumOfCreate, Ansi.Fg(242, 193, 78)),
            ("ALTER ", c.NumOfAlter, Ansi.Fg(242, 193, 78)),
            ("DROP  ", c.NumOfDrop, Ansi.Fg(242, 92, 84))
        }
        Dim maxV As Double = 0
        For Each it In items
            If it.Item2 > maxV Then maxV = it.Item2
        Next
        Dim barW As Integer = Math.Max(6, w - 26)
        Dim peakName As String = ""
        Dim peakV As Double = -1
        For Each it In items
            If it.Item2 > peakV Then
                peakV = it.Item2
                peakName = it.Item1.Trim()
            End If
        Next
        For Each it In items
            Dim ratio As Double = If(maxV > 0, it.Item2 / maxV, 0)
            Dim bar As String = Ansi.Bar(ratio, barW, it.Item3)
            Dim lbl As String = Ansi.FgMuted() & it.Item1 & " " & Ansi.Reset() & it.Item3 & Ansi.Bold(FmtRate(it.Item2).PadLeft(8)) & Ansi.Reset()
            Dim isPeak As Boolean = (it.Item1.Trim() = peakName)
            Dim line As String = " " & lbl & " " & bar
            If isPeak Then line = " " & Ansi.Inverse(lbl.Trim()) & " " & bar
            PanelLine(sb, line, w)
        Next
        PanelEnd(sb, w)
    End Sub

    ' ---------- I/O & Network ----------
    Private Sub RenderIoNetwork(sb As StringBuilder, c As Counter, w As Integer)
        PanelStart(sb, "I/O & Network (KB/s)", w)
        Dim rows As New List(Of (String, String, String)) From {
            ("Data Read ", Ansi.Fg(61, 214, 140), FmtKBs(c.Innodb_data_read)),
            ("Data Write", Ansi.Fg(63, 182, 201), FmtKBs(c.Innodb_data_written)),
            ("Net Recv  ", Ansi.Fg(63, 182, 201), FmtKBs(c.Bytes_received)),
            ("Net Send  ", Ansi.Fg(61, 214, 140), FmtKBs(c.Bytes_sent))
        }
        For Each r In rows
            Dim val As String = r.Item3.PadLeft(12)
            Dim line As String = " " & Ansi.FgMuted() & r.Item1 & " " & Ansi.Reset() & r.Item2 & Ansi.Bold(val) & Ansi.Reset()
            PanelLine(sb, line, w)
        Next
        PanelEnd(sb, w)
    End Sub

    ' ---------- Buffer Pool ----------
    Private Sub RenderBufferPool(sb As StringBuilder, c As Counter, w As Integer)
        PanelStart(sb, "InnoDB Buffer Pool", w)
        Dim hit As Double = c.BufferPoolHitRate * 100.0
        Dim hitColor As String = Ansi.GradeValue(hit, 90, 95)
        Dim hitBar As String = Ansi.Bar(Math.Min(1, c.BufferPoolHitRate), w - 26, hitColor)
        PanelLine(sb, " " & Ansi.FgMuted() & "Hit Rate " & Ansi.Reset() & hitColor & Ansi.Bold(hit.ToString("F2").PadLeft(7) & "%") & Ansi.Reset() & " " & hitBar, w)

        Dim usage As Double = c.BufferPoolUsage * 100.0
        Dim usageColor As String = Ansi.GradeValue(usage, 75, 90)
        Dim usageBar As String = Ansi.Bar(Math.Min(1, c.BufferPoolUsage), w - 28, usageColor)
        PanelLine(sb, " " & Ansi.FgMuted() & "Usage   " & Ansi.Reset() & usageColor & Ansi.Bold(usage.ToString("F1").PadLeft(7) & "%") & Ansi.Reset() & " " & usageBar, w)

        Dim poolSize As String = FmtBytes(_vars.GetInnodbBufferPoolSize())
        PanelLine(sb, " " & Ansi.FgMuted() & "Size    " & Ansi.Reset() & Ansi.FgReset() & poolSize.PadLeft(w - 11), w)

        Dim reqs As String = FmtRate(c.Innodb_buffer_pool_read_requests) & " r/s  " & FmtRate(c.Innodb_buffer_pool_write_requests) & " w/s"
        PanelLine(sb, " " & Ansi.FgMuted() & "Requests" & Ansi.Reset() & " " & reqs, w)

        Dim disk As String = FmtRate(c.Innodb_buffer_pool_disk_reads) & " disk read/s"
        PanelLine(sb, " " & Ansi.FgMuted() & "Disk Rds" & Ansi.Reset() & " " & disk, w)

        PanelEnd(sb, w)
    End Sub

    ' ---------- Connections & Process ----------
    Private Sub RenderConnections(sb As StringBuilder, c As Counter, proc As ProcessSnapshot, w As Integer)
        PanelStart(sb, "Connections & mysqld Process", w)
        PanelLine(sb, " " & Ansi.FgMuted() & "Threads Connected " & Ansi.Reset() & Ansi.FgReset() & Ansi.Bold(c.ClientConnections.ToString().PadLeft(w - 19)), w)
        PanelLine(sb, " " & Ansi.FgMuted() & "Threads Running   " & Ansi.Reset() & Ansi.Fg(242, 193, 78) & Ansi.Bold(c.ThreadsRunning.ToString().PadLeft(w - 19)), w)
        PanelLine(sb, " " & Ansi.FgMuted() & "Slow Queries     " & Ansi.Reset() & Ansi.Fg(242, 92, 84) & Ansi.Bold(c.NumOfSlow.ToString().PadLeft(w - 19)), w)

        If proc IsNot Nothing AndAlso proc.Available Then
            Dim cpuColor As String = Ansi.GradeValue(proc.CpuPercent, 50, 80)
            PanelLine(sb, " " & Ansi.FgMuted() & "mysqld CPU       " & Ansi.Reset() & cpuColor & Ansi.Bold(proc.CpuPercent.ToString("F1").PadLeft(w - 19) & "%"), w)
            Dim memColor As String = Ansi.GradeValue(CDbl(proc.MemoryBytes) / (1024.0 * 1024.0 * 1024.0), 0.6, 0.85)
            PanelLine(sb, " " & Ansi.FgMuted() & "mysqld Memory    " & Ansi.Reset() & memColor & Ansi.Bold(FmtBytes(proc.MemoryBytes).PadLeft(w - 19)), w)
            PanelLine(sb, " " & Ansi.FgMuted() & "mysqld Threads   " & Ansi.Reset() & Ansi.FgReset() & Ansi.Bold(proc.ThreadCount.ToString().PadLeft(w - 19)), w)
        Else
            Dim note As String = If(proc IsNot Nothing, proc.Note, "unavailable")
            PanelLine(sb, " " & Ansi.FgMuted() & "mysqld process   " & Ansi.Reset() & Ansi.FgWarn() & "N/A" & Ansi.FgMuted() & " (" & note & ")", w)
        End If
        PanelEnd(sb, w)
    End Sub

    ' ---------- Slow queries ----------
    Private Sub RenderSlowQueries(sb As StringBuilder, slow As List(Of SlowQueryInfo), w As Integer)
        Dim title As String = "Current Slow Queries (threshold " & _opts.SlowThreshold.ToString() & "s, top " & _opts.MaxSlowRows.ToString() & ")"
        PanelStart(sb, title, w)

        If slow Is Nothing OrElse slow.Count = 0 Then
            PanelLine(sb, " " & Ansi.FgOk() & "No slow queries running." & Ansi.Reset(), w)
            PanelEnd(sb, w)
            Return
        End If

        ' Column widths
        Dim cId As Integer = 5
        Dim cUser As Integer = 12
        Dim cHost As Integer = 16
        Dim cDb As Integer = 12
        Dim cState As Integer = 12
        Dim cTime As Integer = 6
        Dim fixedW As Integer = cId + cUser + cHost + cDb + cState + cTime + 6
        Dim cSql As Integer = Math.Max(10, w - 2 - fixedW)
        If cSql < 10 Then
            ' shrink others
            cHost = 10 : cDb = 8 : cState = 8
            fixedW = cId + cUser + cHost + cDb + cState + cTime + 6
            cSql = Math.Max(8, w - 2 - fixedW)
        End If

        Dim header As String = " " &
            Ansi.FgMuted() & "ID".PadRight(cId) & " " &
            "USER".PadRight(cUser) & " " &
            "HOST".PadRight(cHost) & " " &
            "DB".PadRight(cDb) & " " &
            "STATE".PadRight(cState) & " " &
            "TIME".PadRight(cTime) & " " &
            "SQL" & Ansi.Reset()
        PanelLine(sb, header, w)

        For Each q In slow
            Dim timeColor As String = Ansi.GradeValue(CDbl(q.TimeSec), _opts.SlowThreshold, _opts.SlowThreshold * 2)
            Dim idS As String = Trunc(q.Id.ToString(), cId)
            Dim userS As String = Trunc(q.User, cUser)
            Dim hostS As String = Trunc(q.Host, cHost)
            Dim dbS As String = Trunc(If(q.Database, ""), cDb)
            Dim stateS As String = Trunc(q.State, cState)
            Dim timeS As String = Trunc(q.TimeSec.ToString() & "s", cTime)
            Dim sqlS As String = Trunc(If(q.Sql, ""), cSql)
            Dim line As String = " " &
                Ansi.FgReset() & idS.PadRight(cId) & " " &
                Ansi.Fg(63, 182, 201) & userS.PadRight(cUser) & " " & Ansi.Reset() &
                Ansi.FgMuted() & hostS.PadRight(cHost) & " " & Ansi.Reset() &
                Ansi.FgReset() & dbS.PadRight(cDb) & " " &
                Ansi.FgMuted() & stateS.PadRight(cState) & " " & Ansi.Reset() &
                timeColor & timeS.PadRight(cTime) & " " & Ansi.Reset() &
                Ansi.FgReset() & sqlS
            PanelLine(sb, line, w)
        Next
        PanelEnd(sb, w)
    End Sub

    Private Shared Function Trunc(s As String, width As Integer) As String
        If s Is Nothing Then s = ""
        s = s.Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbTab, " ")
        If s.Length > width Then Return s.Substring(0, width - 1) & "…"
        Return s.PadRight(width)
    End Function

    ' ---------- Two-column merge ----------
    Private Sub MergeColumns(sb As StringBuilder, left As String, right As String, colW As Integer, gap As Integer, totalW As Integer)
        Dim lLines As String() = left.Split({vbCrLf, vbLf}, StringSplitOptions.None)
        Dim rLines As String() = right.Split({vbCrLf, vbLf}, StringSplitOptions.None)
        Dim n As Integer = Math.Max(lLines.Length, rLines.Length)
        For i As Integer = 0 To n - 1
            Dim l As String = If(i < lLines.Length, StripNewline(lLines(i)), "")
            Dim r As String = If(i < rLines.Length, StripNewline(rLines(i)), "")
            sb.Append(PadRaw(l, colW))
            sb.Append(New String(" "c, gap))
            sb.Append(PadRaw(r, colW))
            sb.AppendLine()
        Next
    End Sub

    Private Shared Function StripNewline(s As String) As String
        Return s.Replace(vbCr, "").Replace(vbLf, "")
    End Function

    Private Shared Function PadRaw(s As String, width As Integer) As String
        ' s may contain ANSI codes; measure visible length
        Dim vis As Integer = VisibleLen(s)
        If vis >= width Then Return TruncateVisible(s, width)
        Return s & New String(" "c, width - vis)
    End Function

    Private Shared Function VisibleLen(s As String) As Integer
        Dim len As Integer = 0
        Dim i As Integer = 0
        While i < s.Length
            If s(i) = ChrW(&H1B) Then
                ' skip escape sequence until 'm' or letter command
                i += 1
                While i < s.Length AndAlso Not Char.IsLetter(s(i))
                    i += 1
                End While
                If i < s.Length Then i += 1
            Else
                len += 1
                i += 1
            End If
        End While
        Return len
    End Function

    Private Shared Function TruncateVisible(s As String, width As Integer) As String
        Dim sb As New StringBuilder()
        Dim vis As Integer = 0
        Dim i As Integer = 0
        While i < s.Length AndAlso vis < width
            If s(i) = ChrW(&H1B) Then
                sb.Append(s(i))
                i += 1
                While i < s.Length AndAlso Not Char.IsLetter(s(i))
                    sb.Append(s(i))
                    i += 1
                End While
                If i < s.Length Then sb.Append(s(i)) : i += 1
            Else
                sb.Append(s(i))
                vis += 1
                i += 1
            End If
        End While
        Return sb.ToString()
    End Function

    Private Shared Function StripEscape(s As String) As String
        Dim sb As New StringBuilder()
        Dim i As Integer = 0
        While i < s.Length
            If s(i) = ChrW(&H1B) Then
                i += 1
                While i < s.Length AndAlso Not Char.IsLetter(s(i))
                    i += 1
                End While
                If i < s.Length Then i += 1
            Else
                sb.Append(s(i))
                i += 1
            End If
        End While
        Return sb.ToString()
    End Function

End Class
