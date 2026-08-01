Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' Command line options for the MySqlMonitor console tool.
''' </summary>
Public Class MonitorOptions

    ''' <summary>MySQL server host.</summary>
    Public Property Host As String = "localhost"

    ''' <summary>MySQL server port.</summary>
    Public Property Port As Integer = 3306

    ''' <summary>MySQL user name.</summary>
    Public Property User As String = "root"

    ''' <summary>MySQL password.</summary>
    Public Property Password As String = ""

    ''' <summary>Default database (optional).</summary>
    Public Property Database As String = ""

    ''' <summary>Refresh interval in seconds.</summary>
    Public Property Interval As Double = 2.0

    ''' <summary>Slow query threshold in seconds (used for the slow query panel).</summary>
    Public Property SlowThreshold As Double = 10.0

    ''' <summary>Maximum number of slow queries to display.</summary>
    Public Property MaxSlowRows As Integer = 20

    ''' <summary>Show the help text and exit.</summary>
    Public Property ShowHelp As Boolean = False

    ''' <summary>
    ''' Parse the given command line arguments into a <see cref="MonitorOptions"/> object.
    ''' Supported switches:
    '''   -h  host          -P  port
    '''   -u  user          -p  password
    '''   -d  database      -i  interval(seconds)
    '''   -s  slow threshold -n  max slow rows
    '''   --help            -?   show this help
    ''' </summary>
    Public Shared Function Parse(args As String()) As MonitorOptions
        Dim opt As New MonitorOptions

        Dim i As Integer = 0
        Do While i < args.Length
            Dim a = args(i)

            Select Case a
                Case "-host"
                    opt.Host = NextArg(args, i)
                Case "-port"
                    Integer.TryParse(NextArg(args, i), opt.Port)
                Case "-u"
                    opt.User = NextArg(args, i)
                Case "-p"
                    opt.Password = NextArg(args, i)
                Case "-d"
                    opt.Database = NextArg(args, i)
                Case "-i"
                    Double.TryParse(NextArg(args, i), opt.Interval)
                Case "-s"
                    Double.TryParse(NextArg(args, i), opt.SlowThreshold)
                Case "-n"
                    Integer.TryParse(NextArg(args, i), opt.MaxSlowRows)
                Case "--help", "-?"
                    opt.ShowHelp = True
            End Select

            i += 1
        Loop

        If opt.Interval <= 0 Then
            opt.Interval = 2.0
        End If

        Return opt
    End Function

    Private Shared Function NextArg(args As String(), ByRef i As Integer) As String
        If i + 1 < args.Length Then
            i += 1
            Return args(i)
        End If
        Return ""
    End Function

    ''' <summary>
    ''' Build a mysql connection uri string from the parsed options.
    ''' Format: mysql://user:password@host:port/database
    ''' </summary>
    Public Function BuildConnectionUri() As ConnectionUri
        Return New ConnectionUri(User, Password, Database, Host, Port)
    End Function

    ''' <summary>
    ''' Print the command line usage help text.
    ''' </summary>
    Public Shared Sub PrintHelp()
        Console.WriteLine("MySqlMonitor - realtime MySQL performance monitor")
        Console.WriteLine()
        Console.WriteLine("Usage: MySqlMonitor [-h host] [-P port] [-u user] [-p password]")
        Console.WriteLine("                [-d database] [-i interval] [-s slowThreshold] [-n maxSlowRows]")
        Console.WriteLine()
        Console.WriteLine("  -h  host            MySQL server host (default: localhost)")
        Console.WriteLine("  -P  port            MySQL server port (default: 3306)")
        Console.WriteLine("  -u  user            MySQL user name   (default: root)")
        Console.WriteLine("  -p  password        MySQL password    (default: empty)")
        Console.WriteLine("  -d  database        default database  (optional)")
        Console.WriteLine("  -i  interval        refresh interval in seconds (default: 2)")
        Console.WriteLine("  -s  slowThreshold   slow query threshold in seconds (default: 10)")
        Console.WriteLine("  -n  maxSlowRows     max slow queries shown (default: 20)")
        Console.WriteLine("  --help, -?          show this help and exit")
        Console.WriteLine()
        Console.WriteLine("Example:")
        Console.WriteLine("  MySqlMonitor -h 127.0.0.1 -P 3306 -u root -p secret -i 3 -s 5")
    End Sub

End Class
