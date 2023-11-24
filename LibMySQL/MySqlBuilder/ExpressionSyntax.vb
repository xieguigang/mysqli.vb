Imports System.Runtime.CompilerServices

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

        Public Function [in](f As FieldAssert, vals As String()) As FieldAssert
            f.op = NameOf([in])
            f.val = vals.Select(Function(v) FieldAssert.value(v)).JoinBy(", ")
            f.val = $"({f.val})"
            Return f
        End Function
    End Module
End Namespace