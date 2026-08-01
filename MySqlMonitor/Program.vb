Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' MySqlMonitor - a realtime MySQL performance monitor for the console.
'''
''' Connects to a MySQL server, samples performance counters / process resources
''' on a fixed interval and renders a full-screen ANSI dashboard.
''' </summary>
Module Program

    ' Rolling history buffers that feed the dashboard sparklines.
    Private _history As MetricHistory = Nothing

    Private _running As Boolean = True
    Private _mysql As MySqli = Nothing

    Sub Main(args As String())
        Dim opts = MonitorOptions.Parse(args)

        If opts.ShowHelp Then
            MonitorOptions.PrintHelp()
            Return
        Else
            ' Cancel handling: restore the terminal on Ctrl+C instead of leaving it
            ' in the alternate-screen / hidden-cursor state.
            AddHandler Console.CancelKeyPress, AddressOf OnCancel
        End If

        Call RunLoop(opts)
    End Sub

    Private Sub RunLoop(opts As MonitorOptions)
        Dim uri As ConnectionUri = opts.BuildConnectionUri()

        ' The MySqli constructor establishes the connection (and validates it
        ' via a ping). A failure throws and is handled below.
        _mysql = New MySqli(uri)

        If _mysql.Ping < 0 Then
            WriteError("Failed to connect to MySQL server:" &
                      Environment.NewLine & "  " & uri.ToString.Replace(If(opts.Password.StringEmpty, "XXXXXXXXXXXXXX", opts.Password), "******") &
                      Environment.NewLine & "  " & _mysql.LastError?.Message)
            Return
        End If

        Dim counter = New Counter(_mysql)
        Dim vars = New VariablesReader(_mysql)
        Dim procMon = New ProcessMonitor(vars)
        Dim procList = New ProcessListReader(_mysql)

        ' Build the merged theme list (built-ins + any custom INI themes) and pick
        ' the initial theme from -theme / DefaultTheme (falls back to the first).
        Dim themes As List(Of Theme) = Theme.BuiltInThemes()
        If Not String.IsNullOrEmpty(opts.ThemeFile) Then
            themes.AddRange(Theme.FromIni(opts.ThemeFile))
        End If
        Dim themeIdx As Integer = Math.Max(0, themes.FindIndex(Function(t) t.Name.Equals(opts.DefaultTheme, StringComparison.OrdinalIgnoreCase)))
        Dim dashboard = New Dashboard(opts, vars, themes(themeIdx))
        _history = New MetricHistory()

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
                _history.Sample(c)
            Catch ex As Exception
                c = counter
            End Try

            ' Sample the mysqld process resources.
            Dim snap = procMon.Sample(opts.Interval)

            ' Sample slow queries.
            Dim slow As List(Of SlowQueryInfo) = Nothing
            Try
                slow = procList.GetSlowQueries(opts.MaxSlowRows)
            Catch ex As Exception
                slow = New List(Of SlowQueryInfo)
            End Try

            ' Render and repaint the whole screen in one write (double buffered).
            Dim frame = dashboard.Render(c, snap, slow, startTime, _history)
            Console.Out.Write(Ansi.Home())
            Console.Out.Write(Ansi.ClearDown())
            Console.Out.Write(frame)
            Console.Out.Flush()

            ' Wait for the next interval. While waiting we still poll for keys so
            ' that the "t" hotkey can cycle themes and repaint immediately.
            Dim waited = 0
            Dim dirty = False
            While _running AndAlso waited < CInt(opts.Interval * 1000)
                If Console.KeyAvailable Then
                    Dim key = Console.ReadKey(True)
                    If key.Key = ConsoleKey.T Then
                        ' Cycle to the next theme and repaint on the next pass.
                        themeIdx = (themeIdx + 1) Mod themes.Count
                        dashboard.SetTheme(themes(themeIdx))
                        dirty = True
                        Exit While
                    End If
                End If
                Dim stepMs = Math.Min(100, CInt(opts.Interval * 1000) - waited)
                Threading.Thread.Sleep(stepMs)
                waited += stepMs
            End While

            ' If a theme switch happened, loop back immediately to repaint.
            If dirty Then Continue While
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
