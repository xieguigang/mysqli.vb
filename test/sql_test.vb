#Region "Microsoft.VisualBasic::ce106176ec7fc104d39f903a22b119eb, src\mysqli\test\sql_test.vb"

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

    '   Total Lines: 15
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 557 B


    ' Module sql_test
    ' 
    '     Sub: Main2
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder

Public Module sql_test

    Sub Main2()
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.`b.b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a.a`.`b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.`b`"))

        Pause()
    End Sub
End Module
