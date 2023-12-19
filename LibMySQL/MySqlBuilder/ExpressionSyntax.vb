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
    End Module
End Namespace