Imports System
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Uri

Module Program
    Sub Main(args As String())
        Dim model As New Model("knowledge", New ConnectionUri With {.Database = "graphql", .IPAddress = "localhost", .Password = 123456, .Port = 3306, .User = "graphql"})

        Dim search1 = model.where(
            field("id") = 1,
            field("add_time") > #2023-11-11 12:00:00#
        ).find(Of mysql.knowledge)


        Pause()
    End Sub


End Module
