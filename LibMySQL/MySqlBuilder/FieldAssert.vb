﻿Imports System.Runtime.CompilerServices

Namespace MySqlBuilder

    Public Class FieldAssert

        Public Property name As String
        Public Property val As String

        Public Overrides Function ToString() As String
            Return name & val
        End Function

        Public Overloads Shared Operator =(field As FieldAssert, val As String) As FieldAssert
            field.val = " = " & value(val)
            Return field
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, val As String) As FieldAssert
            field.val = " <> " & value(val)
            Return field
        End Operator

        Public Overloads Shared Operator >(field As FieldAssert, val As String) As FieldAssert
            field.val = " > " & value(val)
            Return field
        End Operator

        Public Overloads Shared Operator <(field As FieldAssert, val As String) As FieldAssert
            field.val = " < " & value(val)
            Return field
        End Operator

        Private Shared Function value(val As String) As String
            If val.StringEmpty Then
                Return "''"
            ElseIf val.First = "~" Then
                Return val.Substring(1)
            Else
                Return $"'{val}'"
            End If
        End Function

    End Class

    Public Module ExpressionSyntax

        ''' <summary>
        ''' Create a new field reference
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function field(name As String) As FieldAssert
            Return New FieldAssert With {.name = name}
        End Function

    End Module
End Namespace