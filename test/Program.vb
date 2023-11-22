Imports Microsoft.VisualBasic.Serialization.JSON
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Uri

Module Program
    Sub Main(args As String())
        Dim model As New Model("knowledge", New ConnectionUri With {.Database = "graphql", .IPAddress = "localhost", .Password = 123456, .Port = 3306, .User = "graphql"})

        Dim insert = model.add(
            f("key") = 1112,
            f("display_title") = "dfgfhaskfsfs",
            f("node_type") = 11,
            f("graph_size") = 24234,
            f("add_time") = #2023/1/6 19:55:33#,
            f("description") = "anything!",
            f("id") = 991
        )

        Call Console.WriteLine(Model.GetLastMySql)

        Dim updates = model _
            .where(model.field("id") = 99) _
            .limit(1) _
            .save(model.field("display_title") = "new display title")

        Call Console.WriteLine(Model.GetLastMySql)

        Dim search1 = model.where(
            field("id") = 99,
            field("add_time") < #2023-11-11 12:00:00#
        ).find(Of mysql.knowledge)

        Call Console.WriteLine(Model.GetLastMySql)
        Call Console.WriteLine(search1.GetJson)

        Pause()
    End Sub


End Module
