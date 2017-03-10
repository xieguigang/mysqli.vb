Namespace ServerApp

    Public Class MemoryCache(Of T As SQLTable)

        ReadOnly mysql As New MySQL
        ReadOnly __cache As Dictionary(Of String, T)

        Sub New(cnn As ConnectionUri)
            If (mysql <= cnn) = -1.0R Then
                Throw New Exception("No avalaible mysql connection!")
            End If
        End Sub

        Public Function Query(SQL$) As T()

        End Function

        Public Function ExecuteScalar(SQL$) As T

        End Function
    End Class
End Namespace
