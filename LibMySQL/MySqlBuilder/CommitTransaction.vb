Imports System.Runtime.CompilerServices

Namespace MySqlBuilder

    ''' <summary>
    ''' commit operation for the insert operation:
    ''' 
    ''' 1. <see cref="CommitInsert"/>
    ''' 2. <see cref="CommitTransaction"/>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' this class is used to commit the insert operation
    ''' </remarks>
    Public Interface IDataCommitOperation

        Sub Commit()

    End Interface

    ''' <summary>
    ''' the abstract model for insert into sql
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IInsertModel(Of T As IInsertModel(Of T))

        ''' <summary>
        ''' insert delayed into
        ''' </summary>
        ''' <returns></returns>
        Function delayed() As T
        ''' <summary>
        ''' insert ignore into
        ''' </summary>
        ''' <returns></returns>
        Function ignore() As T
        ''' <summary>
        ''' insert into
        ''' </summary>
        ''' <returns></returns>
        Function clearOption() As T

        ''' <summary>
        ''' implements of the insert into
        ''' </summary>
        ''' <param name="fields"></param>
        Function add(ParamArray fields As FieldAssert()) As Boolean

    End Interface

    Public Class CommitTransaction : Implements IDisposable, IDataCommitOperation, IModel, IInsertModel(Of CommitTransaction)

        Dim disposedValue As Boolean
        Dim model As Model
        Dim trans_sql As New List(Of String)

        Sub New(model As Model)
            Me.model = model
        End Sub

        ''' <summary>
        ''' set delayed options for insert into
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' this delayed options will be reste to no-delayed after insert has been called
        ''' </remarks>
        Public Function delayed() As CommitTransaction Implements IInsertModel(Of CommitTransaction).delayed
            model.delayed()
            Return Me
        End Function

        Public Function ignore() As CommitTransaction Implements IInsertModel(Of CommitTransaction).ignore
            model.ignore()
            Return Me
        End Function

        Public Function clearOption() As CommitTransaction Implements IInsertModel(Of CommitTransaction).clearOption
            model.clearOption()
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function field(name As String) As FieldAssert Implements IModel.field
            Return model.field(name)
        End Function

        Public Sub add(sql As String)
            Call trans_sql.Add(sql)
        End Sub

        Public Function add(ParamArray fields As FieldAssert()) As Boolean Implements IInsertModel(Of CommitTransaction).add
            Call add(model.add_sql(fields))
            Return True
        End Function

        Public Sub commit() Implements IDataCommitOperation.Commit
            Dim ex As Exception = Nothing

            If Not model.mysql.CommitTransaction(trans_sql.JoinBy(vbCrLf), ex) Then
                Throw ex
            Else
                disposedValue = True
            End If
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