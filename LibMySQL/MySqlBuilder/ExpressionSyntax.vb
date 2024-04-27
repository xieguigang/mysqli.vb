#Region "Microsoft.VisualBasic::80d245bce11a2681460ae1eead8d7f05, G:/graphQL/src/mysqli/LibMySQL//MySqlBuilder/ExpressionSyntax.vb"

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
            If name.First <> "`" AndAlso name.Last <> "`" Then
                name = $"`{name}`"
            End If

            Return New FieldAssert(name)
        End Function

        Public Function match(name As String) As FieldAssert
            If name.First <> "`" AndAlso name.Last <> "`" Then
                name = $"`{name}`"
            End If

            Return New FieldAssert(name) With {.op = NameOf(ExpressionSyntax.match)}
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
            If name.First <> "`" AndAlso name.Last <> "`" Then
                name = $"`{name}`"
            End If

            Return New FieldAssert(name)
        End Function

        <Extension>
        Public Function between(f As FieldAssert, min As String, max As String) As FieldAssert
            f.op = NameOf(between)
            f.val = FieldAssert.value(min)
            f.val2 = FieldAssert.value(max)

            Return f
        End Function

        <Extension>
        Public Function [in](Of T)(f As FieldAssert, vals As IEnumerable(Of T)) As FieldAssert
            f.op = NameOf([in])
            f.val = vals _
                .SafeQuery _
                .Select(Function(v) FieldAssert.value(v.ToString)) _
                .JoinBy(", ")
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
        Public Function [or](test As FieldAssert()) As String
            Return (From ti As FieldAssert In test Let sql = ti.ToString Select sql).JoinBy(" OR ")
        End Function
    End Module
End Namespace
