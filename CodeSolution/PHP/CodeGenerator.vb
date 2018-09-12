#Region "Microsoft.VisualBasic::5272c760ac6f97c6872eca6006de3fd9, CodeSolution\PHP\CodeGenerator.vb"

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
'         Function: GenerateClass, GenerateCode
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace PHP

    Public Module CodeGenerator

        ReadOnly phpTypes As New Dictionary(Of MySqlDbType, String) From {
            {MySqlDbType.BigInt, "integer"},
            {MySqlDbType.Double, "double"},
            {MySqlDbType.VarChar, "string"}
        }

        ''' <summary>
        ''' 生成Class代码
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="[Namesapce]"></param>
        ''' <returns></returns>
        Public Function GenerateClass(SQL$, namesapce$) As NamedValue(Of String)
            Dim table As Table = SQLParser.ParseTable(SQL)
            Dim php As String = CodeGenerator.GenerateCode(table, namesapce)

            Return New NamedValue(Of String) With {
                .Name = table.TableName,
                .Value = php
            }
        End Function

        <Extension>
        Private Function GenerateCode(table As Table, namespace$) As String
            Dim php As New StringBuilder
            Dim type$

            Call php.AppendLine("class " & table.TableName & " extends SQLTable {")

            For Each field As Field In table.Fields
                type = phpTypes(field.DataType.MySQLType)

                Call php.AppendLine($"/**")

                For Each line As String In field.Comment.LineTokens
                    Call php.AppendLine($" * {line}")
                Next

                Call php.AppendLine($" *")
                Call php.AppendLine($" * @var {type}")
                Call php.AppendLine($"*/")
                Call php.AppendLine($"public ${field.FieldName};")
            Next

            Call php.AppendLine("}")

            Return php.ToString
        End Function
    End Module
End Namespace
