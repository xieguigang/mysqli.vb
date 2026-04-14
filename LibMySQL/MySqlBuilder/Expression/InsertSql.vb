
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder.Expression

    Public Class InsertSql : Inherits SQLModel

        ReadOnly fields As FieldAssert()

        Public Sub New(fields As FieldAssert(), schema As Table, query As QueryBuilder)
            MyBase.New(schema, query)
            Me.fields = fields
        End Sub

        Public Overrides Function ToString() As String
            Dim names As String = fields.Select(Function(a) a.GetSafeName).JoinBy(", ")
            Dim vals As String = fields.Select(Function(a) a.val).JoinBy(", ")
            Dim sql As String = $"INSERT {opt.Description} INTO `{schema.Database}`.`{schema.TableName}` ({names}) VALUES ({vals});"

            Return sql
        End Function
    End Class
End Namespace