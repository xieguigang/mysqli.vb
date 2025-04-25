Namespace MySqlBuilder

    Public Class CommitTransaction : Implements IDisposable

        Dim disposedValue As Boolean
        Dim model As Model
        Dim trans_sql As New List(Of String)

        Sub New(model As Model)
            Me.model = model
        End Sub

        Public Sub add(sql As String)
            Call trans_sql.Add(sql)
        End Sub

        Public Sub add(ParamArray fields As FieldAssert())
            Call add(model.add_sql(fields))
        End Sub

        Public Sub commit()
            model.mysql.CommitTransaction(trans_sql.JoinBy(vbCrLf))
            disposedValue = True
        End Sub

        ''' <summary>
        ''' commit the transaction
        ''' </summary>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace