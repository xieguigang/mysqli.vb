﻿#Region "Microsoft.VisualBasic::cb6052dbb5fda7969ecea8e453a2ad15, Reflector\CLI\CLIProgram.vb"

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
'     Function: __EXPORT, ExportDumpDir, ReflectsConvert
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.CodeSolution.PHP
Imports MySQL2vb = Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic.CodeGenerator

<CLI>
<Package("MySQL.Reflector")>
<Description("Tools for convert the mysql schema dump sql script into VisualBasic classes source code.")>
Module CLI

    Const InputsNotFound As String = "The required input parameter ""/sql"" is not specified!"

    <ExportAPI("--reflects", Example:="--reflects /sql ./test.sql /split /namespace ExampleNamespace")>
    <Description("Automatically generates visualbasic source code from the MySQL database schema dump.")>
    <Usage("--reflects /sql <sql_path/std_in> [-o <output_path> /namespace <namespace> --language <php/visualbasic, default=visualbasic> /split]")>
    <Argument("/sql", False, CLITypes.File, PipelineTypes.std_in,
              AcceptTypes:={GetType(String)},
              Description:="The file path of the MySQL database schema dump file."),
     Argument("-o", True, CLITypes.File,
              AcceptTypes:={GetType(String)},
              Description:="The output file path of the generated visual basic source code file from the SQL dump file ""/sql"""),
     Argument("/namespace", True,
              AcceptTypes:={GetType(String)},
              Description:="The namespace value will be insert into the generated source code if this parameter is not null.")>
    <Argument("/split", True,
              AcceptTypes:={GetType(Boolean)},
              Description:="Split the source code into sevral files and named by table name?")>
    <Argument("/auto_increment.disable", True,
              AcceptTypes:={GetType(Boolean)},
              Description:="Enable output the auto increment field in the mysql table instead of auto increment in the process of mysql inserts.")>
    <Group(Program.ORM_CLI)>
    Public Function ReflectsConvert(args As CommandLine) As Integer
        Dim split As Boolean = args("/split")
        Dim SQL As String = args("/sql")
        Dim out$ = args("-o")
        Dim ns As String = args("/namespace")
        Dim language$ = args("--language") Or "visualbasic"

        ' 当文件不存在的时候可能是std_in，则判断是否存在out并且是split状态
        If Not SQL.FileExists Then
            If split AndAlso String.IsNullOrEmpty(out) Then
                Call VBDebugger.Warning(InputsNotFound)
                Return -1
            End If
        End If

        If language.TextEquals("php") Then
            Dim mysqlDoc As StreamReader = args.OpenStreamInput("/sql")

            ' 如果选择utf8编码的话，在windows平台上面utf8是默认带有BOM头的
            ' 这个编码会导致php脚本的解析不正常
            ' 在这里使用不带有BOM头信息的utf8编码
            Using output As StreamWriter = args.OpenStreamOutput("-o", Encodings.UTF8WithoutBOM)
                Call output.WriteLine(mysqlDoc.GeneratePhpModelCode(ns))
                Call output.Flush()
            End Using

            Return 0
        End If

        If FileIO.FileSystem.FileExists(SQL) Then
            Dim writer As StreamWriter = Nothing
            If Not split Then
                writer = args.OpenStreamOutput("-o")
            End If
            Return __EXPORT(
                SQL, args.OpenStreamInput("/sql"),
                ns,
                out, writer,
                split)
        Else
            Dim msg As String = $"The target schema sql dump file ""{SQL}"" is not exists on your file system!"
            Call VBDebugger.PrintException(msg)
            Return -2
        End If

        Return 0
    End Function

    ''' <summary>
    ''' Export source code document to output stream
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <param name="file"></param>
    ''' <param name="ns"></param>
    ''' <param name="out"></param>
    ''' <param name="output"></param>
    ''' <param name="split"></param>
    ''' <returns></returns>
    Private Function __EXPORT%(SQL$, file As StreamReader, ns$, out$, output As StreamWriter, split As Boolean)
        If split Then ' 分开文档的输出形式，则不能够使用stream了
            Dim codes As Dictionary(Of String, String) = MySQL2vb.GenerateCodeSplit(file, ns, SQL)

            If String.IsNullOrEmpty(out) Then
                out = FileIO.FileSystem.GetParentPath(SQL)
                out = $"{out}/{IO.Path.GetFileNameWithoutExtension(SQL)}/"
            End If

            Call FileIO.FileSystem.CreateDirectory(out)

            For Each doc As KeyValuePair(Of String, String) In codes
                Call doc.Value.SaveTo($"{out}/{doc.Key}.vb", Encoding.Unicode)
            Next
        Else ' 整个的文档形式
            If output Is Nothing Then
                If String.IsNullOrEmpty(out) Then
                    out = FileIO.FileSystem.GetParentPath(SQL)
                    out = $"{out}/{SQL.BaseName}.vb"
                End If

                ' Convert the SQL file into a visualbasic source code
                Dim doc$ = MySQL2vb.GenerateCode(file, ns, SQL)
                ' Save the vb source code into a text file
                Return doc.SaveTo(out, Encoding.Unicode).CLICode
            Else
                Call output.Write(MySQL2vb.GenerateCode(file, ns, SQL))
                Call output.Flush()
            End If
        End If

        Return 0
    End Function

    ''' <summary>
    ''' Scans for the table schema sql files in a directory and converts these sql file as visualbasic source code
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--export.dump")>
    <Description("Scans for the table schema sql files in a directory and converts these sql file as visualbasic source code.")>
    <Usage("--export.dump [-o <out_dir> /namespace <namespace> --dir <source_dir>]")>
    <Group(Program.ORM_CLI)>
    Public Function ExportDumpDir(args As CommandLine) As Integer
        Dim DIR As String = args("--dir")
        Dim ns As String = args("/namespace")
        Dim outDIR As String = args("-o")

        If String.IsNullOrEmpty(DIR) Then
            DIR = App.CurrentDirectory
        End If
        If String.IsNullOrEmpty(outDIR) Then
            outDIR = App.CurrentDirectory & "/MySQL_Tables/"
        End If

        Call FileIO.FileSystem.CreateDirectory(outDIR)

        Dim SQLs As IEnumerable(Of String) = ls - l - wildcards("*.sql") <= DIR
        Dim LQuery = SQLs _
            .Select(Function(sql)
                        Return MySQL2vb.GenerateClass(sql.ReadAllText, ns)
                    End Function) _
            .ToArray

        For Each cls As NamedValue(Of String) In LQuery
            Dim vb As String = $"{outDIR}/{cls.Name}.vb"
            Call cls.Value.SaveTo(vb)
        Next

        Return LQuery.IsNullOrEmpty.CLICode
    End Function
End Module
