Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL

Namespace Workbench

    Public Class DataWriter : Implements IDisposable

        ReadOnly out As TextWriter
        ReadOnly distinct As Boolean = True
        ReadOnly AI As Boolean = False

        Private disposedValue As Boolean

        Sub New(out As TextWriter, Optional distinct As Boolean = True, Optional AI As Boolean = False)
            Me.out = out
            Me.distinct = distinct
            Me.AI = AI
        End Sub

        Private Iterator Function GetBatchInsertSql(block As IEnumerable(Of MySQLTable)) As IEnumerable(Of String)
            For Each rowData As MySQLTable In block.SafeQuery
                If Not rowData Is Nothing Then
                    Yield rowData.GetDumpInsertValue(AI)
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine()
            Call out.WriteLine()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLine(line As String)
            Call out.WriteLine(line)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub UnlockTable(tableName As String)
            Call out.UnlockTable(tableName)
        End Sub

        Public Sub CommitBatch(block As IEnumerable(Of MySQLTable), schemaTable As Table)
            Dim insert_sql$ = schemaTable.GenerateInsertSql(trimAutoIncrement:=Not AI)
            Dim schema$ = insert_sql.StringSplit("\)\s*VALUES\s*\(").First & ") VALUES "
            Dim insertBlocks As String()

            If distinct Then
                insertBlocks = GetBatchInsertSql(block).Distinct.ToArray
            Else
                insertBlocks = GetBatchInsertSql(block).ToArray
            End If

            ' Generates the batch insert SQL line
            Dim SQL As String = schema & insertBlocks.JoinBy(", ") & ";"

            ' 在下面调用.IsNullOrEmpty进行判断来避免出现
            ' INSERT INTO `tissue_locations` (`hash_code`, `uniprot_id`, `name`, `tissue_id`, `tissue_name`) VALUES ;
            ' 这种尴尬的错误
            If Not insertBlocks.IsNullOrEmpty Then
                Call out.WriteLine(SQL)
            End If
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call out.Flush()
                    Call out.Dispose()
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