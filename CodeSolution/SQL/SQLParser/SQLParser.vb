#Region "Microsoft.VisualBasic::48960782e360995cf3c6f912bfa3c6b7, CodeSolution\SQL\SQLParser.vb"

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

' Module SQLParser
' 
'     Function: __createDataType, (+2 Overloads) __createField, __createSchema, __createSchemaInner, __getDBName
'               __getNumberValue, __parseTable, __splitInternal, __sqlParser, (+2 Overloads) LoadSQLDoc
'               LoadSQLDocFromStream, ParseTable
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
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Public Module SQLParser

        Public Function ParseTable(SQL As String) As Reflection.Schema.Table
            Dim createTableSQL As String = RegexpParser.GetCreateTableSQLText(SQL)
            Dim tokens As NamedValue(Of String()) = __sqlParser(createTableSQL.Replace(vbLf, vbCr))

            Try
                Return __parseTable(SQL, tokens)
            Catch ex As Exception
                Dim dump As StringBuilder = New StringBuilder
                Call dump.AppendLine(SQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(NameOf(createTableSQL) & "   ====> ")
                Call dump.AppendLine(createTableSQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine($"TableName:={tokens.Name}")
                Call dump.AppendLine(New String("-"c, 120))
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(String.Join(vbCrLf & "  >  ", tokens.Value))

                Throw New Exception(dump.ToString, ex)
            End Try
        End Function

        Private Function __parseTable(SQL As String, Tokens As NamedValue(Of String())) As Reflection.Schema.Table
            Dim DB As String = GetDBName(SQL)
            Dim TableName As String = Tokens.Value(Scan0)
            Dim PrimaryKey As String = Tokens.Name
            Dim FieldsTokens As String() = Tokens.Value.Skip(1).ToArray
            Dim Table As Table = SetValue(Of Table).InvokeSet(
                __createSchema(FieldsTokens,
                               TableName,
                               PrimaryKey,
                               SQL, comment:=Nothing),
                NameOf(Reflection.Schema.Table.Database),
                DB)
            Return Table
        End Function

        ''' <summary>
        ''' Loading the table schema from a specific SQL doucment.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function LoadSQLDoc(path$) As Table()
            Using file As New StreamReader(New FileStream(path, FileMode.Open))
                Return file.LoadSQLDoc
            End Using
        End Function

        Public Function LoadSQLDocFromStream(sqlDoc As String) As Table()
            Dim DB As String = GetDBName(sqlDoc)
            Dim tables = (From table As String
                          In sqlDoc.SplitTableCreateInternal
                          Let tokens As TableTokens = TableTokens.ParseTokens(sql:=table)
                          Select tokens).ToArray
            Dim setValue = New SetValue(Of Table)().GetSet(NameOf(Table.Database))
            Dim SqlSchema = LinqAPI.Exec(Of Table) _
                                                   _
            () <= From table
                  In tables
                  Let tbl As Table = __createSchema(
                      table.fields,
                      table.tableName,
                      table.primaryKey,
                      table.original,
                      comment:=table.comment)
                  Select setValue(tbl, DB)

            Return SqlSchema
        End Function

        <Extension>
        Public Function LoadSQLDoc(stream As StreamReader, Optional ByRef raw As String = Nothing) As Table()
            With stream.ReadToEnd.Replace("<", "&lt;")
                raw = .ByRef
                Return LoadSQLDocFromStream(.ByRef)
            End With
        End Function

        ''' <summary>
        ''' Create a MySQL table schema object.
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <param name="tableName"></param>
        ''' <param name="primaryKey"></param>
        ''' <param name="createTableSQL"></param>
        ''' <returns></returns>
        Private Function __createSchema(fields As String(), tableName As String, primaryKey As String, createTableSQL As String, comment As String) As Reflection.Schema.Table
            Try
                Dim table As Table = __createSchemaInner(fields, tableName, primaryKey, createTableSQL)
                table.Comment = comment
                Return table
            Catch ex As Exception

                With New StringBuilder
                    Call .AppendLine(NameOf(createTableSQL))
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(createTableSQL)
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine($"{NameOf(tableName)}   ===>  {tableName}")
                    Call .AppendLine($"{NameOf(primaryKey)}  ===>  {primaryKey}")
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine(NameOf(fields))
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(String.Join(vbCrLf, fields))

                    Throw New Exception(.ToString, ex)
                End With

            End Try
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <param name="tableName"></param>
        ''' <param name="primaryKey"></param>
        ''' <param name="createTableSQL">Create table SQL raw.</param>
        ''' <returns></returns>
        Private Function __createSchemaInner(fields$(), tableName$, primaryKey$, createTableSQL$) As Table
            Dim primaryKeys$()

            tableName = r.Matches(tableName, "`.+?`").ToArray.Last
            tableName = Mid(tableName, 2, Len(tableName) - 2)
            primaryKey = r.Match(primaryKey, "\(`.+?`\)").Value

            If Not String.IsNullOrEmpty(primaryKey) Then
                primaryKey = r.Replace(primaryKey, "\(\d+\)", "")
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKeys = Strings.Split(primaryKey, "`,`")
            Else
                primaryKeys = New String() {}
            End If

            Dim comment As String = r.Match(createTableSQL, "COMMENT='.+';", RegexOptions.Singleline).Value
            Dim fieldList = fields _
            .Select(AddressOf __createField) _
            .ToDictionary(Function(field) field.FieldName)

            If Not String.IsNullOrEmpty(comment) Then
                comment = Mid(comment, 10)
                comment = Mid(comment, 1, Len(comment) - 2)
            End If

            createTableSQL = ASCII.ReplaceQuot(createTableSQL, "\'")

            ' The database fields reflection result {Name, Attribute}
            Dim tableSchema As New Table(fieldList) With {
            .TableName = tableName,
            .PrimaryFields = primaryKeys.AsList,   ' Assuming at least only one primary key in a table
            .Index = primaryKey,
            .Comment = comment,
            .SQL = createTableSQL
        }
            Return tableSchema
        End Function

        ''' <summary>
        ''' Regex expression for parsing the comments of the field in a table definition.
        ''' </summary>
        Const FIELD_COMMENTS As String = "COMMENT '.+?',"

        Private Function __createField(fieldDef$, tokens$()) As Field
            Dim FieldName As String = tokens(0)
            Dim DataType As String = tokens(1)
            Dim Comment As String = Regex.Match(fieldDef, FIELD_COMMENTS).Value
            Dim i As Integer = InStr(fieldDef, FieldName)

            fieldDef = Mid(fieldDef, i + Len(FieldName))
            i = InStr(fieldDef, DataType)
            fieldDef = Mid(fieldDef, i + Len(DataType)).Replace(",", "").Trim
            FieldName = Mid(FieldName, 2, Len(FieldName) - 2)

            If Not String.IsNullOrEmpty(Comment) Then
                Comment = Mid(Comment, 10)
                Comment = Mid(Comment, 1, Len(Comment) - 2)
            End If

            Dim pos% = InStr(fieldDef, "COMMENT '", CompareMethod.Text)
            Dim p As i32 = 0

            If pos = 0 Then  '没有注释，则百分之百就是列属性了
                pos = Integer.MaxValue
            End If

            Dim IsAutoIncrement As Boolean = (p = InStr(fieldDef, "AUTO_INCREMENT", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim IsNotNull As Boolean = (p = InStr(fieldDef, "NOT NULL", CompareMethod.Text)) > 0 AndAlso p < pos

            Dim field As New Field With {
            .FieldName = FieldName,
            .DataType = __createDataType(DataType.Replace(",", "").Trim),  ' Some data type can be merged into a same type when we mapping a database table
            .Comment = Comment,
            .AutoIncrement = IsAutoIncrement,
            .NotNull = IsNotNull
        }
            Return field
        End Function

        Private Function __createField(fieldDef As String) As Reflection.Schema.Field
            Dim name$ = r.Match(fieldDef, "`.+?`", RegexICSng).Value
            Dim tokens$() = {name}.Join(fieldDef.Replace(name, "").Trim.Split)

            Try
                Return __createField(fieldDef, tokens)
            Catch ex As Exception
                Throw New Exception($"{NameOf(__createField)} ===>  {fieldDef}{vbCrLf & vbCrLf & vbCrLf}", ex)
            End Try
        End Function

        ''' <summary>
        ''' Mapping the MySQL database type and visual basic data type 
        ''' </summary>
        ''' <param name="type_define"></param>
        ''' <returns></returns>
        Private Function __createDataType(type_define$) As Reflection.DbAttributes.DataType
            Dim type As Reflection.DbAttributes.MySqlDbType
            Dim parameter As String = ""

            If type_define.TextEquals("tinyint") OrElse r.Match(type_define, "tinyint\(\d+\)", RegexOptions.IgnoreCase).Success Then
                parameter = __getNumberValue(type_define, 1)

                If parameter = "1" Then
                    ' boolean 
                    type = Reflection.DbAttributes.MySqlDbType.Boolean
                Else
                    type = Reflection.DbAttributes.MySqlDbType.Int32
                End If

            ElseIf "int".TextEquals(type_define) OrElse r.Match(type_define, "int\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Int64
                parameter = __getNumberValue(type_define, 11)

            ElseIf Regex.Match(type_define, "varchar\(\d+\)", RegexOptions.IgnoreCase).Success OrElse Regex.Match(type_define, "char\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.VarChar
                parameter = __getNumberValue(type_define, 45)

            ElseIf Regex.Match(type_define, "double", RegexOptions.IgnoreCase).Success OrElse InStr(type_define, "float", CompareMethod.Text) > 0 Then
                type = Reflection.DbAttributes.MySqlDbType.Double

            ElseIf Regex.Match(type_define, "datetime", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "date", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "timestamp", RegexOptions.IgnoreCase).Success Then

                type = Reflection.DbAttributes.MySqlDbType.DateTime

            ElseIf Regex.Match(type_define, "text", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Text

            ElseIf InStr(type_define, "enum(", CompareMethod.Text) > 0 Then   ' enum类型转换为String类型？？？？
                type = Reflection.DbAttributes.MySqlDbType.String

            ElseIf InStr(type_define, "Blob", CompareMethod.Text) > 0 OrElse
            Regex.Match(type_define, "varbinary\(\d+\)", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "binary\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Blob

            ElseIf Regex.Match(type_define, "decimal\(", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Decimal

            ElseIf Regex.Match(type_define, "bit\(", RegexICSng).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Bit
                parameter = __getNumberValue(type_define, 1)

            Else

                'More complex type is not support yet, but you can easily extending the mapping code at here
                Throw New NotImplementedException($"Type define is not support yet for    {NameOf(type_define)}   >>> ""{type_define}""")

            End If

            Return New Reflection.DbAttributes.DataType(type, parameter)
        End Function

        Private Function __getNumberValue(typeDef$, default$) As String
            Dim parameter$ = r.Match(typeDef, "\(.+?\)").Value

            If parameter.StringEmpty Then
                Return [default]
            Else
                parameter = Mid(parameter, 2, Len(parameter) - 2)
                Return parameter
            End If
        End Function
    End Module
End Namespace