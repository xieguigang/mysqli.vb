Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Uri

Public MustInherit Class IDatabase

    Protected mysqli As MySqli

    Public Sub New(mysqli As ConnectionUri)
        Me.mysqli = mysqli
    End Sub

    Protected Function model(Of T As MySQLTable)() As Model
        Return New Model(TableName.GetTableName(Of T), mysqli)
    End Function

End Class
