#Region "Microsoft.VisualBasic::090d851a12726dbcf1652a2fe1791358, LibMySQL\Extensions\LinqExports.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

' Module LinqExports
' 
'     Sub: DumpBlock, ProjectDumping
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
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
    Public Sub ProjectDumping(source As IEnumerable(Of MySQLTable), EXPORT$,
                              Optional bufferSize% = 500,
                              Optional singleTransaction As Boolean = False,
                              Optional echo As Boolean = True,
                              Optional auto_increment As Boolean = False)
        Dim saveSQL$ = EXPORT

        If singleTransaction Then
            EXPORT = App.GetAppSysTempFile(sessionID:=App.PID)
        End If

        Dim DBName$ = source.dumpRows(EXPORT, bufferSize, singleTransaction, echo, auto_increment)

        If singleTransaction Then

            If echo Then
                Call $"Output single transaction SQL file to: {saveSQL}".__INFO_ECHO
            End If

            Call joinTransactionSql(saveSQL, DBName, EXPORT)

            If echo Then
                Call "job done!".__INFO_ECHO
            End If
        End If
    End Sub

    <Extension>
    Private Function dumpRows(source As IEnumerable(Of MySQLTable),
                              EXPORT$,
                              bufferSize%,
                              singleTransaction As Boolean,
                              echo As Boolean,
                              AI As Boolean) As String

        Dim writer As New Dictionary(Of String, StreamWriter)
        Dim buffer As New Dictionary(Of String, (schema As Table, bufferData As List(Of MySQLTable)))
        Dim tableNames As New Dictionary(Of Type, String)
        Dim DBName$ = ""

        For Each obj As MySQLTable In source.SafeQuery
            Dim type As Type = obj.GetType
            Dim tblName$

            If Not tableNames.ContainsKey(type) Then
                tableNames(type) = TableName.GetTableName(type)
            End If

            tblName = tableNames(type)

            If Not writer.ContainsKey(tblName) Then
                buffer(tblName) = (New Table(type), New List(Of MySQLTable))
                DBName = buffer(tblName).schema.Database

                With $"{EXPORT}/{DBName}_{tblName}.sql".OpenWriter
                    If Not singleTransaction Then
                        Call .WriteLine(OptionsTempChange.Replace("%s", DBName))

                        If echo Then
                            Call ("  --> " & DirectCast(.BaseStream, FileStream).Name).__INFO_ECHO
                        End If
                    End If

                    Call .LockTable(tblName)
                    Call .WriteLine()
                    Call writer.Add(tblName, .ByRef)
                End With
            End If

            With buffer(tblName)
                If .bufferData = bufferSize Then
                    Call .bufferData.DumpBlock(.schema, writer(tblName), AI:=AI)
                    Call .bufferData.Clear()

                    If echo Then
                        Call $"write_buffer({tblName})".__DEBUG_ECHO
                    End If
                Else
                    Call .bufferData.Add(obj)
                End If
            End With
        Next

        For Each buf In buffer.EnumerateTuples
            With buf.obj
                Call .bufferData.DumpBlock(.schema, writer(buf.name), AI:=AI)
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

        Return DBName
    End Function

    ''' <summary>
    ''' Merge the sql files that exported into a large single sql transaction file.
    ''' </summary>
    ''' <param name="saveSQL$"></param>
    ''' <param name="dbName$"></param>
    ''' <param name="EXPORT$"></param>
    Private Sub joinTransactionSql(saveSQL$, dbName$, EXPORT$)
        Using SQL As StreamWriter = saveSQL.OpenWriter
            With SQL
                Call .WriteLine(OptionsTempChange.Replace("%s", dbName))

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
    End Sub

    <Extension>
    Public Sub DumpBlock(block As IEnumerable(Of MySQLTable), schemaTable As Table, out As TextWriter,
                         Optional distinct As Boolean = True,
                         Optional AI As Boolean = False)

        Dim INSERT$ = schemaTable.GenerateInsertSql(trimAutoIncrement:=Not AI)
        Dim schema$ = INSERT.StringSplit("\)\s*VALUES\s*\(").First & ") VALUES "
        Dim insertBlocks$() = block _
            .Where(Function(r) Not r Is Nothing) _
            .Select(Function(r) r.GetDumpInsertValue(AI)) _
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
