Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL
Imports Microsoft.VisualBasic.Language

''' <summary>
''' 使用Linq方法进行非常大的数据库的导出操作
''' </summary>
Public Module LinqExports

    ''' <summary>
    ''' 将数据结果导出到一个文件夹之中，文件名为表名称
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="EXPORT$"></param>
    <Extension>
    Public Sub ProjectDumping(source As IEnumerable(Of NamedValue(Of SQLTable)), EXPORT$, Optional bufferSize% = 2000)
        Dim writer As New Dictionary(Of String, StreamWriter)
        Dim buffer As New Dictionary(Of String, (schema As Table, bufferData As List(Of SQLTable)))

        For Each x As NamedValue(Of SQLTable) In source
            If Not writer.ContainsKey(x.Name) Then
                writer(x.Name) = $"{EXPORT}/{x.Name}.sql".OpenWriter
                buffer(x.Name) = (New Table(x.ValueType), New List(Of SQLTable))
            End If

            With buffer(x.Name)
                If .bufferData = bufferSize Then
                    Call .bufferData.DumpBlock(.schema, writer(x.Name))
                    Call .bufferData.Clear()
                Else
                    Call .bufferData.Add(x.Value)
                End If
            End With
        Next

        For Each buf In buffer.EnumerateTuples
            With buf.obj
                Call .bufferData.DumpBlock(.schema, writer(buf.name))
            End With
        Next

        For Each handle As StreamWriter In writer.Values
            Call handle.Flush()
            Call handle.Close()
            Call handle.Dispose()
        Next
    End Sub

    <Extension>
    Public Sub DumpBlock(block As IEnumerable(Of SQLTable), schemaTable As Table, out As StreamWriter, Optional distinct As Boolean = True)
        Dim INSERT$ = schemaTable.GenerateInsertSql
        Dim schema$ = INSERT.StringSplit("\)\s*VALUES\s*\(").First & ") VALUES "
        Dim insertBlocks$() = block _
            .Where(Function(r) Not r Is Nothing) _
            .Select(Function(r) r.GetDumpInsertValue) _
            .ToArray

        If distinct Then
            insertBlocks = insertBlocks _
                .Distinct _
                .ToArray
        End If

        ' Generates the SQL dumps data
        Dim SQL$ = schema & insertBlocks.JoinBy(", ") & ";"

        Call out.WriteLine(SQL)
    End Sub
End Module
