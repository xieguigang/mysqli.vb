Imports Oracle.LinuxCompatibility.MySQL
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Module Test

    <TableName("Test")>
    Public Class T : Inherits SQLTable

        <DatabaseField("A")> Public Property A As String
        <DatabaseField("B")> Public Property B As Double
        <DatabaseField("C")> Public Property C As Date

        Public Overrides Function GetReplaceSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetDumpInsertValue() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetInsertSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetUpdateSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetDeleteSQL() As String
            Throw New NotImplementedException()
        End Function
    End Class

    Sub Main()
        Dim uri$ = "https://localhost:3306/client?user=root&password=&database=test"
        Dim table As New Table(Of T)(uri)
        Dim A As FieldArgument = NameOf(T.A)
        Dim B As FieldArgument = NameOf(T.B)
        Dim C As FieldArgument = NameOf(T.C)

        Dim SQL = table _
            .Where(
                A <= "555",
                C <= #2017-6-9 12:56:36#,
                B <= -99.369) _
            .And(
                A <= "666",
                B <= 10) _
            .Or(
                A <= "999") _
            .GetSQL(scalar:=True)

        Dim all = table.SelectAll

        println(SQL)

        Pause()
    End Sub
End Module
