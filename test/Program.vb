Imports System
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Uri

Module Program
    Sub Main(args As String())
        Dim model As New Model(New ConnectionUri With {.Database = "graphql", .IPAddress = "localhost", .Password = 123456, .Port = 3306, .User = "graphql"})
    End Sub
End Module
