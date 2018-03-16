#Region "Microsoft.VisualBasic::e55e9d3cac6fcd17ab930de322df5802, Reflector\CLI\docs.vb"

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

' Module CLIProgram
' 
'     Function: MySQLMarkdown
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.CodeSolution
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Partial Module CLIProgram

    <ExportAPI("/MySQL.Markdown")>
    <Usage("/MySQL.Markdown /sql <database.sql/std_in> [/toc /out <out.md/std_out>]")>
    <Description("Generates the SDK document of your mysql database.")>
    <Group(Program.DocsTool)>
    <Argument("/sql", False, CLITypes.File, PipelineTypes.std_in,
              Description:="The sql content source from a sql file or sql ``std_out`` output")>
    <Argument("/out", True, CLITypes.File, PipelineTypes.std_out,
              Description:="The markdown document output to a specific file or output onto the ``std_out`` device.")>
    <Argument("/toc", True, CLITypes.Boolean, Description:="Add topics of content?")>
    Public Function MySQLMarkdown(args As CommandLine) As Integer
        With args.OpenStreamOutput("/out", Encodings.UTF8WithoutBOM)
            If Not .BaseStream Is GetType(FileStream) Then
                VBDebugger.ForceSTDError = True
            End If

            Dim sql$ = args.OpenStreamInput("/sql").ReadToEnd
            Dim schema As Table() = SQLParser.LoadSQLDocFromStream(sql)
            Dim markdown$ = schema.Documentation

            Call .WriteLine(markdown)
            Call .Flush()
        End With

        Return 0
    End Function
End Module
