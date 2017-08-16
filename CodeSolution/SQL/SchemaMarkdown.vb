#Region "Microsoft.VisualBasic::a2b3cfecccdc41a0b31714b0a1bec96b, ..\mysqli\CodeSolution\SQL\SchemaMarkdown.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Markup.MarkDown
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Public Module SchemaMarkdown

    <Extension>
    Private Function __attrs(field As Field) As String
        Dim a As New List(Of String)

        With field

            If .AutoIncrement Then
                a += "AI"
            End If
            If .Binary Then
                a += "B"
            End If
            If .NotNull Then
                a += "NN"
            End If
            If .PrimaryKey Then
                a += "PK"
            End If
            If .Unique Then
                a += "UQ"
            End If
            If .Unsigned Then
                a += "UN"
            End If
            If .ZeroFill Then
                a += "ZF"
            End If

        End With

        Return a _
            .Select(Function(s) $"``{s}``") _
            .JoinBy(", ")
    End Function

    <Extension>
    Public Function MakeMarkdown(table As Table) As String
        Dim md As New StringBuilder

        Call md.AppendLine("## " & table.TableName)
        Call md.AppendLine(CLangStringFormatProvider.ReplaceMetaChars(table.Comment))
        Call md.AppendLine()
        Call md.AppendLine("|field|type|attributes|description|")
        Call md.AppendLine("|-----|----|----------|-----------|")

        For Each field As Field In table.Fields
            Dim columns = {
                field.FieldName,
                field.DataType.ToString,
                field.__attrs,
                field.Comment _
                    .Replace("\n", "<br />") _
                    .Replace("\t", ASCII.TAB)
            }
            Dim row$ = columns _
                .Select(Function(s) s.Replace("|", "\|")) _
                .JoinBy("|")
            Call md.AppendLine("|" & row & "|")
        Next

        Call md.AppendLine()
        Call md.AppendLine("```SQL")
        Call md.AppendLine(table.SQL)
        Call md.AppendLine("```")

        Call md.AppendLine()
        Call md.AppendLine()

        Return md.ToString
    End Function

    <Extension>
    Public Function Documentation(schema As IEnumerable(Of Table), Optional autoTOC As Boolean = False) As String
        Dim md As New StringBuilder

        Call md.AppendLine("# MySQL development docs")

        Call md.AppendLine("Mysql database field attributes notes:")
        Call md.AppendLine()
        Call md.AppendLine("> AI: Auto Increment; B: Binary; NN: Not Null; PK: Primary Key; UQ: Unique; UN: Unsigned; ZF: Zero Fill")
        Call md.AppendLine()

        For Each t As Table In schema
            Call md.AppendLine(t.MakeMarkdown)
        Next

        Dim markdown$ = TOC.AddToc(md.ToString)
        Return markdown
    End Function
End Module

