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
        Call function_test()

        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.`b.b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a.a`.`b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.`b`"))

        Pause()
    End Sub

    Sub raw_expr_test()
        Dim f5 = field("e") = "~ 9 <> 1"
        Dim f4 = field("d") = "~ 9 > 1"
        Dim f1 = field("a") = "~ 3+3"
        Dim f2 = field("b") = "~ not TRUE"
        Dim f3 = field("c") = "~{s}-[2-[3-[[(2~{r})-4-[[[(2~{r},3~{s},4~{r},5~{r})-5-(6-Aminopurin-9-Yl)-4-Oxidanyl-3-Phosphonooxy-Oxolan-2-Yl]methoxy-Oxidanyl-Phosphoryl]oxy-Oxidanyl-Phosphoryl]oxy-3,3-Dimethyl-2-Oxidanyl-Butanoyl]amino]propanoylamino]ethyl] (~{e})-Dodec-2-Enethioate"

        Call Console.WriteLine(f5)
        Call Console.WriteLine(f1)
        Call Console.WriteLine(f2)
        Call Console.WriteLine(f3)
        Call Console.WriteLine(f4)

        Pause()
    End Sub

    Sub function_test()
        Dim call_func = field("test").instr("aaa")
        Dim assert = call_func = 1

        Call Console.WriteLine(assert.ToString)

        Pause()
    End Sub
End Module
