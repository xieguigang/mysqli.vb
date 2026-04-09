
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder.Expression

    Public Class UpdateSql : Inherits SQLModel

        ''' <summary>
        ''' updates data
        ''' </summary>
        ReadOnly fields As FieldAssert()

        Public Sub New(fields As FieldAssert(), schema As Table, query As QueryBuilder)
            MyBase.New(schema, query)
            Me.fields = fields
        End Sub

        Public Overrides Function ToString() As String
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim setFields As New List(Of String)

            For Each field As FieldAssert In fields
                If field.op <> "=" Then
                    Throw New InvalidOperationException
                End If

                Call setFields.Add($"{field.GetSafeName} = {field.val}")
            Next

            Return $"UPDATE `{schema.Database}`.`{schema.TableName}` SET {setFields.JoinBy(", ")} {where} {limit};"
        End Function

    End Class
End Namespace