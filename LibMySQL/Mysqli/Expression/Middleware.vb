
Imports Microsoft.VisualBasic.Scripting

Namespace Expressions

    Public Structure FieldArgument

        ReadOnly Name$

        Sub New(name$)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Shared Operator <=(field As FieldArgument, value As Object) As String
            Return $"`{field.Name}` = '{InputHandler.ToString(value)}'"
        End Operator

        Public Shared Operator >=(field As FieldArgument, value As Object) As String
            Throw New NotImplementedException
        End Operator

        Public Shared Widening Operator CType(name$) As FieldArgument
            Return New FieldArgument(name)
        End Operator
    End Structure

    Public Structure WhereArgument(Of T As {New, MySQLTable})

        Dim table As Table(Of T)
        Dim condition$

        Public Function GetSQL(Optional scalar As Boolean = False) As String
            If scalar Then
                Return $"SELECT * FROM `{table.Schema.TableName}` WHERE {condition} LIMIT 1;"
            Else
                Return $"SELECT * FROM `{table.Schema.TableName}` WHERE {condition};"
            End If
        End Function

        Public Overrides Function ToString() As String
            Return condition
        End Function
    End Structure
End Namespace