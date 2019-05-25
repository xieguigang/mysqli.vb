Imports Oracle.LinuxCompatibility.MySQL.Scripting

Module Module1

    Sub Main()

        Dim table As New Linq(Of mysql.visitor_stat)
        Dim linq = Function() From row As mysql.visitor_stat In table Where row.app = 1 Select row


        Pause()
    End Sub

End Module
