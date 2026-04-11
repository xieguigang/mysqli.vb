Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder.Expression

    Public MustInherit Class SQLModel

        Protected ReadOnly query As QueryBuilder
        Protected ReadOnly schema As Table

        Public Property opt As DMLOptions

        Protected Sub New(schema As Table, query As QueryBuilder)
            Me.query = query
            Me.schema = schema
        End Sub

        Public Shared Narrowing Operator CType(sql As SQLModel) As String
            Return sql.ToString
        End Operator

    End Class

    Public Class SQLText : Inherits SQLModel

        ReadOnly sql As String

        Sub New(sql As String)
            Call MyBase.New(Nothing, Nothing)
            Me.sql = sql
        End Sub

        Public Overrides Function ToString() As String
            Return sql
        End Function

    End Class
End Namespace