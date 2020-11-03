Imports System.ComponentModel
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Oracle.LinuxCompatibility.MySQL

Partial Module CLI

    <ExportAPI("/union")>
    <Usage("/union /in <directory> [/out <out.sql>]")>
    <Description("Union all of the sql file in the target directory into a one big sql text file.")>
    <Argument("/in", False, CLITypes.File, PipelineTypes.std_in, AcceptTypes:={GetType(MySQLTable)}, Description:="")>
    <Argument("/out", True, CLITypes.File, PipelineTypes.std_out, AcceptTypes:={GetType(MySQLTable)}, Description:="")>
    Public Function Union(args As CommandLine) As Integer
        Dim imports$ = args.ReadInput("/in")

        VBDebugger.ForceSTDError = True

        Using out As StreamWriter = args.OpenStreamOutput("/out")
            For Each file As String In ls - l - r - "*.sql" <= [imports]
                For Each line As String In file.IterateAllLines
                    Call out.WriteLine(line)
                Next

                Call out.WriteLine("-- " & RelativePath([imports], file))

                Call out.WriteLine()
                Call out.WriteLine()
            Next
        End Using

        Return 0
    End Function
End Module