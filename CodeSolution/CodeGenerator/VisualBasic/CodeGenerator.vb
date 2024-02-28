#Region "Microsoft.VisualBasic::01acce94920468d02d5b130750a48c0c, CodeSolution\VisualBasic\CodeGenerator.vb"

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
'                   __generateCodeSplit, __INSERT_VALUES, __notImplementForIndex, __replaceInsertCommon, __replaceInsertInvokeCommon
'                   __schemaDb, __toDataType, FixInvalids, GenerateClass, (+3 Overloads) GenerateCode
'                   (+2 Overloads) GenerateCodeSplit, GenerateSingleDocument, getExprInvoke, PropertyName, SQLComments
'                   VBClass, vbCode
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder.VBLanguage
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL
Imports ColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute
Imports DataType = Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes.DataType
Imports MySqlScript = Oracle.LinuxCompatibility.MySQL.Scripting.Extensions

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

            name = KeywordProcessor.AutoEscapeVBKeyword(name.Replace(" ", "_"))
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
            Return vbCode(listSQL, "", "", Nothing, [namespace])
        End Function

        Const SCHEMA_SECTIONS As String = "-- Table structure for table `.+?`"

        ''' <summary>
        ''' Generate the source code file from the table schema dumping.
        ''' (使用这个函数生成的程序源代码所有的``Class``类都是被放置在一个源文件之中的)
        ''' </summary>
        ''' <param name="Sql"></param>
        ''' <param name="head"></param>
        ''' <param name="FileName"></param>
        ''' <param name="tableSql"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function vbCode(Sql As IEnumerable(Of Table), head$, fileName$, tableSql As Dictionary(Of String, String), namespace$) As String
            Dim vb As New StringBuilder(1024)
            Dim haveNamespace As Boolean = Not String.IsNullOrEmpty([namespace])

            Call vb.AppendLine($"REM  {GetType(CodeGenerator).FullName}")
            Call vb.AppendLine($"REM  Microsoft VisualBasic MYSQL")
            Call vb.AppendLine()
            Call vb.AppendLine($"' SqlDump= {fileName}")
            Call vb.AppendLine()
            Call vb.AppendLine()

            If tableSql Is Nothing Then
                tableSql = New Dictionary(Of String, String)
            End If

            Dim tokens$() = Strings.Split(head.Replace(vbLf, ""), vbCr)
            Dim getSql = tableSql.GetValue

            For Each line As String In tokens
                Call vb.AppendLine("' " & line)
            Next

            Call vb.AppendLine()
            ' Call vb.AppendLine("Imports " & LinqMappingNs)
            Call vb.AppendLine("Imports System.Xml.Serialization")
            Call vb.AppendLine("Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps")
            Call vb.AppendLine("Imports " & LibMySQLReflectionNs)
            Call vb.AppendLine("Imports " & $"MySqlScript = {GetType(MySqlScript).FullName}")
            Call vb.AppendLine()

            If haveNamespace Then
                Call vb.AppendLine($"Namespace {[namespace]}")
            End If

            For Each line As String In From table As Table
                                       In Sql
                                       Let SqlDef As String = getSql(table.TableName)
                                       Select table.VBClass(SqlDef)
                Call vb.AppendLine()
                Call vb.AppendLine(line)
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
        ''' <returns></returns>
        ''' <remarks><see cref="SQLComments"/></remarks>
        <Extension> Public Function VBClass(table As Table, DefSql$) As String
            Dim tokens$() = DefSql.LineTokens
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

                ' Apply the custom attribute on the property 
                Call vb.Append(Field.__createAttribute(IsPrimaryKey:=table.PrimaryFields.Contains(Field.FieldName)))
                ' Generate the property name 
                Call vb.Append("Public Property " & FixInvalids(Field.FieldName))
                ' Generate the property data type
                Call vb.Append(__toDataType(Field.DataType))
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
            Call vb.AppendLine(___INSERT_SQL(table, True, SQLlist("INSERT")))
            Call vb.AppendLine(___INSERT_SQL(table, False, SQLlist("INSERT")))
            Call vb.AppendLine(___REPLACE_SQL(table, True, SQLlist("REPLACE")))
            Call vb.AppendLine(___REPLACE_SQL(table, False, SQLlist("REPLACE")))
            Call vb.AppendLine(___DELETE_SQL(table, SQLlist("DELETE")))
            Call vb.AppendLine(___UPDATE_SQL(table, SQLlist("UPDATE")))
            Call vb.AppendLine("#End Region")
            Call vb.AppendLine()
            Call vb.Append(SQLlist("DELETE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetDeleteSQL() As String")
            Call vb.AppendLine(___DELETE_SQL_Invoke(table, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.Append(SQLlist("INSERT").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetInsertSQL() As String")
            Call vb.AppendLine(___INSERT_SQL_Invoke(table, True, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.Append(SQLlist("INSERT").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetInsertSQL(AI As Boolean) As String")
            Call vb.AppendLine("        If AI Then")
            Call vb.AppendLine(___INSERT_SQL_Invoke(table, False, refConflict))
            Call vb.AppendLine("        Else")
            Call vb.AppendLine(___INSERT_SQL_Invoke(table, True, refConflict))
            Call vb.AppendLine("        End If")
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.AppendLine("''' <summary>")
            Call vb.AppendLine($"''' <see cref=""{NameOf(MySQLTable.GetInsertSQL)}""/>")
            Call vb.AppendLine("''' </summary>")
            Call vb.AppendLine(__INSERT_VALUES(table))
            Call vb.AppendLine()
            Call vb.Append(SQLlist("REPLACE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetReplaceSQL() As String")
            Call vb.AppendLine(___REPLACE_SQL_Invoke(table, True, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.Append(SQLlist("REPLACE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetReplaceSQL(AI As Boolean) As String")
            Call vb.AppendLine("        If AI Then")
            Call vb.AppendLine(___REPLACE_SQL_Invoke(table, False, refConflict))
            Call vb.AppendLine("        Else")
            Call vb.AppendLine(___REPLACE_SQL_Invoke(table, True, refConflict))
            Call vb.AppendLine("        End If")
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine()
            Call vb.Append(SQLlist("UPDATE").SQLComments)
            Call vb.AppendLine("    Public Overrides Function GetUpdateSQL() As String")
            Call vb.AppendLine(___UPDATE_SQL_Invoke(table, refConflict))
            Call vb.AppendLine("    End Function")
            Call vb.AppendLine("#End Region")
            Call vb.AppendLine()

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
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function __clone(type$) As String
            Return $"''' <summary>
                     ''' Memberwise clone of current table Object.
                     ''' </summary>
                     Public Function Clone() As {type}
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

        Private Function ___REPLACE_SQL(Schema As Table, TrimAutoIncrement As Boolean, ByRef SQL As Value(Of String)) As String
            Return __replaceInsertCommon(Schema, TrimAutoIncrement, True, SQL)
        End Function

        Private Function ___REPLACE_SQL_Invoke(Schema As Table, TrimAutoIncrement As Boolean, refConflict As Boolean) As String
            Return __replaceInsertInvokeCommon(Schema, TrimAutoIncrement, True, refConflict)
        End Function

        Private Function __INSERT_VALUES(schema As Table) As String
            Dim sb As New StringBuilder

            Call sb.AppendLine($"    Public Overrides Function {NameOf(MySQLTable.GetDumpInsertValue)}(AI As Boolean) As String")
            Call sb.AppendLine($"        If AI Then")
            Call sb.AppendLine($"            Return $""{schema.valueRef(False)}""")
            Call sb.AppendLine($"        Else")
            Call sb.AppendLine($"            Return $""{schema.valueRef(True)}""")
            Call sb.AppendLine($"        End If")
            Call sb.AppendLine($"    End Function")

            Return sb.ToString
        End Function

        <Extension>
        Private Function valueRef(schema As Table, trimAutoIncrement As Boolean) As String
            Dim values$ = SqlGenerateMethods.GenerateInsertValues(schema, trimAutoIncrement)
            Dim fields As IEnumerable(Of Field)

            If trimAutoIncrement Then
                fields = schema.Fields.Where(Function(f) Not f.AutoIncrement)
            Else
                fields = schema.Fields
            End If

            For Each field As SeqValue(Of Field) In fields.SeqIterator
                ' 20230320 deal with the datetime value
                Dim type = field.value.DataType.MySQLType
                Dim fieldValue As String

                If type = MySqlDbType.Date OrElse type = MySqlDbType.DateTime Then
                    fieldValue = FixInvalids((+field).FieldName) & ".ToString(""yyyy-MM-dd hh:mm:ss"")"
                Else
                    fieldValue = FixInvalids((+field).FieldName)
                End If

                ' 在代码之中应该是propertyName而不是数据库之中的fieldName
                ' 因为schema对象是直接从SQL之中解析出来的，所以反射属性为空
                ' 在这里使用TrimKeyword(Field.FieldName)来生成代码之中的属性的名称
                values = values.Replace("{" & field.i & "}", "{" & fieldValue & "}")
            Next

            Return values
        End Function

        ''' <summary>
        ''' 因为schema对象是直接从SQL之中解析出来的，所以反射属性为空
        ''' 在这里使用TrimKeyword(Field.FieldName)来生成代码之中的属性的名称
        ''' </summary>
        ''' <param name="field"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function PropertyName(field As Field) As String
            Return FixInvalids(field.FieldName)
        End Function

        ''' <summary>
        ''' 生成``INSERT INTO``和``REPLACE INTO``部分所共同的语句
        ''' </summary>
        ''' <param name="Schema"></param>
        ''' <param name="AI_strip">如果这个参数为真，则在生成的SQL语句之中将不会包含自增的字段</param>
        ''' <param name="isReplace"></param>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Private Function __replaceInsertCommon(Schema As Table,
                                               AI_strip As Boolean,
                                               isReplace As Boolean,
                                               ByRef SQL As Value(Of String)) As String

            Dim name$ = If(isReplace, "REPLACE", "INSERT")
            Dim SqlBuilder As New StringBuilder($"    Friend Shared ReadOnly {name}{If(AI_strip, "", "_AI")}_SQL$ = ")

            SQL.Value = SqlGenerateMethods.GenerateInsertSql(Schema, AI_strip)

            If isReplace Then
                SQL = SQL.Value.Replace("INSERT INTO", "REPLACE INTO")
            End If

            Call SqlBuilder.AppendLine()
            Call SqlBuilder.AppendLine("        " & sprintf(<SQL>%s</SQL>, SQL.Value))

            Return SqlBuilder.ToString
        End Function

        Private Function __replaceInsertInvokeCommon(Schema As Table,
                                                     TrimAutoIncrement As Boolean,
                                                     Replace As Boolean,
                                                     refConflict As Boolean) As String

            Dim SqlBuilder As New StringBuilder("        ")
            Dim Name As String = If(Replace, "REPLACE", "INSERT") & If(TrimAutoIncrement, "", "_AI")
            Call SqlBuilder.Append($"Return String.Format({Name}_SQL, ")
            If Not TrimAutoIncrement Then
                Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Select getExprInvoke(Field, refConflict)).ToArray))
            Else
                Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Where Not Field.AutoIncrement Select getExprInvoke(Field, refConflict)).ToArray))
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
            Dim SqlBuilder As New StringBuilder("    Friend Shared ReadOnly UPDATE_SQL$ = ")
            SQL.Value = SqlGenerateMethods.GenerateUpdateSql(Schema)
            Call SqlBuilder.AppendLine()
            Call SqlBuilder.AppendLine("        " & sprintf(<SQL>%s</SQL>, SQL.Value))

            Return SqlBuilder.ToString
        End Function

        Private Function ___UPDATE_SQL_Invoke(Schema As Reflection.Schema.Table, refConflict As Boolean) As String
            Dim PrimaryKeys = Schema.GetPrimaryKeyFields

            If PrimaryKeys.IsNullOrEmpty Then
                Return NameOf(___UPDATE_SQL_Invoke).__notImplementForIndex
            End If

            Dim SqlBuilder As StringBuilder = New StringBuilder("        ")
            Call SqlBuilder.Append("Return String.Format(UPDATE_SQL, ")
            Call SqlBuilder.Append(String.Join(", ", (From Field In Schema.Fields Select getExprInvoke(Field, refConflict)).ToArray))
            Call SqlBuilder.Append(", " & String.Join(", ", PrimaryKeys.Select(Function(field) getExprInvoke(field, refConflict)).ToArray) & ")")

            Return SqlBuilder.ToString
        End Function

        Private Function ___DELETE_SQL(Schema As Reflection.Schema.Table, ByRef SQL As Value(Of String)) As String
            Dim SqlBuilder As New StringBuilder("    Friend Shared ReadOnly DELETE_SQL$ =")
            SQL.Value = SqlGenerateMethods.GenerateDeleteSql(Schema)
            Call SqlBuilder.AppendLine()
            Call SqlBuilder.AppendLine("        " & sprintf(<SQL>%s</SQL>, SQL.Value))

            Return SqlBuilder.ToString
        End Function

        <Extension>
        Private Function __notImplementForIndex(method$) As String
            Return "        " & $"Throw New NotImplementedException(""Table key was Not found, unable To generate {method } automatically, please edit this Function manually!"")"
        End Function

        Private Function ___DELETE_SQL_Invoke(schema As Table, refConflict As Boolean) As String
            Try
                Dim SqlBuilder As String
                Dim primaryKeys As Field() = schema.GetPrimaryKeyFields
                Dim refInvoke As String = primaryKeys _
                    .Select(Function(field)
                                Return getExprInvoke(field, refConflict)
                            End Function) _
                    .JoinBy(", ")

                If primaryKeys.IsNullOrEmpty Then
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
        ''' <param name="field"></param>
        ''' <returns></returns>
        Private Function getExprInvoke(field As Field, dtype_conflicts As Boolean) As String
            If field.DataType.MySQLType = MySqlDbType.Date OrElse
                field.DataType.MySQLType = MySqlDbType.DateTime Then

                If dtype_conflicts Then
                    Dim ref$ = GetType(MySqlScript).FullName
                    Return $"{ref}.ToMySqlDateTimeString({FixInvalids(field.FieldName)})"
                Else
                    Return $"MySqlScript.ToMySqlDateTimeString({FixInvalids(field.FieldName)})"
                End If
            Else
                Return FixInvalids(field.FieldName)
            End If
        End Function

        ReadOnly DataTypeFullNamesapce As String = GetType(MySqlDbType).Name
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
        <Extension>
        Private Function __createAttribute(Field As Field, IsPrimaryKey As Boolean) As String
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
            Dim Schema As Table() = SQLParser.LoadSQLDoc(file, SqlDump)
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

            Return Schema.vbCode(
                head:=CreateTables.First,
                fileName:=FileIO.FileSystem.GetFileInfo(path).Name,
                tableSql:=SchemaSQL,
                [namespace]:=[Namespace])
        End Function

        ''' <summary>
        ''' 返回 {类名, 类定义}
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="SqlDump">The SQL dumping file path.(Dump sql文件的文件路径)</param>
        ''' <param name="ns">The namespace of the source code classes</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GenerateCodeSplit(SqlDump$, Optional ns$ = "") As Dictionary(Of String, String)
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
                                          Optional path$ = Nothing) As Dictionary(Of String, String)

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
                                  Function(x)
                                      Return x.tbl
                                  End Function)
            Catch ex As Exception
                Dim g = SchemaSQLLQuery.ToArray.CheckDuplicated(Function(x) x.tableName)
                Dim dupliTables As String = g.Select(Function(tb) tb.Tag).JoinBy(", ")

                Throw New Exception("Duplicated tables:  " & dupliTables, ex)
            End Try

            Dim filename As String = If(path.FileExists, FileIO.FileSystem.GetFileInfo(path).Name, "")

            Return Schema.__generateCodeSplit(
                head:=CreateTables.First,
                FileName:=filename,
                TableSql:=SchemaSQL,
                [Namespace]:=ns
            )
        End Function

        ''' <summary>
        ''' Generate the source code file from the table schema dumping
        ''' </summary>
        ''' <param name="SqlDoc"></param>
        ''' <param name="head"></param>
        ''' <param name="FileName"></param>
        ''' <param name="TableSql"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Private Function __generateCodeSplit(SqlDoc As IEnumerable(Of Table),
                                             head$, FileName$,
                                             TableSql As Dictionary(Of String, String),
                                             Namespace$) As Dictionary(Of String, String)

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
                             Let vbClass As String = Table.VBClass(SqlDef)
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
            Call VB.AppendLine("Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps")
            Call VB.AppendLine("Imports " & LibMySQLReflectionNs)
            Call VB.AppendLine("Imports " & $"MySqlScript = {GetType(MySqlScript).FullName}")

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
