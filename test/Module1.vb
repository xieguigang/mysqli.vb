Imports Oracle.LinuxCompatibility.MySQL.Scripting

Module Module1

    Sub Main()

        Dim table As New Linq(Of mysql.visitor_stat)
        Dim sql = BuildSQL(Function() From row As mysql.visitor_stat In table Select row.ip, row.ref, y = row.uid + 9) ' Where row.app = 1 AndAlso row.ip Like "192.168.%" Select row Order By row.ip Descending Distinct)


        Pause()
    End Sub
End Module
