Imports System.IO
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL

Namespace Workbench

    Public Class DataWriter

        ReadOnly out As TextWriter
        ReadOnly distinct As Boolean = True
        ReadOnly AI As Boolean = False

        Sub New(out As TextWriter, Optional distinct As Boolean = True, Optional AI As Boolean = False)
            Me.out = out
            Me.distinct = distinct
            Me.AI = AI
        End Sub

        Private Iterator Function GetBatchInsertSql(block As IEnumerable(Of MySQLTable)) As IEnumerable(Of String)
            For Each rowData As MySQLTable In block.SafeQuery
                If Not rowData Is Nothing Then
                    Yield r.GetDumpInsertValue(AI)
                End If
            Next
        End Function

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
    End Class
End Namespace