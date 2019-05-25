Imports Oracle.LinuxCompatibility.MySQL.Scripting

Module Module1

    Sub Main()

        Dim table As New Linq(Of mysql.visitor_stat)
        Dim sql = BuildSQL(Function() From row As mysql.visitor_stat
                                      In table
                                      Where row.app = 1 AndAlso row.ip Like "192.168.%"
                                      Select row.ip, row.success, row.time
                                      Order By ip Descending
                                      Distinct)

        sql = BuildSQL(Function() From row As mysql.visitor_stat
                                  In table
                                  Select row
                                  Take 1)

        Pause()
    End Sub
End Module
