Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL

''' <summary>
''' 使用Linq方法进行非常大的数据库的导出操作
''' </summary>
Public Module LinqExports

    ''' <summary>
    ''' 将数据结果导出到一个文件夹之中，文件名为表名称
    ''' </summary>
    ''' <param name="source">名字必须为表名称</param>
    ''' <param name="EXPORT$"></param>
    ''' <param name="singleTransaction">
    ''' Merge the sql files that exported into a large single sql transaction file? Default is not.
    ''' </param>
    <Extension>
    Public Sub ProjectDumping(source As IEnumerable(Of NamedValue(Of MySQLTable)), EXPORT$, Optional bufferSize% = 500, Optional singleTransaction As Boolean = False, Optional echo As Boolean = True)
        Dim writer As New Dictionary(Of String, StreamWriter)
        Dim buffer As New Dictionary(Of String, (schema As Table, bufferData As List(Of MySQLTable)))
        Dim DBName$ = ""
        Dim saveSQL$ = EXPORT

        EXPORT = App.GetAppSysTempFile(sessionID:=App.PID)

        For Each x As NamedValue(Of MySQLTable) In source
            If Not writer.ContainsKey(x.Name) Then
                buffer(x.Name) = (New Table(x.ValueType), New List(Of MySQLTable))
                DBName = buffer(x.Name).schema.Database

                With $"{EXPORT}/{DBName}_{x.Name}.sql".OpenWriter
                    If Not singleTransaction Then
                        Call .WriteLine(OptionsTempChange.Replace("%s", DBName))

                        If echo Then
                            Call ("  --> " & DirectCast(.BaseStream, FileStream).Name).__INFO_ECHO
                        End If
                    End If

                    Call .LockTable(x.Name)
                    Call .WriteLine()
                    Call writer.Add(x.Name, .ref)
                End With
            End If

            With buffer(x.Name)
                If .bufferData = bufferSize Then
                    Call .bufferData.DumpBlock(.schema, writer(x.Name))
                    Call .bufferData.Clear()

                    If echo Then
                        Call $"write_buffer({x.Name})".__DEBUG_ECHO
                    End If
                Else
                    Call .bufferData.Add(x.Value)
                End If
            End With
        Next

        For Each buf In buffer.EnumerateTuples
            With buf.obj
                Call .bufferData.DumpBlock(.schema, writer(buf.name))
            End With

            With writer(buf.name)
                Call .WriteLine()
                Call .UnlockTable(buf.name)

                If Not singleTransaction Then
                    Call .WriteLine(OptionsRestore, Now.ToString)
                End If
            End With
        Next

        For Each handle As StreamWriter In writer.Values
            Call handle.Flush()
            Call handle.Close()
            Call handle.Dispose()
        Next

        If singleTransaction Then

            If echo Then
                Call $"Output single transaction SQL file to: {saveSQL}".__INFO_ECHO
            End If

            ' merge the sql files that exported into a large single sql transaction file.
            Using SQL As StreamWriter = saveSQL.OpenWriter
                With SQL
                    Call .WriteLine(OptionsTempChange.Replace("%s", DBName))

                    For Each path As String In ls - l - r - "*.sql" <= EXPORT
                        Using reader As StreamReader = path.OpenReader

                            Do While Not reader.EndOfStream
                                Call .WriteLine(reader.ReadLine)
                            Loop

                        End Using
                    Next

                    Call .WriteLine(OptionsRestore, Now.ToString)
                End With
            End Using

            If echo Then
                Call "job done!".__INFO_ECHO
            End If
        End If
    End Sub

    <Extension>
    Public Sub DumpBlock(block As IEnumerable(Of MySQLTable), schemaTable As Table, out As StreamWriter, Optional distinct As Boolean = True)
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

        ' 在下面调用.IsNullOrEmpty进行判断来避免出现
        ' INSERT INTO `tissue_locations` (`hash_code`, `uniprot_id`, `name`, `tissue_id`, `tissue_name`) VALUES ;
        ' 这种尴尬的错误
        If Not insertBlocks.IsNullOrEmpty Then
            Call out.WriteLine(SQL)
        End If
    End Sub
End Module
