Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder.Expression

Namespace MySqlBuilder

    Public Class CommitTransaction : Implements IDisposable, IDataCommitOperation, IModel, IInsertModel(Of CommitTransaction)

        Dim disposedValue As Boolean
        Dim model As Model
        Dim trans_sql As New List(Of SQLModel)
        Dim blockSize As Integer = 1024

        Sub New(model As Model, Optional blockSize As Integer = 1024)
            Me.model = model
            Me.blockSize = blockSize
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

        ''' <summary>
        ''' delete on where condition filter records
        ''' </summary>
        ''' <param name="where"></param>
        Public Sub delete(ParamArray where As FieldAssert())
            If where.IsNullOrEmpty Then
                Throw New InvalidOperationException("DELETE FROM must has condition for make record filtering!")
            End If

            Call add(sql:=model.where(where).delete_sql)
        End Sub

        ''' <summary>
        ''' add transaction sql text into current memory cache
        ''' </summary>
        ''' <param name="sql"></param>
        Public Sub add(sql As String)
            Call trans_sql.Add(New SQLText(sql))
        End Sub

        Public Sub add(sql As SQLModel)
            Call trans_sql.Add(sql)
        End Sub

        ''' <summary>
        ''' just create the insert into sql and added into the transaction memory cache pool
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function add(ParamArray fields As FieldAssert()) As Boolean Implements IInsertModel(Of CommitTransaction).add
            Call add(model.add_sql(fields))
            Return True
        End Function

        ''' <summary>
        ''' write the in-memory cache of the transaction data into database
        ''' </summary>
        Public Sub commit() Implements IDataCommitOperation.Commit
            Dim ex As Exception = Nothing

            For Each block As SQLModel() In trans_sql.SplitIterator(blockSize)
                Dim sql As IEnumerable(Of String) = From line As SQLModel
                                                    In block
                                                    Let dml_sql As String = CStr(line)
                                                    Select dml_sql

                If Not model.mysql.CommitTransaction(sql, ex) Then
                    Throw ex
                End If
            Next

            trans_sql.Clear()
            disposedValue = True
        End Sub

        ''' <summary>
        ''' commit the transaction
        ''' </summary>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call commit()
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