
Imports System.Runtime.CompilerServices

Namespace MySqlBuilder

    Public Module BuilderExtensions

        <Extension>
        Friend Function IsNullOrEmpty(where As FilterConditions) As Boolean
            If where Is Nothing Then
                Return True
            Else
                Return where.where.IsNullOrEmpty
            End If
        End Function

        <Extension>
        Public Function Add(data As List(Of FieldAssert), fieldName As String, value As Object) As List(Of FieldAssert)
            Call data.Add(field(fieldName) = value)
            Return data
        End Function

    End Module
End Namespace