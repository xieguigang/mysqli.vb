Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL

Public Module DataDumps

    <Extension>
    Public Sub DumpMySQL(output As StreamWriter, ParamArray tables As SQLTable()())

    End Sub

    Const OptionsTempChange$ = "-- MySQL dump 1.50  Distrib 5.7.12, for Microsoft VisualBasic.NET ORM code solution (x86_64)

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
    Const OptionsRestore$ = "

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on {0}"

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
    Public Function DumpTransaction(Of T As SQLTable)(source As IEnumerable(Of T),
                                                      Optional custom As Func(Of T, String) = Nothing,
                                                      Optional type$ = "insert",
                                                      Optional distinct As Boolean = True) As String
        Dim SQL As Func(Of T, String)

        If custom Is Nothing Then
            Select Case LCase(type)
                Case "insert" : SQL = Function(o) o.GetInsertSQL
                Case "update" : SQL = Function(o) o.GetUpdateSQL
                Case "delete" : SQL = Function(o) o.GetDeleteSQL
                Case "replace" : SQL = Function(o) o.GetReplaceSQL
                Case Else
                    Throw New ArgumentException("Only allowes ""insert/update/delete/replace"" actions.", paramName:=NameOf(type))
            End Select
        Else
            SQL = custom
        End If

        Dim schemaTable As New Table(GetType(T))
        Dim tableName$ = schemaTable.TableName
        Dim sb As New StringBuilder()

        Call sb.AppendLine(OptionsTempChange)

        Call sb.AppendLine("--")
        Call sb.AppendLine($"-- Dumping data for table `{tableName}`")
        Call sb.AppendLine("--")
        Call sb.AppendLine()

        Call sb.AppendLine($"LOCK TABLES `{tableName}` WRITE;")
        Call sb.AppendLine($"/*!40000 ALTER TABLE `{tableName}` DISABLE KEYS */;")

        If type.TextEquals("insert") Then
            Dim insertBlocks$() = source _
                .Where(Function(r) Not r Is Nothing) _
                .Select(Function(r) r.GetDumpInsertValue) _
                .ToArray
            Dim INSERT$ = schemaTable.GenerateInsertSql
            Dim schema$ = INSERT.StringSplit("\)\s*VALUES\s*\(").First & ") VALUES "

            If distinct Then
                insertBlocks = insertBlocks.Distinct.ToArray
            End If

            For Each block In insertBlocks.Split(200)
                Call sb.AppendLine(schema & block.JoinBy(", ") & ";")
            Next
        Else
            Call sb.AppendLine(source.Select(SQL).JoinBy(ASCII.LF))
        End If

        Call sb.AppendLine($"/*!40000 ALTER TABLE `{tableName}` ENABLE KEYS */;")
        Call sb.AppendLine("UNLOCK TABLES;")
        Call sb.AppendFormat(OptionsRestore, Now.ToString)

        Return sb.ToString
    End Function

    ''' <summary>
    ''' 从<see cref="SQLTable"/>之中生成SQL语句之后保存到指定的文件句柄之上，
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
    ''' <param name="distinct">是否对<see cref="SQLTable.GetDumpInsertValue()"/>进行去重处理？默认是</param>
    ''' <returns></returns>
    <Extension>
    Public Function DumpTransaction(Of T As SQLTable)(source As IEnumerable(Of T),
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
End Module
