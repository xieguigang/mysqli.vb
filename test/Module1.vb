#Region "Microsoft.VisualBasic::ae460951bc9590b360918c5068657044, G:/graphQL/src/mysqli/test//Module1.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 671 B


    ' Module Module1
    ' 
    '     Sub: connectionStringTest
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.Uri

Module Module1

    Sub connectionStringTest()
        Dim conn As New ConnectionUri With {.Database = "aaaa", .IPAddress = "127.0.0.1", .Password = 123, .Port = 33669, .TimeOut = 60, .User = "me"}

        Call Console.WriteLine(conn.ToString)
        Call Console.WriteLine(conn.GetDisplayUri)

        Dim uri = conn.GenerateUri(Function(s) s)
        Dim conn_str = conn.GetConnectionString

        Dim parse1 As ConnectionUri = uri
        Dim parse2 As ConnectionUri = conn_str

        Call Console.WriteLine(parse1)
        Call Console.WriteLine(parse2)

        Pause()
    End Sub
End Module

