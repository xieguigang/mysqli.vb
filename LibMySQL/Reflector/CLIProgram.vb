Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq

Module CLIProgram

    Public Function Main() As Integer
        Return GetType(CLIProgram).RunCLI(args:=App.CommandLine)
    End Function


    <ExportAPI("--reflects", Info:="Automatically generates visualbasic source code from the MySQL database schema dump.",
                           Usage:="--reflects /sql <sql_path> [-o <output_path> /namespace <namespace> /split]",
                           Example:="--reflects /sql ./test.sql")>
    <ParameterInfo("/sql", False,
                   Description:="The file path of the MySQL database schema dump file."),
     ParameterInfo("-o", True,
                   Description:="The output file path of the generated visual basic source code file from the SQL dump file ""/sql"""),
     ParameterInfo("/namespace", True,
                   Description:="The namespace value will be insert into the generated source code if this parameter is not null.")>
    Public Function Convert(argvs As Microsoft.VisualBasic.CommandLine.CommandLine) As Integer
        If Not argvs.CheckMissingRequiredParameters("/sql").IsNullOrEmpty Then
            Call Console.WriteLine("The required input parameter ""/sql"" is not specified!")
            Return -1
        End If

        Dim SQL As String = argvs("/sql"), Output As String = argvs("-o")
        Dim [Namespace] As String = argvs("/namespace")

        If FileIO.FileSystem.FileExists(SQL) Then

            If argvs.GetBoolean("/split") Then

                If String.IsNullOrEmpty(Output) Then
                    Output = FileIO.FileSystem.GetParentPath(SQL)
                    Output = $"{Output}/{IO.Path.GetFileNameWithoutExtension(SQL)}/"
                End If

                Call FileIO.FileSystem.CreateDirectory(Output)

                For Each doc In Oracle.LinuxCompatibility.MySQL.CodeGenerator.GenerateCodeSplit(SQL, [Namespace])
                    Call doc.Value.SaveTo($"{Output}/{doc.Key}.vb", System.Text.Encoding.Unicode)
                Next

            Else
                If String.IsNullOrEmpty(Output) Then
                    Output = FileIO.FileSystem.GetParentPath(SQL)
                    Output = $"{Output}/{IO.Path.GetFileNameWithoutExtension(SQL)}.vb"
                End If

                Dim doc As String = Oracle.LinuxCompatibility.MySQL.CodeGenerator.GenerateCode(SQL, [Namespace])  'Convert the SQL file into a visualbasic source code
                Return CInt(doc.SaveTo(Output, System.Text.Encoding.Unicode))                                            'Save the vb source code into a text file

            End If

        Else
            Call Console.WriteLine($"The target schema sql dump file ""{SQL}"" is not exists on your file system!")
            Return -2
        End If

        Return 0
    End Function

    <ExportAPI("--export.dump", Usage:="--export.dump [-o <out_dir> /namespace <namespace> --dir <source_dir>]")>
    Public Function ExportDumpDir(args As CommandLine.CommandLine) As Integer
        Dim Dir As String = args("--dir")
        Dim ns As String = args("/namespace")
        Dim OutDir As String = args("-o")

        If String.IsNullOrEmpty(Dir) Then
            Dir = My.Computer.FileSystem.CurrentDirectory
        End If
        If String.IsNullOrEmpty(OutDir) Then
            OutDir = My.Computer.FileSystem.CurrentDirectory & "/MySQL_Tables/"
        End If

        Call FileIO.FileSystem.CreateDirectory(OutDir)

        Dim SQLs = FileIO.FileSystem.GetFiles(
            Dir,
            FileIO.SearchOption.SearchTopLevelOnly,
            "*.sql").ToArray(Function(file) FileIO.FileSystem.ReadAllText(file))
        Dim LQuery = SQLs.ToArray(Function(sql) Oracle.LinuxCompatibility.MySQL.CodeGenerator.GenerateClass(sql, ns))

        For Each cls In LQuery
            Dim vb As String = $"{OutDir}/{cls.Key}.vb"
            Call cls.Value.SaveTo(vb)
        Next

        Return LQuery.IsNullOrEmpty.CLICode
    End Function
End Module
