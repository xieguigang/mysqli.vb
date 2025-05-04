
Imports System.Runtime.CompilerServices

Namespace MySqlBuilder

    Module BuilderExtensions

        <Extension>
        Public Function IsNullOrEmpty(where As FilterConditions) As Boolean
            If where Is Nothing Then
                Return True
            Else
                Return where.where.IsNullOrEmpty
            End If
        End Function

    End Module
End Namespace