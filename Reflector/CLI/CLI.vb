#Region "Microsoft.VisualBasic::20128f4199b2aee26c92be2d0a4002ae, src\mysqli\Reflector\CLI\CLI.vb"

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

    '   Total Lines: 282
    '    Code Lines: 223
    ' Comment Lines: 24
    '   Blank Lines: 35
    '     File Size: 14.55 KB


    ' Module CLI
    ' 
    '     Function: __EXPORT, ExportDumpDir, ReflectsConvert, schemaCompares
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Oracle.LinuxCompatibility.MySQL.CodeSolution
Imports Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports MySQL2vb = Oracle.LinuxCompatibility.MySQL.CodeSolution.VisualBasic.CodeGenerator

<CLI>
<Package("MySQL.Reflector")>
<Description("Tools for convert the mysql schema dump sql script into VisualBasic classes source code.")>
Module CLI

    Const InputsNotFound As String = "The required input parameter ""/sql"" is not specified!"

    <ExportAPI("--reflects")>
    <Example("--reflects /sql ./test.sql /split /namespace ExampleNamespace")>
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
        Dim language$ = args("/language") Or "visualbasic"

        ' 当文件不存在的时候可能是std_in，则判断是否存在out并且是split状态
        If Not SQL.FileExists Then
            If split AndAlso String.IsNullOrEmpty(out) Then
                Call VBDebugger.Warning(InputsNotFound)
                Return -1
            End If
        End If

        If FileIO.FileSystem.FileExists(SQL) Then
            Dim writer As StreamWriter = Nothing
            Dim reader = args.OpenStreamInput("/sql")

            If Not split Then
                writer = args.OpenStreamOutput("-o")
            End If

            Return __EXPORT(SQL, reader, ns, out, writer, split)
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

            Call codes.Keys _
                .GenerateDbSchema(dbname:=out.BaseName, [namespace]:=ns) _
                .SaveTo($"{out}/db_{out.BaseName}.vb", Encoding.UTF8)

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
    <Description("Scans for the table schema sql files in a directory And converts these sql file as visualbasic source code.")>
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

    <ExportAPI("--compares")>
    <Description("Compare the difference betweens two database schema. This cli tools Is used 
                  for make upgrades of current database schema to the new updates database 
                  schema.")>
    <Usage("--compares /current <schema.sql> /updates <schema.sql> [/output <report.md>]")>
    <Argument("/current", False, CLITypes.File, PipelineTypes.undefined,
              AcceptTypes:={GetType(String)},
              Description:="The file path to the database schema sql file that current used in the product environment.")>
    <Argument("/updates", False, CLITypes.File, PipelineTypes.undefined,
              AcceptTypes:={GetType(String)},
              Description:="The file path to the database schema sql file that modified and will be updates to the current used product environment.")>
    Public Function schemaCompares(args As CommandLine) As Integer
        Dim current As String = args <= "/current"
        Dim updates As String = args <= "/updates"
        Dim output As String = args("/output") Or $"{current.ParentPath}/{current.BaseName}_upgrades_to_{updates.BaseName}.schema_compares.report.md"
        Dim schema_updates As Table() = SQLParser.LoadSQLDoc(updates)
        Dim schema_current As Dictionary(Of String, Table) = SQLParser.LoadSQLDoc(current).ToDictionary(Function(t) t.TableName)
        Dim report As New StringBuilder

        Call report.AppendLine("# Schema update report")
        Call report.AppendLine()
        Call report.AppendLine($"Update report for schema of ``{current.FileName}`` updates to new model ``{updates.FileName}``")

        For Each newModel As Table In schema_updates
            Call report.AppendLine($"### Updates for ``{newModel.TableName}``")
            Call report.AppendLine()

            If schema_current.ContainsKey(newModel.TableName) Then
                Dim current_table As Table = schema_current(newModel.TableName)
                Dim current_fields = current_table.Fields.ToDictionary(Function(f) f.FieldName)
                Dim dbName As String = current_table.Database

                If newModel.Comment <> current_table.Comment Then
                    Call report.AppendLine("Update table description comment:")
                    Call report.AppendLine()
                    Call report.AppendLine("```sql")
                    Call report.AppendLine($"ALTER TABLE `{dbName}`.`{newModel.TableName}` COMMENT = '{newModel.Comment}' ;")
                    Call report.AppendLine("```")
                    Call report.AppendLine()
                End If

                For Each field As Field In newModel.Fields
                    If current_fields.ContainsKey(field.FieldName) Then
                        ' check of the data type is different or not
                        Dim current_field As Field = current_fields(field.FieldName)

                        If field.Comment <> current_field.Comment Then
                            Call report.AppendLine($"Description comment of data field has been updated:")
                            Call report.AppendLine()
                            Call report.AppendLine("```sql")
                            Call report.AppendLine($"ALTER TABLE `{dbName}`.`{newModel.TableName}` CHANGE COLUMN `{field.FieldName}` {field} COMMENT '{field.Comment}' ;")
                            Call report.AppendLine("```")
                        End If

                        If current_field.DataType <> field.DataType Then
                            ' data type has been updated
                            Call report.AppendLine($"Data type of current table field ``{current_field.FieldName}`` has been updated:")
                            Call report.AppendLine()
                            Call report.AppendLine("```sql")
                            Call report.AppendLine($"ALTER TABLE `{dbName}`.`{current_table.TableName}` CHANGE COLUMN `{field.FieldName}` {field} COMMENT '{field.Comment}' ;")
                            Call report.AppendLine("```")
                            Call report.AppendLine()
                        End If

                        If current_field.AutoIncrement <> field.AutoIncrement OrElse
                            current_field.Unique <> field.Unique OrElse
                            current_field.Binary <> field.Binary OrElse
                            current_field.Default <> field.Default OrElse
                            current_field.NotNull <> field.NotNull OrElse
                            current_field.PrimaryKey <> field.PrimaryKey OrElse
                            current_field.Unsigned <> field.Unsigned OrElse
                            current_field.ZeroFill <> field.ZeroFill Then

                            ' field attribute has been changed
                            Call report.AppendLine($"Field data attribute of current table ``{current_field.FieldName}`` has been updated:")
                            Call report.AppendLine()
                            Call report.AppendLine("```sql")
                            Call report.AppendLine($"ALTER TABLE `{dbName}`.`{current_table.TableName}` CHANGE COLUMN `{field.FieldName}` {field} COMMENT '{field.Comment}' ;")
                            Call report.AppendLine("```")
                            Call report.AppendLine()
                        End If
                    Else
                        ' add new data field
                        Call report.AppendLine($"Add a new data field ``{field.FieldName}``:")
                        Call report.AppendLine()
                        Call report.AppendLine("```sql")
                        Call report.AppendLine($"ALTER TABLE `{dbName}`.`{current_table.TableName}` ADD COLUMN {field} COMMENT '{field.Comment}' ;")
                        Call report.AppendLine("```")
                        Call report.AppendLine()
                    End If
                Next

                Dim newModelFields As Index(Of String) = newModel.FieldNames.Indexing

                For Each fieldName As String In current_fields.Keys
                    If Not fieldName Like newModelFields Then
                        ' current field in current used database schema has been delete
                        Call report.AppendLine($"A field(``{fieldName}``) has been deleted in the updates model:")
                        Call report.AppendLine()
                        Call report.AppendLine("```sql")
                        Call report.AppendLine($"ALTER TABLE `{dbName}`.`{current_table.TableName}` DROP COLUMN `{fieldName}`;")
                        Call report.AppendLine("```")
                        Call report.AppendLine()
                    End If
                Next
            Else
                ' add new table
                Call report.AppendLine("Current database schema didn't has this table, a new table will be created:")
                Call report.AppendLine()
                Call report.AppendLine("```sql")
                Call report.AppendLine(newModel.SQL)
                Call report.AppendLine("```")
                Call report.AppendLine()
            End If
        Next

        Return report.ToString.SaveTo(output).CLICode
    End Function
End Module
