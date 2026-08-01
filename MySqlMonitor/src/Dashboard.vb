' Dashboard.vb
' Renders the full-screen ANSI dashboard from a snapshot of performance data.
' Pure string building -> caller writes it once (double-buffered) to the console.

Imports System.Text
Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter
Imports Oracle.LinuxCompatibility.MySQL.Uri

Public Class Dashboard

    Private ReadOnly _opts As MonitorOptions
    Private ReadOnly _vars As VariablesReader
    Private _theme As Theme
    Private _width As Integer = 100

    Public Sub New(opts As MonitorOptions, vars As VariablesReader, theme As Theme)
        _opts = opts
        _vars = vars
        _theme = If(theme, Theme.Ocean())

        ' Fill the terminal width. Prefer the buffer width (which on wide /
        ' ultra-wide monitors can be very large), falling back to the window
        ' width, then to a sane default. No artificial 140-column cap so the
        ' dashboard spans ultra-wide ("带鱼屏") terminals.
        Dim bufW As Integer = Console.BufferWidth
        If bufW <= 0 Then bufW = Console.WindowWidth
        If bufW <= 0 Then bufW = 80
        _width = Math.Max(80, bufW)
    End Sub

    ' Swap the active theme at runtime (hotkey cycling) without rebuilding the
    ' dashboard. The next Render() call picks up the new colors.
    Public Sub SetTheme(theme As Theme)
        If theme IsNot Nothing Then _theme = theme
    End Sub

    Public Function CurrentTheme() As Theme
        Return _theme
    End Function

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

    ' Right-pad / truncate a string to exact VISIBLE width. The string may
    ' contain ANSI escape sequences; those must NOT count toward the length,
    ' otherwise padding pushes the right border "│" out of alignment and
    ' truncating on s.Length can cut an escape sequence in half (yielding
    ' stray "Esc[" garbage). We reuse VisibleLen / TruncateVisible.
    Private Shared Function Pad(s As String, width As Integer) As String
        Dim vis As Integer = VisibleLen(s)
        If vis >= width Then Return TruncateVisible(s, width)
        Return s & New String(" "c, width - vis)
    End Function

    ' ---- Main render ----
    Public Function Render(counter As Counter, proc As ProcessSnapshot, slow As List(Of SlowQueryInfo), startTime As Date, history As MetricHistory) As String
        Dim sb As New StringBuilder()
        Dim w As Integer = _width

        sb.Append(Ansi.HideCursor())
        sb.Append(Ansi.Home())
        sb.Append(_theme.FgBG())
        sb.Append(Ansi.FgReset())
        sb.Append(Ansi.ClearDown())

        ' ===== Top status bar =====
        RenderHeader(sb, startTime, w)

        ' Compute a balanced layout: left column (throughput + io) and right column (buffer pool + connections)
        Dim colGap As Integer = 2
        Dim colW As Integer = (w - colGap) \ 2
        If colW < 38 Then colW = w ' single column fallback for narrow terminals

        Dim left As New StringBuilder()
        RenderThroughput(left, counter, colW)
        RenderIoNetwork(left, counter, colW)

        Dim right As New StringBuilder()
        RenderBufferPool(right, counter, colW)
        RenderConnections(right, counter, proc, colW)

        If colW >= w Then
            sb.Append(left.ToString())
            sb.Append(right.ToString())
        Else
            ' Side-by-side: merge line by line
            MergeColumns(sb, left.ToString(), right.ToString(), colW, colGap, w)
        End If

        ' ===== History trends (full width) =====
        RenderTrends(sb, counter, history, w)

        ' ===== Slow queries (full width, rendered last) =====
        RenderSlowQueries(sb, slow, w)

        sb.Append(Ansi.Reset())
        sb.Append(Ansi.ShowCursor())
        Return sb.ToString()
    End Function

    ' ---------- Header ----------
    Private Sub RenderHeader(sb As StringBuilder, startTime As Date, w As Integer)
        Dim title As String = Ansi.Bold(" MySqlMonitor ") & _theme.FgMuted() & " real-time performance dashboard"
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

        sb.Append(_theme.FgPanel())
        sb.Append(_theme.FgAccent())
        sb.Append(Pad(title & Ansi.Reset() & _theme.FgMuted() & "  " & right, w))
        sb.Append(Ansi.Reset())
        sb.AppendLine()
        sb.Append(_theme.FgBorder())
        sb.Append(Ansi.HLine(w))
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    ' ---------- Panel frame ----------
    Private Sub PanelStart(sb As StringBuilder, title As String, w As Integer)
        sb.Append(_theme.FgBorder())
        sb.Append("╭" & Ansi.HLine(w - 2) & "╮")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
        ' Title row with vertical borders
        Dim inner As String = " " & Ansi.Bold(title) & _theme.FgMuted()
        sb.Append(_theme.FgBorder())
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.Append(Pad(inner, w - 2))
        sb.Append(_theme.FgBorder())
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    Private Sub PanelEnd(sb As StringBuilder, w As Integer)
        sb.Append(_theme.FgBorder())
        sb.Append("╰" & Ansi.HLine(w - 2) & "╯")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    Private Sub PanelLine(sb As StringBuilder, text As String, w As Integer)
        sb.Append(_theme.FgBorder())
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.Append(Pad(text, w - 2))
        sb.Append(_theme.FgBorder())
        sb.Append("│")
        sb.Append(Ansi.Reset())
        sb.AppendLine()
    End Sub

    ' ---------- Throughput ----------
    Private Sub RenderThroughput(sb As StringBuilder, c As Counter, w As Integer)
        PanelStart(sb, "Query Throughput (ops/s)", w)
        Dim items As New List(Of (String, Double, String)) From {
            ("SELECT", c.NumOfSelect, _theme.FgSelect()),
            ("INSERT", c.NumOfInsert, _theme.FgInsert()),
            ("UPDATE", c.NumOfUpdate, _theme.FgUpdate()),
            ("DELETE", c.NumOfDelete, _theme.FgDelete()),
            ("CREATE", c.NumOfCreate, _theme.FgCreate()),
            ("ALTER ", c.NumOfAlter, _theme.FgAlter()),
            ("DROP  ", c.NumOfDrop, _theme.FgDrop())
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
            Dim lbl As String = _theme.FgMuted() & it.Item1 & " " & Ansi.Reset() & it.Item3 & Ansi.Bold(FmtRate(it.Item2).PadLeft(8)) & Ansi.Reset()
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
            ("Data Read ", _theme.FgSelect(), FmtKBs(c.Innodb_data_read)),
            ("Data Write", _theme.FgUpdate(), FmtKBs(c.Innodb_data_written)),
            ("Net Recv  ", _theme.FgUser(), FmtKBs(c.Bytes_received)),
            ("Net Send  ", _theme.FgSelect(), FmtKBs(c.Bytes_sent))
        }
        For Each r In rows
            Dim val As String = r.Item3.PadLeft(12)
            Dim line As String = " " & _theme.FgMuted() & r.Item1 & " " & Ansi.Reset() & r.Item2 & Ansi.Bold(val) & Ansi.Reset()
            PanelLine(sb, line, w)
        Next
        PanelEnd(sb, w)
    End Sub

    ' ---------- Buffer Pool ----------
    Private Sub RenderBufferPool(sb As StringBuilder, c As Counter, w As Integer)
        PanelStart(sb, "InnoDB Buffer Pool", w)
        Dim hit As Double = c.BufferPoolHitRate * 100.0
        Dim hitColor As String = _theme.Grade(hit, 90, 95)
        Dim hitBar As String = Ansi.Bar(Math.Min(1, c.BufferPoolHitRate), w - 26, hitColor)
        PanelLine(sb, " " & _theme.FgMuted() & "Hit Rate " & Ansi.Reset() & hitColor & Ansi.Bold(hit.ToString("F2").PadLeft(7) & "%") & Ansi.Reset() & " " & hitBar, w)

        Dim usage As Double = c.BufferPoolUsage * 100.0
        Dim usageColor As String = _theme.Grade(usage, 75, 90)
        Dim usageBar As String = Ansi.Bar(Math.Min(1, c.BufferPoolUsage), w - 28, usageColor)
        PanelLine(sb, " " & _theme.FgMuted() & "Usage   " & Ansi.Reset() & usageColor & Ansi.Bold(usage.ToString("F1").PadLeft(7) & "%") & Ansi.Reset() & " " & usageBar, w)

        Dim poolSize As String = FmtBytes(_vars.GetInnodbBufferPoolSize())
        PanelLine(sb, " " & _theme.FgMuted() & "Size    " & Ansi.Reset() & Ansi.FgReset() & poolSize.PadLeft(w - 11), w)

        Dim reqs As String = FmtRate(c.Innodb_buffer_pool_read_requests) & " r/s  " & FmtRate(c.Innodb_buffer_pool_write_requests) & " w/s"
        PanelLine(sb, " " & _theme.FgMuted() & "Requests" & Ansi.Reset() & " " & reqs, w)

        Dim disk As String = FmtRate(c.Innodb_buffer_pool_disk_reads) & " disk read/s"
        PanelLine(sb, " " & _theme.FgMuted() & "Disk Rds" & Ansi.Reset() & " " & disk, w)

        PanelEnd(sb, w)
    End Sub

    ' ---------- Connections & Process ----------
    Private Sub RenderConnections(sb As StringBuilder, c As Counter, proc As ProcessSnapshot, w As Integer)
        PanelStart(sb, "Connections & mysqld Process", w)
        PanelLine(sb, " " & _theme.FgMuted() & "Threads Connected " & Ansi.Reset() & Ansi.FgReset() & Ansi.Bold(c.ClientConnections.ToString().PadLeft(w - 19)), w)
        PanelLine(sb, " " & _theme.FgMuted() & "Threads Running   " & Ansi.Reset() & _theme.FgWarn() & Ansi.Bold(c.ThreadsRunning.ToString().PadLeft(w - 19)), w)
        PanelLine(sb, " " & _theme.FgMuted() & "Slow Queries     " & Ansi.Reset() & _theme.FgDanger() & Ansi.Bold(c.NumOfSlow.ToString().PadLeft(w - 19)), w)

        If proc IsNot Nothing AndAlso proc.Available Then
            Dim cpuColor As String = _theme.Grade(proc.CpuPercent, 50, 80)
            PanelLine(sb, " " & _theme.FgMuted() & "mysqld CPU       " & Ansi.Reset() & cpuColor & Ansi.Bold(proc.CpuPercent.ToString("F1").PadLeft(w - 19) & "%"), w)
            Dim memColor As String = _theme.Grade(CDbl(proc.MemoryBytes) / (1024.0 * 1024.0 * 1024.0), 0.6, 0.85)
            PanelLine(sb, " " & _theme.FgMuted() & "mysqld Memory    " & Ansi.Reset() & memColor & Ansi.Bold(FmtBytes(proc.MemoryBytes).PadLeft(w - 19)), w)
            PanelLine(sb, " " & _theme.FgMuted() & "mysqld Threads   " & Ansi.Reset() & Ansi.FgReset() & Ansi.Bold(proc.ThreadCount.ToString().PadLeft(w - 19)), w)
        Else
            Dim note As String = If(proc IsNot Nothing, proc.Note, "unavailable")
            PanelLine(sb, " " & _theme.FgMuted() & "mysqld process   " & Ansi.Reset() & _theme.FgWarn() & "N/A" & _theme.FgMuted() & " (" & note & ")", w)
        End If
        PanelEnd(sb, w)
    End Sub

    ' ---------- Slow queries ----------
    Private Sub RenderSlowQueries(sb As StringBuilder, slow As List(Of SlowQueryInfo), w As Integer)
        Dim title As String = "Current Slow Queries (threshold " & _opts.SlowThreshold.ToString() & "s, top " & _opts.MaxSlowRows.ToString() & ")"
        PanelStart(sb, title, w)

        If slow Is Nothing OrElse slow.Count = 0 Then
            PanelLine(sb, " " & _theme.FgOk() & "No slow queries running." & Ansi.Reset(), w)
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
            _theme.FgMuted() & "ID".PadRight(cId) & " " &
            "USER".PadRight(cUser) & " " &
            "HOST".PadRight(cHost) & " " &
            "DB".PadRight(cDb) & " " &
            "STATE".PadRight(cState) & " " &
            "TIME".PadRight(cTime) & " " &
            "SQL" & Ansi.Reset()
        PanelLine(sb, header, w)

        For Each q In slow
            Dim timeColor As String = _theme.Grade(CDbl(q.TimeSec), _opts.SlowThreshold, _opts.SlowThreshold * 2)
            Dim idS As String = Trunc(q.Id.ToString(), cId)
            Dim userS As String = Trunc(q.User, cUser)
            Dim hostS As String = Trunc(q.Host, cHost)
            Dim dbS As String = Trunc(If(q.Database, ""), cDb)
            Dim stateS As String = Trunc(q.State, cState)
            Dim timeS As String = Trunc(q.TimeSec.ToString() & "s", cTime)
            Dim sqlS As String = Trunc(If(q.Sql, ""), cSql)
            Dim line As String = " " &
                Ansi.FgReset() & idS.PadRight(cId) & " " &
                _theme.FgUser() & userS.PadRight(cUser) & " " & Ansi.Reset() &
                _theme.FgMuted() & hostS.PadRight(cHost) & " " & Ansi.Reset() &
                Ansi.FgReset() & dbS.PadRight(cDb) & " " &
                _theme.FgMuted() & stateS.PadRight(cState) & " " & Ansi.Reset() &
                timeColor & timeS.PadRight(cTime) & " " & Ansi.Reset() &
                Ansi.FgReset() & sqlS
            PanelLine(sb, line, w)
        Next
        PanelEnd(sb, w)
    End Sub

    ' ---------- History Trends (sparkline) ----------
    ' Full-width panel aggregating every metric sequence collected by MetricHistory
    ' into compact sparkline trend charts. Color is graded by the latest value so a
    ' rising/dangerous trend stands out (red = danger, yellow = warn, green = ok).
    Private Sub RenderTrends(sb As StringBuilder, c As Counter, history As MetricHistory, w As Integer)
        Dim title As String = "History Trends (last " & MetricHistory.Capacity.ToString() & " samples)"
        PanelStart(sb, title, w)

        ' Reserve room for label + current value; sparkline takes the rest.
        Dim labelW As Integer = 12
        Dim valW As Integer = 13
        Dim sparkW As Integer = w - 2 - labelW - valW
        If sparkW < 8 Then sparkW = 8

        ' Throughput group (ops/s) — counter class, graded by absolute rate.
        RenderTrendGroup(sb, "Query Throughput (ops/s)", w, {
            ("SELECT", c.NumOfSelect, history.SelectSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("INSERT", c.NumOfInsert, history.InsertSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("UPDATE", c.NumOfUpdate, history.UpdateSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("DELETE", c.NumOfDelete, history.DeleteSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("CREATE", c.NumOfCreate, history.CreateSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("ALTER ", c.NumOfAlter, history.AlterSeries, AddressOf FmtRate, AddressOf GradeThroughput),
            ("DROP  ", c.NumOfDrop, history.DropSeries, AddressOf FmtRate, AddressOf GradeThroughput)
        }, labelW, valW, sparkW)

        ' I/O & Network group (KB/s) — series are already in KB/s.
        RenderTrendGroup(sb, "I/O & Network (KB/s)", w, {
            ("Data Read ", c.Innodb_data_read, history.ReadKbSeries, AddressOf FmtKBs, AddressOf GradeIo),
            ("Data Write", c.Innodb_data_written, history.WriteKbSeries, AddressOf FmtKBs, AddressOf GradeIo),
            ("Net Recv  ", c.Bytes_received, history.RecvKbSeries, AddressOf FmtKBs, AddressOf GradeIo),
            ("Net Send  ", c.Bytes_sent, history.SentKbSeries, AddressOf FmtKBs, AddressOf GradeIo)
        }, labelW, valW, sparkW)

        ' InnoDB Buffer Pool group — ratio class, reuse existing thresholds.
        RenderTrendGroup(sb, "InnoDB Buffer Pool", w, {
            ("Hit Rate ", c.BufferPoolHitRate * 100.0, history.HitSeries, AddressOf FmtPct, Function(v) _theme.Grade(v, 90, 95)),
            ("Usage    ", c.BufferPoolUsage * 100.0, history.UsageSeries, AddressOf FmtPct, Function(v) _theme.Grade(v, 75, 90))
        }, labelW, valW, sparkW)

        ' Connections group.
        RenderTrendGroup(sb, "Connections", w, {
            ("Threads   ", CDbl(c.ClientConnections), history.ConnsSeries, AddressOf FmtInt, AddressOf GradeConns)
        }, labelW, valW, sparkW)

        PanelEnd(sb, w)
    End Sub

    ' Delegate: grade a current value into an ANSI color prefix.
    Private Delegate Function GradeDel(v As Double) As String

    ' Render one metric row: " label  value  sparkline".
    Private Sub RenderTrendRow(sb As StringBuilder, label As String, value As Double, series As Double(),
                               fmt As Func(Of Double, String), grade As GradeDel, labelW As Integer, valW As Integer, sparkW As Integer, w As Integer)
        Dim color As String = grade(value)
        Dim spark As String = Ansi.Sparkline(series, sparkW, color)
        Dim valStr As String = fmt(value).PadLeft(valW)
        Dim line As String = " " & _theme.FgMuted() & label.PadRight(labelW) & Ansi.Reset() &
                             color & Ansi.Bold(valStr) & Ansi.Reset() & " " & spark
        PanelLine(sb, line, w)
    End Sub

    ' Render a titled group of trend rows inside the trends panel.
    Private Sub RenderTrendGroup(sb As StringBuilder, groupTitle As String, w As Integer,
                                 rows As (String, Double, Double(), Func(Of Double, String), GradeDel)(),
                                 labelW As Integer, valW As Integer, sparkW As Integer)
        PanelLine(sb, " " & _theme.FgAccent() & Ansi.Bold(groupTitle) & Ansi.Reset(), w)
        For Each r In rows
            RenderTrendRow(sb, r.Item1, r.Item2, r.Item3, r.Item4, r.Item5, labelW, valW, sparkW, w)
        Next
    End Sub

    ' ---- value graders (higher = worse) ----
    Private Function GradeThroughput(v As Double) As String
        Return _theme.Grade(v, 5000, 20000)
    End Function
    Private Function GradeIo(v As Double) As String
        ' v is bytes/s (from counter); grade on MB/s.
        Return _theme.Grade(v / (1024.0 * 1024.0), 50, 200)
    End Function
    Private Function GradeConns(v As Double) As String
        Return _theme.Grade(v, 80, 150)
    End Function

    ' ---- extra formatters ----
    Private Shared Function FmtPct(v As Double) As String
        Return v.ToString("F1") & "%"
    End Function
    Private Shared Function FmtInt(v As Double) As String
        Return v.ToString("F0")
    End Function

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
