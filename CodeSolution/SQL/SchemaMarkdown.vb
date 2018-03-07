#Region "Microsoft.VisualBasic::87c85cdc964c9a405db880a2bb4256c0, CodeSolution\SQL\SchemaMarkdown.vb"

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

    ' Module SchemaMarkdown
    ' 
    '     Function: __attrs, Documentation, MakeMarkdown
    ' 
    ' /********************************************************************************/

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
    Private Function __attrs(field As Field, table As Table) As String
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
            If .PrimaryKey OrElse table.PrimaryFields.IndexOf(.FieldName) > -1 Then
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

        Call md.AppendLine("## " & table.TableName) _
               .AppendLine() _
               .AppendLine(CLangStringFormatProvider.ReplaceMetaChars(table.Comment)) _
               .AppendLine() _
               .AppendLine("|field|type|attributes|description|") _
               .AppendLine("|-----|----|----------|-----------|")

        For Each field As Field In table.Fields
            Dim columns = {
                field.FieldName,
                field.DataType.ToString,
                field.__attrs(table),
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

        Return md.ToString
    End Function

    <Extension>
    Public Function MakeHTML(table As Table) As String
        Return New MarkdownHTML().Transform(table.MakeMarkdown)
    End Function

    ''' <summary>
    ''' 从表的定义之中生成开发文档
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="autoTOC"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Documentation(schema As IEnumerable(Of Table), Optional autoTOC As Boolean = False) As String
        Dim md As New StringBuilder

        Call md.AppendLine("# MySQL development docs") _
               .AppendLine() _
               .AppendLine("Mysql database field attributes notes:") _
               .AppendLine() _
               .AppendLine("> **AI**: Auto Increment;") _
               .AppendLine("> **B**:  Binary;") _
               .AppendLine("> **G**:  Generated") _
               .AppendLine("> **NN**: Not Null;") _
               .AppendLine("> **PK**: Primary Key;") _
               .AppendLine("> **UQ**: Unique;") _
               .AppendLine("> **UN**: Unsigned;") _
               .AppendLine("> **ZF**: Zero Fill") _
               .AppendLine() _
               .AppendLine()

        For Each t As Table In schema
            Call md.AppendLine(t.MakeMarkdown)
        Next

        If autoTOC Then
            Return TOC.AddToc(md)
        Else
            Return md.ToString
        End If
    End Function
End Module
