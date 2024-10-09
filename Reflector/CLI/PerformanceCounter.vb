Imports System.Threading
Imports Flute.Http
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Net.Http
Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter
Imports Oracle.LinuxCompatibility.MySQL.Uri

Partial Module CLI

    <ExportAPI("/performance_counter")>
    <Usage("/performance_counter -u <mysql_user_name> -p <password> [--host <mysql_host, default=localhost> --port <mysql_port, default=3306> --http <monitor_output, default=86>]")>
    Public Function PerformanceCounter(args As CommandLine) As Integer
        Dim u As String = args("-u")
        Dim p As String = args("-p")
        Dim host As String = args("--host") Or "localhost"
        Dim port As Integer = args("--port") Or 3306
        Dim http As Integer = args("--http") Or 86
        Dim uri As New ConnectionUri() With {
            .Database = "performance_schema",
            .IPAddress = host,
            .Password = p,
            .Port = port,
            .User = u
        }
        Dim monitor As New Logger(uri, echo:=False)
        Dim service = New HttpDriver() _
            .HttpMethod("get", Sub(req, response)
                                   Dim url As URL = req.URL

                                   If Strings.Trim(url.path).Trim("/"c) = "get" Then
                                       Call response.WriteJSON(monitor.GetLogging)
                                   End If
                               End Sub) _
            .GetSocket(http)

        Call New Thread(Sub() monitor.Run()).Start()
        Call Thread.Sleep(1000)
        Call VBDebugger.EchoLine($"use api 'localhost:{http}/get/' for get mysql server performance counter data.")
        Call service.Run()

        Return 0
    End Function

End Module