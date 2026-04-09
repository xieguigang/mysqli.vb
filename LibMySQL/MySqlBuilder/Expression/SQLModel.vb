Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder.Expression

    Public MustInherit Class SQLModel

        Protected ReadOnly query As QueryBuilder
        Protected ReadOnly schema As Table

        Protected Sub New(schema As Table, query As QueryBuilder)
            Me.query = query
            Me.schema = schema
        End Sub

        Public Shared Narrowing Operator CType(sql As SQLModel) As String
            Return sql.ToString
        End Operator

    End Class
End Namespace