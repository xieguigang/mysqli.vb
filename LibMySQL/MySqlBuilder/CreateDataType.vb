#Region "Microsoft.VisualBasic::eec6dd2db926b994d034951b51f081de, src\mysqli\LibMySQL\MySqlBuilder\CreateDataType.vb"

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

    '   Total Lines: 86
    '    Code Lines: 59
    ' Comment Lines: 8
    '   Blank Lines: 19
    '     File Size: 4.30 KB


    '     Module CreateDataType
    ' 
    '         Function: CreateDataType, GetNumberValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace MySqlBuilder

    Public Module CreateDataType

        ''' <summary>
        ''' Mapping the MySQL database type and visual basic data type 
        ''' </summary>
        ''' <param name="type_define"></param>
        ''' <returns></returns>
        Public Function CreateDataType(type_define$) As Reflection.DbAttributes.DataType
            Dim type As Reflection.DbAttributes.MySqlDbType
            Dim parameter As String = ""

            If type_define = "tinyint unsigned" Then
                type = Reflection.DbAttributes.MySqlDbType.Byte
            ElseIf type_define.TextEquals("tinyint") OrElse r.Match(type_define, "tinyint\(\d+\)", RegexOptions.IgnoreCase).Success Then
                parameter = GetNumberValue(type_define, 1)

                If parameter = "1" Then
                    ' 20240905
                    ' byte
                    type = Reflection.DbAttributes.MySqlDbType.Byte
                Else
                    type = Reflection.DbAttributes.MySqlDbType.Int32
                End If

            ElseIf "int".TextEquals(type_define) OrElse r.Match(type_define, "int\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Int32
                parameter = GetNumberValue(type_define, 11)

            ElseIf type_define.TextEquals("int unsigned") Then
                type = Reflection.DbAttributes.MySqlDbType.UInt32

            ElseIf Regex.Match(type_define, "varchar\(\d+\)", RegexOptions.IgnoreCase).Success OrElse Regex.Match(type_define, "char\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.VarChar
                parameter = GetNumberValue(type_define, 45)

            ElseIf Regex.Match(type_define, "double", RegexOptions.IgnoreCase).Success OrElse Strings.InStr(type_define, "float", CompareMethod.Text) > 0 Then
                type = Reflection.DbAttributes.MySqlDbType.Double

            ElseIf Regex.Match(type_define, "datetime", RegexOptions.IgnoreCase).Success OrElse
                Regex.Match(type_define, "date", RegexOptions.IgnoreCase).Success OrElse
                Regex.Match(type_define, "timestamp", RegexOptions.IgnoreCase).Success Then

                type = Reflection.DbAttributes.MySqlDbType.DateTime

            ElseIf type_define.TextEquals("json") OrElse Regex.Match(type_define, "text", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Text

            ElseIf Strings.InStr(type_define, "enum(", CompareMethod.Text) > 0 Then   ' enum类型转换为String类型？？？？
                type = Reflection.DbAttributes.MySqlDbType.String

            ElseIf Strings.InStr(type_define, "Blob", CompareMethod.Text) > 0 OrElse
                Regex.Match(type_define, "varbinary\(\d+\)", RegexOptions.IgnoreCase).Success OrElse
                Regex.Match(type_define, "binary\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Blob

            ElseIf Regex.Match(type_define, "decimal\(", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Decimal

            ElseIf Regex.Match(type_define, "bit\(", RegexICSng).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Bit
                parameter = GetNumberValue(type_define, 1)
            ElseIf type_define = "bigint unsigned" Then
                type = Reflection.DbAttributes.MySqlDbType.UInt64
            ElseIf type_define = "smallint unsigned" Then
                type = Reflection.DbAttributes.MySqlDbType.UInt16
            Else
                ' More complex type is not support yet, but you can
                ' easily extending the mapping code at here

                Throw New NotImplementedException($"Type define is not support yet for    {NameOf(type_define)}   >>> ""{type_define}""")
            End If

            Return New Reflection.DbAttributes.DataType(type, parameter)
        End Function

        Private Function GetNumberValue(typeDef$, Optional default$ = Nothing) As String
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
