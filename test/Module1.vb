Imports Oracle.LinuxCompatibility.MySQL.Uri

Module Module1

    Sub connectionStringTest()
        Dim conn As New ConnectionUri With {.Database = "aaaa", .IPAddress = "127.0.0.1", .Password = 123, .Port = 33669, .TimeOut = 60, .User = "me"}

        Call Console.WriteLine(conn.ToString)
        Call Console.WriteLine(conn.GetDisplayUri)

        Dim uri = conn.GenerateUri(Function(s) s)
        Dim conn_str = conn.GetConnectionString

        Dim parse1 As ConnectionUri = uri
        Dim parse2 As ConnectionUri = conn_str

        Call Console.WriteLine(parse1)
        Call Console.WriteLine(parse2)

        Pause()
    End Sub
End Module
