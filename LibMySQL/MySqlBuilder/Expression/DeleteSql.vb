Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder.Expression

    Public Class DeleteSql : Inherits SQLModel

        Public Sub New(schema As Table, query As QueryBuilder)
            MyBase.New(schema, query)
        End Sub

        Public Overrides Function ToString() As String
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim sql As String = $"DELETE FROM `{schema.Database}`.`{schema.TableName}` {where} {limit};"

            Return sql
        End Function
    End Class
End Namespace