#Region "Microsoft.VisualBasic::80d245bce11a2681460ae1eead8d7f05, src\mysqli\LibMySQL\MySqlBuilder\ExpressionSyntax.vb"

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

    '   Total Lines: 86
    '    Code Lines: 60
    ' Comment Lines: 12
    '   Blank Lines: 14
    '     File Size: 2.88 KB


    '     Module ExpressionSyntax
    ' 
    '         Function: (+2 Overloads) [in], [or], between, f, (+2 Overloads) field
    '                   match
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace MySqlBuilder

    Public Module ExpressionSyntax

        ''' <summary>
        ''' Create a new field reference
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function field(name As String) As FieldAssert
            Return New FieldAssert(FieldAssert.EnsureSafeName(name))
        End Function

        Public Function match(ParamArray names As String()) As FieldAssert
            Dim fields As New List(Of String)

            For Each name As String In names
                Call fields.Add(FieldAssert.EnsureSafeName(name))
            Next

            Return New FieldAssert(fields.JoinBy(", ")) With {.op = NameOf(ExpressionSyntax.match)}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function expr(expression As String) As FieldAssert
            Return New FieldAssert("eval") With {
                .op = "expression",
                .val = expression
            }
        End Function

        ''' <summary>
        ''' Create a new table field reference
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function field(table As String, name As String) As FieldAssert
            Return New FieldAssert($"`{table}`.`{name}`")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function f(name As String) As FieldAssert
            Return New FieldAssert(FieldAssert.EnsureSafeName(name))
        End Function

        <Extension>
        Public Function between(f As FieldAssert, min As String, max As String) As FieldAssert
            f.op = NameOf(between)
            f.val = FieldAssert.value(min)
            f.val2 = FieldAssert.value(max)

            Return f
        End Function

        ''' <summary>
        ''' count(field) in mysql
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        <Extension>
        Public Function count(f As FieldAssert) As FieldAssert
            f.op = op_func
            f.val = $"COUNT({f.name})"
            Return f
        End Function

        <Extension>
        Public Function lower(f As FieldAssert) As FieldAssert
            f.op = op_func
            f.val = $"LOWER({f.name})"
            Return f
        End Function

        <Extension>
        Public Function json_contains_path(f As FieldAssert, path As String, Optional one_or_all As String = "one") As FieldAssert
            f.op = op_func
            f.val = $"JSON_CONTAINS_PATH({f.name}, '{one_or_all}', '{path}')"
            Return f
        End Function

        <Extension>
        Public Function [in](Of T)(f As FieldAssert, vals As IEnumerable(Of T), Optional nullFilter As Boolean = False) As FieldAssert
            Dim check_vals As New List(Of String)

            For Each v As T In vals.SafeQuery
                If v Is Nothing Then
                    If nullFilter Then
                        Continue For
                    End If

                    Throw New NullReferenceException("One of the value for make 'IN' index check is nothing!")
                Else
                    Call check_vals.Add(FieldAssert.value(v.ToString))
                End If
            Next

            f.op = NameOf([in])
            f.val = check_vals.JoinBy(", ")
            f.val = $"({f.val})"

            Return f
        End Function

        <Extension>
        Public Function [in](f As FieldAssert, expr As String) As FieldAssert
            f.op = NameOf([in])
            f.val = $"( {expr} )"
            Return f
        End Function

        <Extension>
        Public Function instr(f As FieldAssert, substr As String) As FieldAssert
            f.op = op_func
            f.val = $"INSTR({f.name}, '{substr.Replace("'", "\'")}')"
            Return f
        End Function

        Public Const op_func As String = "func"

        <Extension>
        Public Function char_length(f As FieldAssert) As FieldAssert
            f.op = op_func
            f.val = $"CHAR_LENGTH({f.name})"
            Return f
        End Function

        <Extension>
        Public Function replace(f As FieldAssert, find As String, replaceTo As String) As FieldAssert
            f.op = op_func
            f.val = $"REPLACE({f.name}, {FieldAssert.value(find)}, {FieldAssert.value(replaceTo)})"
            Return f
        End Function

        <Extension>
        Public Function [or](test As FieldAssert()) As String
            Return (From ti As FieldAssert In test Let sql = ti.ToString Select sql).JoinBy(" OR ")
        End Function

        Public Function union(ParamArray parts_sql As String()) As String
            Return $"({parts_sql.JoinBy(") UNION (")})"
        End Function
    End Module
End Namespace
