#Region "Microsoft.VisualBasic::9dab78ed91927e73fd711977d550b883, ..\mysqli\LibMySQL\Extensions\DataDumps.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL

Public Module DataDumps

    ''' <summary>
    ''' This function works for the tables that have foreign key constraint between each others.
    ''' (这个函数只适用于比较小规模的数据库的导出)
    ''' </summary>
    ''' <param name="output"></param>
    ''' <param name="tables"></param>
    <Extension>
    Public Sub DumpMySQL(output As StreamWriter, ParamArray tables As MySQLTable()())
        Dim type As Type

        Call output.DumpSession(
            Sub(buffer)

                For Each table As MySQLTable() In tables
                    With table
                        type = .First.GetType
                        Call .DumpTransaction(buffer, type, distinct:=True)
                    End With
                Next
            End Sub)
    End Sub

    Public Const OptionsTempChange$ =
        "-- MySQL dump 1.50  Distrib 5.7.12, for Microsoft VisualBasic.NET ORM code solution (x86_64)
--
-- Database: %s
--
-- -----------------------------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

"
    Public Const OptionsRestore$ = "

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on {0}"

    Const ActionNotAllows$ = "Only allowes ""insert/update/delete/replace"" actions."

    ''' <summary>
    ''' 生成用于将数据集合批量导入数据库的INSERT SQL事务
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="type">
    ''' Only allowed action ``insert/update/delete/replace``, if the user custom SQL generator 
    ''' <paramref name="custom"/> is nothing, then this parameter works.
    ''' </param>
    ''' <param name="custom">
    ''' User custom SQL generator. If this parameter is not nothing, then <paramref name="type"/> will disabled.
    ''' </param>
    <Extension> Public Sub DumpTransaction(source As IEnumerable(Of MySQLTable),
                                           out As TextWriter,
                                           type As Type,
                                           Optional custom As Func(Of MySQLTable, String) = Nothing,
                                           Optional action$ = "insert",
                                           Optional distinct As Boolean = True)

        Dim SQL As Func(Of MySQLTable, String)

        If custom Is Nothing Then
            Select Case LCase(action)
                Case "insert" : SQL = Function(o) o.GetInsertSQL
                Case "update" : SQL = Function(o) o.GetUpdateSQL
                Case "delete" : SQL = Function(o) o.GetDeleteSQL
                Case "replace" : SQL = Function(o) o.GetReplaceSQL
                Case Else
                    Throw New ArgumentException(ActionNotAllows, paramName:=NameOf(type))
            End Select
        Else
            SQL = custom
        End If

        Dim schemaTable As New Table(type)
        Dim tableName$ = schemaTable.TableName

        Call out.LockTable(tableName)

        If action.TextEquals("insert") Then
            For Each block In source.Split(200)
                Call block.DumpBlock(schemaTable, out, distinct)
            Next
        Else
            Call source _
                .Select(SQL) _
                .JoinBy(ASCII.LF) _
                .FlushTo(out)
        End If

        Call out.UnlockTable(tableName)
    End Sub

    <Extension>
    Friend Sub LockTable(out As TextWriter, tableName$)
        Call out.WriteLine("--")
        Call out.WriteLine($"-- Dumping data for table `{tableName}`")
        Call out.WriteLine("--")
        Call out.WriteLine()

        Call out.WriteLine($"LOCK TABLES `{tableName}` WRITE;")
        Call out.WriteLine($"/*!40000 ALTER TABLE `{tableName}` DISABLE KEYS */;")
    End Sub

    <Extension>
    Public Sub UnlockTable(out As TextWriter, tableName$)
        Call out.WriteLine($"/*!40000 ALTER TABLE `{tableName}` ENABLE KEYS */;")
        Call out.WriteLine("UNLOCK TABLES;")
    End Sub

    <Extension>
    Private Sub DumpSession(buffer As TextWriter, dumping As Action(Of TextWriter))
        Call buffer.WriteLine(OptionsTempChange)
        Call dumping(buffer)
        Call buffer.WriteLine(OptionsRestore, Now.ToString)
    End Sub

    ''' <summary>
    ''' 生成用于将数据集合批量导入数据库的INSERT SQL事务
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="type">
    ''' Only allowed action ``insert/update/delete/replace``, if the user custom SQL generator 
    ''' <paramref name="custom"/> is nothing, then this parameter works.
    ''' </param>
    ''' <param name="custom">
    ''' User custom SQL generator. If this parameter is not nothing, then <paramref name="type"/> will disabled.
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function DumpTransaction(Of T As MySQLTable)(source As IEnumerable(Of T),
                                                        Optional custom As Func(Of MySQLTable, String) = Nothing,
                                                        Optional type$ = "insert",
                                                        Optional distinct As Boolean = True) As String
        With New StringBuilder
            Call New StringWriter(.ref).DumpSession(
                Sub(buffer)
                    Call source.DumpTransaction(
                        buffer,
                        GetType(T), custom,
                        action:=type,
                        distinct:=distinct)
                End Sub)

            Return .ToString
        End With
    End Function

    ''' <summary>
    ''' Write a very large SQL table data collection into a SQL file.
    ''' (适合导出一个非常大的mysql数据表)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="path$"></param>
    ''' <param name="custom"></param>
    ''' <param name="type$"></param>
    ''' <param name="distinct"></param>
    <Extension>
    Public Sub DumpLargeTransaction(Of T As MySQLTable)(source As IEnumerable(Of T),
                                                        path$,
                                                        Optional custom As Func(Of MySQLTable, String) = Nothing,
                                                        Optional type$ = "insert",
                                                        Optional distinct As Boolean = True)
        Using output As StreamWriter = path.OpenWriter
            Call output.DumpSession(
                Sub(buffer)
                    Call source.DumpTransaction(
                        buffer,
                        GetType(T), custom,
                        action:=type,
                        distinct:=distinct)
                End Sub)
        End Using
    End Sub

    ''' <summary>
    ''' This function is only works for the table that without any foreign key constraint.
    ''' 
    ''' (这个函数只适合于没有外键约束的数据表)
    ''' 
    ''' 从<see cref="MySQLTable"/>之中生成SQL语句之后保存到指定的文件句柄之上，
    ''' + 假若所输入的文件句柄是带有``.sql``后缀的话，会直接保存为该文件，
    ''' + 反之会被当作为文件夹，当前的集合对象会保存为与类型相同名称的sql文件
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <param name="path$">
    ''' 请注意，在这里假若字符串是含有sql作为文件名后缀的话，会直接用作为文件路径来保存
    ''' 假若不是以sql为后缀的话，会被当做文件夹来处理
    ''' </param>
    ''' <param name="encoding"></param>
    ''' <param name="distinct">是否对<see cref="MySQLTable.GetDumpInsertValue()"/>进行去重处理？默认是</param>
    ''' <returns></returns>
    <Extension>
    Public Function DumpTransaction(Of T As MySQLTable)(source As IEnumerable(Of T),
                                                        path$,
                                                        Optional encoding As Encodings = Encodings.Default,
                                                        Optional type$ = "insert",
                                                        Optional distinct As Boolean = True) As Boolean
        Dim sql$ = source.DumpTransaction(
            type:=type,
            distinct:=distinct)

        If Not path.ExtensionSuffix.TextEquals("sql") Then
            Dim name$ = GetType(T).Name
            path = path & "/" & name & ".sql"
        End If

        Return sql.SaveTo(path, encoding.CodePage)
    End Function

    ''' <summary>
    ''' 将csv导入数据库之中的帮助工具，利用这个工具解析csv文件的头部标题行，生成``Create Table``脚本
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function CsvImportsHelper(path As String) As String
        Dim header$() = path _
            .ReadFirstLine _
            .Split(","c) _
            .Select(Function(s) s.Trim(ASCII.Quot)) _
            .ToArray
        Dim fields As Dictionary(Of String, Field) = header _
            .ToDictionary(Function(name) name,
                          Function(name) simpleField(name))
        Dim schema As New Table(fields) With {
            .TableName = path.BaseName,
            .Comment = $"Auto-generated table schema from csv file: {path}",
            .PrimaryFields = New List(Of String) From {header.First},
            .Database = path.ParentDirName
        }
        Dim SQL$ = CreateTableSQL.FromSchema(schema)
        Return SQL
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function simpleField(name As String) As Field
        Return New Field() With {
            .FieldName = name,
            .DataType = New DataType(MySqlDbType.Text)
        }
    End Function
End Module
