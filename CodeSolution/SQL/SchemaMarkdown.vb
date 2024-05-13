#Region "Microsoft.VisualBasic::3f6e7bc4380f182fb11d0445df38094f, src\mysqli\CodeSolution\SQL\SchemaMarkdown.vb"

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

    '   Total Lines: 142
    '    Code Lines: 116
    ' Comment Lines: 6
    '   Blank Lines: 20
    '     File Size: 4.77 KB


    ' Module SchemaMarkdown
    ' 
    '     Function: __attrs, Documentation, MakeHTML, MakeMarkdown
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports MarkdownHTML = Microsoft.VisualBasic.MIME.text.markdown.MarkdownRender

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

        Call md.AppendLine("***") _
               .AppendLine()

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

        If table.Fields.Length >= 10 Then
            Call md.AppendLine(Document.Pagebreak)
            Call md.AppendLine()
        End If

        Call md.AppendLine()
        Call md.AppendLine("#### SQL Declare")
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

        Call md.AppendLine("# MySql Development Docs #") _
               .AppendLine() _
               .AppendLine("MySql database field attributes notes in this development document:") _
               .AppendLine() _
               .AppendLine("> + **AI**: Auto Increment;") _
               .AppendLine("> + **B**:  Binary;") _
               .AppendLine("> + **G**:  Generated") _
               .AppendLine("> + **NN**: Not Null;") _
               .AppendLine("> + **PK**: Primary Key;") _
               .AppendLine("> + **UQ**: Unique;") _
               .AppendLine("> + **UN**: Unsigned;") _
               .AppendLine("> + **ZF**: Zero Fill") _
               .AppendLine() _
               .AppendLine($"Generate time: {Now.ToString}<br />") _
               .AppendLine($"By: ``mysqli.vb`` reflector tool ([https://github.com/xieguigang/mysqli.vb](https://github.com/xieguigang/mysqli.vb))") _
               .AppendLine() _
               .AppendLine(Document.Pagebreak) _
               .AppendLine()

        For Each t As Table In schema
            Call md.AppendLine(t.MakeMarkdown) _
                   .AppendLine(Document.Pagebreak) _
                   .AppendLine()
        Next

        If autoTOC Then
            Return TOC.AddToc(md)
        Else
            Return md.ToString
        End If
    End Function
End Module
