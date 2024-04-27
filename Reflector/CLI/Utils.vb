#Region "Microsoft.VisualBasic::a54271b510d6042da0a0ca4e9d05f577, G:/graphQL/src/mysqli/Reflector//CLI/Utils.vb"

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

    '   Total Lines: 35
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.35 KB


    ' Module CLI
    ' 
    '     Function: Union
    ' 
    ' /********************************************************************************/

#End Region

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
