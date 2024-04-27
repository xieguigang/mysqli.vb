#Region "Microsoft.VisualBasic::95bb23f334896c18f864107dda5f9ced, G:/graphQL/src/mysqli/CodeSolution//SQL/SQLParser/CreateField.vb"

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


    ' Code Statistics:

    '   Total Lines: 90
    '    Code Lines: 71
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 4.32 KB


    '     Module CreateField
    ' 
    '         Function: (+2 Overloads) CreateField
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder.VBLanguage
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateField

        ''' <summary>
        ''' Regex expression for parsing the comments of the field in a table definition.
        ''' </summary>
        Const FIELD_COMMENTS As String = "COMMENT '.+?',"
        Const FIELD_DEFAULT As String = "DEFAULT\s+((" & NumericPattern & ")|(" & Patterns.Identifer & "\(\s*\))|('.*?'))"

        Private Function CreateField(fieldDef$, tokens$()) As Field
            Dim fieldName As String = tokens(0)
            Dim dataType As String = tokens(1)
            Dim commentText As String = r.Match(fieldDef, FIELD_COMMENTS).Value
            Dim i As Integer = InStr(fieldDef, fieldName)
            Dim defaultVal As String

            defaultVal = r.Match(fieldDef, FIELD_DEFAULT, RegexICSng).Value
            fieldDef = Mid(fieldDef, i + Len(fieldName))
            i = InStr(fieldDef, dataType)
            fieldDef = Mid(fieldDef, i + Len(dataType)).Replace(",", "").Trim
            fieldName = Mid(fieldName, 2, Len(fieldName) - 2)

            If Not String.IsNullOrEmpty(commentText) Then
                commentText = Mid(commentText, 10)
                commentText = commentText.Trim("'"c, ","c)
            End If
            If Not String.IsNullOrEmpty(defaultVal) Then
                defaultVal = defaultVal.Substring(7).Trim.Trim("'"c)
            Else
                defaultVal = Nothing
            End If

            Dim pos% = InStr(fieldDef, "COMMENT '", CompareMethod.Text)
            Dim p As i32 = 0

            If pos = 0 Then
                ' 没有注释，则百分之百就是列属性了
                pos = Integer.MaxValue
            End If

            Dim autoIncrement As Boolean = (p = InStr(fieldDef, "AUTO_INCREMENT", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim IsNotNull As Boolean = (p = InStr(fieldDef, "NOT NULL", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim unsigned As Boolean = (p = InStr(fieldDef, "UNSIGNED", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim zeroFill As Boolean = (p = InStr(fieldDef, "ZEROFILL", CompareMethod.Text)) > 0 AndAlso p < pos
            ' Some data type can be merged into a same type
            ' when we mapping a database table
            Dim field As New Field With {
                .FieldName = fieldName,
                .DataType = CreateDataType.CreateDataType(dataType.Replace(",", "").Trim),
                .Comment = commentText,
                .AutoIncrement = autoIncrement,
                .NotNull = IsNotNull,
                .Unsigned = unsigned,
                .ZeroFill = zeroFill,
                .[Default] = defaultVal
            }

            If unsigned Then
                Select Case field.DataType.MySQLType
                    Case Reflection.DbAttributes.MySqlDbType.Int16 : field.DataType.SetMySqlType(Reflection.DbAttributes.MySqlDbType.UInt16)
                    Case Reflection.DbAttributes.MySqlDbType.Int24 : field.DataType.SetMySqlType(Reflection.DbAttributes.MySqlDbType.UInt24)
                    Case Reflection.DbAttributes.MySqlDbType.Int32 : field.DataType.SetMySqlType(Reflection.DbAttributes.MySqlDbType.UInt32)
                    Case Reflection.DbAttributes.MySqlDbType.Int64 : field.DataType.SetMySqlType(Reflection.DbAttributes.MySqlDbType.UInt64)
                End Select
            End If

            Return field
        End Function

        <Extension>
        Public Function CreateField(fieldDef As String) As Reflection.Schema.Field
            Dim name$ = r.Match(fieldDef, "`.+?`", RegexICSng).Value
            Dim tokens$() = {name}.Join(fieldDef.Replace(name, "").Trim.Split)

            Try
                Return CreateField(fieldDef, tokens)
            Catch ex As Exception
                Throw New Exception($"{NameOf(CreateField)} ===>  {fieldDef}{vbCrLf & vbCrLf & vbCrLf}", ex)
            End Try
        End Function
    End Module
End Namespace
