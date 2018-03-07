#Region "Microsoft.VisualBasic::cdca8e18e5f7a120f96c7bf51edad1bd, CodeSolution\VisualBasic\CodeGenerator.vb"

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

    '     Module CodeGenerator
    ' 
    '         Function: ___DELETE_SQL, ___DELETE_SQL_Invoke, ___INSERT_SQL, ___INSERT_SQL_Invoke, ___REPLACE_SQL
    '                   ___REPLACE_SQL_Invoke, ___UPDATE_SQL, ___UPDATE_SQL_Invoke, __clone, __createAttribute
    '                   __generateCode, __generateCodeSplit, __getExprInvoke, __INSERT_VALUES, __notImplementForIndex
    '                   __replaceInsertCommon, __replaceInsertInvokeCommon, __schemaDb, __toDataType, FixInvalids
    '                   GenerateClass, (+3 Overloads) GenerateCode, (+2 Overloads) GenerateCodeSplit, GenerateSingleDocument, GenerateTableClass
    '                   PropertyName, SQLComments
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data.Linq.Mapping
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace VisualBasic

    ''' <summary>
    ''' Automatically generates visualbasic source code from the SQL schema dump document.(根据SQL文档生成Visual Basic源代码)
    ''' </summary>
    ''' <remarks></remarks>
    Public Module CodeGenerator

        ''' <summary>
        ''' Works with the conflicts of the VisualBasic keyword and strips for 
        ''' the invalid characters in the mysql table field name.
        ''' (处理VB里面的关键词的冲突以及清除mysql的表之中的域名中的相对于vb的标识符而言的非法字符)
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks>处理所有的VB标识符之中的非法字符都可以在这个函数之中完成</remarks>
        Public Function FixInvalids(name As String) As String
            ' mysql之中允许在名称中使用可以印刷的ASCII符号，但是vb并不允许，在这里替换掉
            For Each c As Char In ASCII.Symbols
                name = name.Replace(c, "_")
            Next

            name = VBLanguage.AutoEscapeVBKeyword(name.Replace(" ", "_"))
            Return name
        End Function

        ''' <summary>
        ''' 生成Class代码
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="[Namesapce]"></param>
        ''' <returns></returns>
        Public Function GenerateClass(SQL As String, [Namesapce] As String) As NamedValue(Of String)
            Dim table As Table = SQLParser.ParseTable(SQL)
            Dim vb$ = {table}.GenerateCode(Namesapce)

            Return New NamedValue(Of String) With {
                .Name = table.TableName,
                .Value = vb
            }
        End Function

#Region "Mapping the MySQL database type and visual basic data type"

        ''' <summary>
        ''' 将MySQL类型枚举转换为VisualBasic之中的数据类型定义
        ''' </summary>
        ''' <param name="TypeDef"></param>
        ''' <returns></returns>
        Private Function __toDataType(TypeDef As DataType) As String
            Select Case TypeDef.MySQLType

                Case MySqlDbType.Boolean
                    Return " As Boolean"

                Case MySqlDbType.BigInt, MySqlDbType.Int16, MySqlDbType.Int24, MySqlDbType.Int32, MySqlDbType.MediumInt
                    Return " As Integer"

                Case MySqlDbType.Bit, MySqlDbType.Byte
                    Return " As Byte"

                Case MySqlDbType.Date, MySqlDbType.DateTime
                    Return " As Date"

                Case MySqlDbType.Decimal
                    Return " As Decimal"

                Case MySqlDbType.Double, MySqlDbType.Float
                    Return " As Double"

                Case MySqlDbType.Int64
                    Return " As Long"

                Case MySqlDbType.UByte
                    Return " As UByte"

                Case MySqlDbType.UInt16, MySqlDbType.UInt24, MySqlDbType.UInt32
                    Return " As UInteger"

                Case MySqlDbType.UInt64
                    Return " As ULong"

                Case MySqlDbType.LongText, MySqlDbType.MediumText, MySqlDbType.String, MySqlDbType.Text,
                 MySqlDbType.TinyText, MySqlDbType.VarChar, MySqlDbType.VarString
                    Return " As String"

                Case MySqlDbType.Blob, MySqlDbType.LongBlob, MySqlDbType.MediumBlob, MySqlDbType.TinyBlob
                    Return " As Byte()"

                Case Else
                    Throw New NotImplementedException($"{NameOf(TypeDef)}={TypeDef.ToString}")
            End Select
        End Function
#End Region

        ''' <summary>
        ''' Convert each table schema into a visualbasic class object definition.
        ''' </summary>
        ''' <param name="listSQL"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function GenerateCode(listSQL As IEnumerable(Of Table), Optional namespace$ = "") As String
            Return __generateCode(listSQL, "", "", Nothing, [namespace])
        End Function

        Const SCHEMA_SECTIONS As String = "-- Table structure for table `.+?`"

        ''' <summary>
        ''' Generate the source code file from the table schema dumping.
        ''' (使用这个函数生成的程序源代码所有的``Class``类都是被放置在一个源文件之中的)
        ''' </summary>
        ''' <param name="SqlDoc"></param>
        ''' <param name="head"></param>
        ''' <param name="FileName"></param>
        ''' <param name="TableSql"></param>
        ''' <returns></returns>
        Private Function __generateCode(SqlDoc As IEnumerable(Of Table),
                                        head$,
                                        fileName$,
                                        TableSql As Dictionary(Of String, String),
                                        namespace$) As String

            Dim vb As New StringBuilder(1024)
            Dim haveNamespace As Boolean = Not String.IsNullOrEmpty([namespace])

            Call vb.AppendLine($"REM  {GetType(CodeGenerator).FullName}")
            Call vb.AppendLine($"REM  Microsoft VisualBasic MYSQL")
            Call vb.AppendLine()
            Call vb.AppendLine($"' SqlDump= {fileName}")
            Call vb.AppendLine()
            Call vb.AppendLine()

            If TableSql Is Nothing Then
                TableSql = New Dictionary(Of String, String)
            End If

            Dim tokens$() = Strings.Split(head.Replace(vbLf, ""), vbCr)
            Dim getSql = TableSql.GetValue

            For Each line As String In tokens
                Call vb.AppendLine("' " & line)
            Next

            Call vb.AppendLine()
            Call vb.AppendLine("Imports " & LinqMappingNs)
            Call vb.AppendLine("Imports System.Xml.Serialization")
            Call vb.AppendLine("Imports " & LibMySQLReflectionNs)
            Call vb.AppendLine()

            If haveNamespace Then
                Call vb.AppendLine($"Namespace {[namespace]}")
            End If

            For Each Line As String In From Table As Table
                                       In SqlDoc
                                       Let SqlDef As String = getSql(Table.TableName)
                                       Select GenerateTableClass(Table, SqlDef)

                Call vb.AppendLine()
                Call vb.AppendLine(Line)
                Call vb.AppendLine()
            Next

            If haveNamespace Then
                Call vb.AppendLine("End Namespace")
            End If

            Return vb.ToString
        End Function

        Private ReadOnly LibMySQLReflectionNs As String = GetType(MySqlDbType).FullName.Replace(".MySqlDbType", "")
        Private ReadOnly LinqMappingNs As String = GetType(ColumnAttribute).FullName.Replace(".ColumnAttribute", "")
        Private ReadOnly InheritsAbstract As String = GetType(MySQLTable).FullName

        ''' <summary>
        ''' Generate the class object definition to mapping a table in the mysql database.
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="DefSql"></param>
        ''' <param name="stripAI">
        ''' 如果这个参数为真，那么对于AI自增的字段而言，将不会被生成在``INSERT INTO``或者``REPLACE INTO``语句之中
        ''' </param>
        ''' <returns></returns>
        ''' <remarks><see cref="SQLComments"/></remarks>
        <Extension> Public Function GenerateTableClass(table As Table, DefSql$, Optional stripAI As Boolean = True) As String
            Dim tokens$() = DefSql.lTokens
            Dim vb As New StringBuilder("''' <summary>" & vbCrLf)
            Dim DBName As String = table.Database
            Dim refConflict As Boolean = Not (From field As String
                                              In table.FieldNames
                                              Where String.Equals(field, "datatype", StringComparison.OrdinalIgnoreCase)
                                              Select field) _
                                                .FirstOrDefault _
                                                .StringEmpty

            If Not String.IsNullOrEmpty(DBName) Then
                DBName = $", Database:=""{DBName}"""
            End If
            If Not String.IsNullOrEmpty(table.SQL) Then
                DBName &= $", {NameOf(TableName.SchemaSQL)}:=""{vbCrLf}{table.SQL}"""
            End If

            Call vb.AppendLine("''' ```SQL")
            If Not String.IsNullOrEmpty(table.Comment) Then
                Call vb.AppendLine("''' " & table.Comment)
            End If

            For Each line As String In tokens
                Call vb.AppendLine("''' " & line)
            Next
            Call vb.AppendLine("''' ```")
            Call vb.AppendLine("''' </summary>")
            Call vb.AppendLine("''' <remarks></remarks>")

            Call vb.AppendLine($"<Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes.TableName(""{table.TableName}""{DBName})>")
            Call vb.AppendLine($"Public Class {FixInvalids(table.TableName)}: Inherits {InheritsAbstract}")
            Call vb.AppendLine("#Region ""Public Property Mapping To Database Fields""")

            ' 生成Class之中的属性
            For Each Field As Field In table.Fields

                If Not String.IsNullOrEmpty(Field.Comment) Then
                    Call vb.AppendLine("''' <summary>")
                    Call vb.AppendLine("''' " & Field.Comment)
                    Call vb.AppendLine("''' </summary>")
                    Call vb.AppendLine("''' <value></value>")
                    Call vb.AppendLine("''' <returns></returns>")
                    Call vb.AppendLine("''' <remarks></remarks>")
                End If

                Call vb.Append(Field.__createAttribute(IsPrimaryKey:=table.PrimaryFields.Contains(Field.FieldName))) 'Apply the custom attribute on the property 
                Call vb.Append("Public Property " & FixInvalids(Field.FieldName))                                     'Generate the property name 
                Call vb.Append(__toDataType(Field.DataType))                                                          'Generate the property data type
                Call vb.AppendLine()
            Next

            Call vb.AppendLine("#End Region")

            Dim SQLlist As New Dictionary(Of String, Value(Of String)) From {
                {"INSERT", New Value(Of String)},
                {"REPLACE", New Value(Of String)},
                {"DELETE", New Value(Of String)},
                {"UPDATE", New Value(Of String)}
            }

            ' 生成SQL接口
            Call vb.AppendLine("#Region ""Public SQL Interface""")
            Call vb.AppendLine("#Region ""Interface SQL""")
            Call vb.AppendLine(___INSERT_SQL(table, stripAI, SQLlist("INSERT")))
            Call vb.AppendLine(___REPLACE_SQL(table, stripAI, SQLlist("REPLACE")))
            Call vb.AppendLine(___DELETE_SQL(table, SQLlist("DELETE")))
            Call vb.AppendLine(___UPDATE_SQL(table, SQLlist("UPDATE")))
            Call vb.AppendLine("#End Region")
            Call vb.Append(SQLlist("DELETE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetDeleteSQL() As String")
            Call vb.AppendLine(___DELETE_SQL_Invoke(table, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.Append(SQLlist("INSERT").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetInsertSQL() As String")
            Call vb.AppendLine(___INSERT_SQL_Invoke(table, stripAI, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.AppendLine("''' <summary>")
            Call vb.AppendLine($"''' <see cref=""{NameOf(MySQLTable.GetInsertSQL)}""/>")
            Call vb.AppendLine("''' </summary>")
            Call vb.AppendLine(__INSERT_VALUES(table, stripAI))
            Call vb.AppendLine()
            Call vb.Append(SQLlist("REPLACE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetReplaceSQL() As String")
            Call vb.AppendLine(___REPLACE_SQL_Invoke(table, stripAI, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.Append(SQLlist("UPDATE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetUpdateSQL() As String")
            Call vb.AppendLine(___UPDATE_SQL_Invoke(table, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine("#End Region")

#Region "Clone of the table data"
            Call vb.AppendLine(FixInvalids(table.TableName).__clone)
#End Region
            Call vb.AppendLine("End Class")

            Return vb.ToString
        End Function

        ''' <summary>
        ''' Code for invoke self ``MemberwiseClone``
        ''' </summary>
        ''' <param name="type$">Class name</param>
        ''' <returns></returns>
        <Extension>
        Private Function __clone(type$) As String
            Return _
            $"Public Function Clone() As {type}
                  Return DirectCast(MyClass.MemberwiseClone, {type})
              End Function"
        End Function

        ''' <summary>
        ''' ```SQL
        ''' ....
        ''' ```
        ''' </summary>
        ''' <param name="sql"></param>
        ''' <returns></returns>
        <Extension>
        Private Function SQLComments(sql As Value(Of String)) As String
            Dim sb As New StringBuilder
            Call sb.AppendLine("''' <summary>")
            Call sb.AppendLine("''' ```SQL")
            Call sb.AppendLine("''' " & sql.ToString)
            Call sb.AppendLine("''' ```")
            Call sb.AppendLine("''' </summary>")

            Return sb.ToString
        End Function

#Region "INSERT_SQL 假若有列是被标记为自动增长的，则不需要在INSERT_SQL之中在添加他的值了"

        Private Function ___REPLACE_SQL(Schema As Reflection.Schema.Table, TrimAutoIncrement As Boolean, ByRef SQL As Value(Of String)) As String
            Return __replaceInsertCommon(Schema, TrimAutoIncrement, True, SQL)
        End Function

        Private Function ___REPLACE_SQL_Invoke(Schema As Reflection.Schema.Table, TrimAutoIncrement As Boolean, refConflict As Boolean) As String
            Return __replaceInsertInvokeCommon(Schema, TrimAutoIncrement, True, refConflict)
        End Function

        Private Function __INSERT_VALUES(schema As Reflection.Schema.Table, trimAutoIncrement As Boolean) As String
            Dim sb As New StringBuilder
            Dim values$ = Reflection.SQL.SqlGenerateMethods.GenerateInsertValues(schema, trimAutoIncrement)

            For Each field As SeqValue(Of Field) In If(
                trimAutoIncrement,
                schema.Fields.Where(Function(f) Not f.AutoIncrement),
                schema.Fields).SeqIterator

                ' 在代码之中应该是propertyName而不是数据库之中的fieldName
                ' 因为schema对象是直接从SQL之中解析出来的，所以反射属性为空
                ' 在这里使用TrimKeyword(Field.FieldName)来生成代码之中的属性的名称
                values = values.Replace("{" & field.i & "}", "{" & FixInvalids((+field).FieldName) & "}")
            Next

            Call sb.AppendLine($"    Public Overrides Function {NameOf(MySQLTable.GetDumpInsertValue)}() As String")
            Call sb.AppendLine($"        Return $""{values}""")
            Call sb.AppendLine($"    End Function")

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 因为schema对象是直接从SQL之中解析出来的，所以反射属性为空
        ''' 在这里使用TrimKeyword(Field.FieldName)来生成代码之中的属性的名称
        ''' </summary>
        ''' <param name="field"></param>
        ''' <returns></returns>
        <Extension>
        Private Function PropertyName(field As Field) As String
            Return FixInvalids(field.FieldName)
        End Function

        ''' <summary>
        ''' 生成``INSERT INTO``和``REPLACE INTO``部分所共同的语句
        ''' </summary>
        ''' <param name="Schema"></param>
        ''' <param name="AI_strip"></param>
        ''' <param name="isReplace"></param>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Private Function __replaceInsertCommon(Schema As Table,
                                               AI_strip As Boolean,
                                               isReplace As Boolean,
                                               ByRef SQL As Value(Of String)) As String

            Dim Name As String = If(isReplace, "REPLACE", "INSERT")
            Dim SqlBuilder As New StringBuilder($"    Private Shared ReadOnly {Name}_SQL As String = <SQL>%s</SQL>")
            SQL.Value = Reflection.SQL.SqlGenerateMethods.GenerateInsertSql(Schema, AI_strip)

            If isReplace Then
                SQL = SQL.Value.Replace("INSERT INTO", "REPLACE INTO")
            End If

            Call SqlBuilder.Replace("%s", SQL.ToString)

            Return SqlBuilder.ToString
        End Function

        Private Function __replaceInsertInvokeCommon(Schema As Table,
                                                     TrimAutoIncrement As Boolean,
                                                     Replace As Boolean,
                                                     refConflict As Boolean) As String

            Dim SqlBuilder As New StringBuilder("        ")
            Dim Name As String = If(Replace, "REPLACE", "INSERT")
            Call SqlBuilder.Append($"Return String.Format({Name}_SQL, ")
            If Not TrimAutoIncrement Then
                Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Select __getExprInvoke(Field, refConflict)).ToArray))
            Else
                Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Where Not Field.AutoIncrement Select __getExprInvoke(Field, refConflict)).ToArray))
            End If
            Call SqlBuilder.Append(")")

            Return SqlBuilder.ToString
        End Function

        Private Function ___INSERT_SQL(Schema As Table, TrimAutoIncrement As Boolean, ByRef SQL As Value(Of String)) As String
            Return __replaceInsertCommon(Schema, TrimAutoIncrement, False, SQL)
        End Function

        Private Function ___INSERT_SQL_Invoke(Schema As Reflection.Schema.Table, TrimAutoIncrement As Boolean, refConflict As Boolean) As String
            Return __replaceInsertInvokeCommon(Schema, TrimAutoIncrement, False, refConflict)
        End Function
#End Region

        Private Function ___UPDATE_SQL(Schema As Reflection.Schema.Table, ByRef SQL As Value(Of String)) As String
            Dim SqlBuilder As New StringBuilder("    Private Shared ReadOnly UPDATE_SQL As String = <SQL>%s</SQL>")
            SQL.Value = Reflection.SQL.SqlGenerateMethods.GenerateUpdateSql(Schema)
            Call SqlBuilder.Replace("%s", SQL.Value)

            Return SqlBuilder.ToString
        End Function

        Private Function ___UPDATE_SQL_Invoke(Schema As Reflection.Schema.Table, refConflict As Boolean) As String
            Dim PrimaryKeys = Schema.GetPrimaryKeyFields

            If PrimaryKeys.IsNullOrEmpty Then
                Return NameOf(___UPDATE_SQL_Invoke).__notImplementForIndex
            End If

            Dim SqlBuilder As StringBuilder = New StringBuilder("        ")
            Call SqlBuilder.Append("Return String.Format(UPDATE_SQL, ")
            Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Select __getExprInvoke(Field, refConflict)).ToArray))
            Call SqlBuilder.Append(", " & String.Join(", ", PrimaryKeys.Select(Function(field) __getExprInvoke(field, refConflict)).ToArray) & ")")

            Return SqlBuilder.ToString
        End Function

        Private Function ___DELETE_SQL(Schema As Reflection.Schema.Table, ByRef SQL As Value(Of String)) As String
            Dim SqlBuilder As StringBuilder = New StringBuilder("    Private Shared ReadOnly DELETE_SQL As String = <SQL>%s</SQL>")
            SQL.Value = Reflection.SQL.SqlGenerateMethods.GenerateDeleteSql(Schema)
            Call SqlBuilder.Replace("%s", SQL.Value)

            Return SqlBuilder.ToString
        End Function

        <Extension>
        Private Function __notImplementForIndex(method$) As String
            Return "        " & $"Throw New NotImplementedException(""Table key was Not found, unable To generate {method } automatically, please edit this Function manually!"")"
        End Function

        Private Function ___DELETE_SQL_Invoke(Schema As Reflection.Schema.Table, refConflict As Boolean) As String
            Try
                Dim SqlBuilder As String
                Dim PrimaryKeys As Reflection.Schema.Field() = Schema.GetPrimaryKeyFields
                Dim refInvoke As String = String.Join(", ", PrimaryKeys.Select(Function(field) __getExprInvoke(field, refConflict)).ToArray)

                If PrimaryKeys.IsNullOrEmpty Then
                    GoTo NO_KEY
                End If

                SqlBuilder = $"Return String.Format(DELETE_SQL, {refInvoke})"
                SqlBuilder = "        " & SqlBuilder

                Return SqlBuilder.ToString
            Catch ex As Exception
                Call App.LogException(ex)
            End Try
NO_KEY:
            Return NameOf(___DELETE_SQL_Invoke).__notImplementForIndex
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Field"></param>
        ''' <returns></returns>
        Private Function __getExprInvoke(Field As Reflection.Schema.Field, dtype_conflicts As Boolean) As String
            If Field.DataType.MySQLType = Reflection.DbAttributes.MySqlDbType.Date OrElse
                Field.DataType.MySQLType = Reflection.DbAttributes.MySqlDbType.DateTime Then
                If dtype_conflicts Then
                    Dim ref As String = GetType(Reflection.DbAttributes.DataType).FullName
                    Return $"{ref}.ToMySqlDateTimeString({FixInvalids(Field.FieldName)})"
                Else
                    Return $"DataType.ToMySqlDateTimeString({FixInvalids(Field.FieldName)})"
                End If
            Else
                Return FixInvalids(Field.FieldName)
            End If
        End Function

        ReadOnly DataTypeFullNamesapce As String = GetType(Reflection.DbAttributes.MySqlDbType).Name
        ''' <summary>
        ''' 这个是为了兼容sciBASIC之中的csv序列化而设置的属性
        ''' </summary>
        ReadOnly columnMappingType As Type = GetType(ColumnAttribute)

        ''' <summary>
        ''' Apply the custom attribute on the property 
        ''' </summary>
        ''' <param name="Field"></param>
        ''' <param name="IsPrimaryKey"></param>
        ''' <returns></returns>
        <Extension> Private Function __createAttribute(Field As Field, IsPrimaryKey As Boolean) As String
            Dim Code As String = $"    <DatabaseField(""{Field.FieldName}"")"

            If IsPrimaryKey Then
                Code &= ", PrimaryKey"
            End If
            If Field.AutoIncrement Then
                Code &= ", AutoIncrement"
            End If
            If Field.NotNull Then
                Code &= ", NotNull"
            End If

            Code &= $", DataType({DataTypeFullNamesapce}.{Field.DataType.MySQLType.ToString}{If(String.IsNullOrEmpty(Field.DataType.ParameterValue), "", ", """ & Field.DataType.ParameterValue & """")})"
            Code &= $", Column(Name:=""{Field.FieldName}"")"

            If IsPrimaryKey Then
                Code &= ", XmlAttribute"
            End If

            Code &= "> "
            Return Code
        End Function

        ''' <summary>
        ''' Convert the sql definition into the visualbasic source code.
        ''' </summary>
        ''' <param name="SqlDump">The SQL dumping file path.(Dump sql文件的文件路径)</param>
        ''' <returns>VisualBasic source code</returns>
        ''' <remarks></remarks>
        Public Function GenerateCode(SqlDump As String, Optional [Namespace] As String = "") As String
            Return GenerateCode(New StreamReader(New FileStream(SqlDump, FileMode.Open)), [Namespace], SqlDump)
        End Function

        Public Function GenerateCode(file As StreamReader,
                                     Optional [Namespace] As String = "",
                                     Optional path As String = Nothing) As String

            Dim SqlDump As String = ""
            Dim Schema As Reflection.Schema.Table() = SQLParser.LoadSQLDoc(file, SqlDump)
            Dim CreateTables As String() = Regex.Split(SqlDump, SCHEMA_SECTIONS)
            Dim SchemaSQLLQuery = From tbl As String
                                  In CreateTables.Skip(1)           ' The first block of the text splits is the SQL comments from the MySQL data exporter. 
                                  Let tableName As String = Regex.Match(tbl, "`.+?`").Value
                                  Select tableName = Mid(tableName, 2, Len(tableName) - 2),
                                      tbl
            Dim SchemaSQL As Dictionary(Of String, String) =
                SchemaSQLLQuery _
                .ToDictionary(Function(x) x.tableName,
                              Function(x) x.tbl)

            Return __generateCode(
                Schema,
                head:=CreateTables.First,
                fileName:=FileIO.FileSystem.GetFileInfo(path).Name,
                TableSql:=SchemaSQL,
                [namespace]:=[Namespace])
        End Function

        ''' <summary>
        ''' 返回 {类名, 类定义}
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="SqlDump">The SQL dumping file path.(Dump sql文件的文件路径)</param>
        ''' <param name="ns">The namespace of the source code classes</param>
        Public Function GenerateCodeSplit(SqlDump As String, Optional ns As String = "") As Dictionary(Of String, String)
            Return GenerateCodeSplit(New StreamReader(New FileStream(SqlDump, FileMode.Open)), ns, SqlDump)
        End Function

        ''' <summary>
        ''' 返回 {类名, 类定义}
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="file">The SQL dumping file path.(Dump sql文件的文件路径)</param>
        ''' <param name="ns">The namespace of the source code classes</param>
        Public Function GenerateCodeSplit(file As StreamReader,
                                          Optional ns$ = "",
                                          Optional path$ = Nothing,
                                          Optional AI As Boolean = False) As Dictionary(Of String, String)

            Dim sqlDump As String = Nothing
            Dim Schema As Table() = SQLParser.LoadSQLDoc(file, sqlDump)
            Dim CreateTables As String() = Regex.Split(sqlDump, SCHEMA_SECTIONS)
            Dim SchemaSQLLQuery = From tbl As String
                                  In CreateTables.Skip(1)          ' The first block of the text splits is the SQL comments from the MySQL data exporter. 
                                  Let s_TableName As String = Regex.Match(tbl, "`.+?`").Value
                                  Select tableName = Mid(s_TableName, 2, Len(s_TableName) - 2),
                                      tbl
            Dim SchemaSQL As Dictionary(Of String, String) = Nothing
            Try
                SchemaSQL = SchemaSQLLQuery _
                    .ToDictionary(Function(x) x.tableName,
                                  Function(x) x.tbl)
            Catch ex As Exception
                Dim g = SchemaSQLLQuery.ToArray.CheckDuplicated(Of String)(Function(x) x.tableName)
                Dim dupliTables As String = String.Join(", ", g.Select(Function(tb) tb.Tag).ToArray)
                Throw New Exception("Duplicated tables:  " & dupliTables, ex)
            End Try

            Return __generateCodeSplit(
                Schema,
                Head:=CreateTables.First,
                FileName:=If(path.FileExists, FileIO.FileSystem.GetFileInfo(path).Name, ""),
                TableSql:=SchemaSQL,
                [Namespace]:=ns, AI:=AI)
        End Function

        ''' <summary>
        ''' Generate the source code file from the table schema dumping
        ''' </summary>
        ''' <param name="SqlDoc"></param>
        ''' <param name="Head"></param>
        ''' <param name="FileName"></param>
        ''' <param name="TableSql"></param>
        ''' <returns></returns>
        Private Function __generateCodeSplit(SqlDoc As IEnumerable(Of Table),
                                             Head$, FileName$,
                                             TableSql As Dictionary(Of String, String),
                                             Namespace$,
                                             AI As Boolean) As Dictionary(Of String, String)

            Dim haveNamespace As Boolean = Not String.IsNullOrEmpty([Namespace])

            If TableSql Is Nothing Then
                TableSql = New Dictionary(Of String, String)
            End If

            Dim classList = (From Table As Table
                             In SqlDoc
                             Let SqlDef As String = If(
                                 TableSql.ContainsKey(Table.TableName),
                                 TableSql(Table.TableName),
                                 "")
                             Let vbClass As String = Table.GenerateTableClass(SqlDef, stripAI:=Not AI)
                             Select classDef = vbClass,
                                 Table).ToArray
            Dim LQuery = (From table
                          In classList
                          Select table.Table,
                              doc = table.classDef.GenerateSingleDocument(haveNamespace, [Namespace])).ToArray
            Return LQuery.ToDictionary(
                Function(x) x.Table.TableName,
                Function(x) x.doc)
        End Function

        Private Function __schemaDb(DbName As String, ns As String) As String
            Dim classDef$ = {
                $"Public MustInherits Class {DbName}: Inherits {InheritsAbstract}",
                $"",
                $"End Class"
            }.JoinBy(ASCII.LF)

            Return classDef.GenerateSingleDocument(Not String.IsNullOrEmpty(ns), ns)
        End Function

        <Extension>
        Private Function GenerateSingleDocument(ClassDef$, haveNamespace As Boolean, namespace$) As String
            Dim VB As New StringBuilder(1024)

            Call VB.AppendLine($"REM  {GetType(CodeGenerator).FullName}")
            Call VB.AppendLine($"REM  MYSQL Schema Mapper")
            Call VB.AppendLine($"REM      for Microsoft VisualBasic.NET {GetType(CodeGenerator).ModuleVersion}")
            Call VB.AppendLine()
            Call VB.AppendLine($"REM  Dump @{Now.ToString}")
            Call VB.AppendLine()

            Call VB.AppendLine()
            Call VB.AppendLine("Imports " & LinqMappingNs)
            Call VB.AppendLine("Imports System.Xml.Serialization")
            Call VB.AppendLine("Imports " & LibMySQLReflectionNs)

            Call VB.AppendLine()

            If haveNamespace Then
                Call VB.AppendLine($"Namespace {[namespace]}")
            End If

            Call VB.AppendLine()
            Call VB.AppendLine(ClassDef)
            Call VB.AppendLine()

            If haveNamespace Then
                Call VB.AppendLine("End Namespace")
            End If

            Return VB.ToString
        End Function
    End Module
End Namespace
