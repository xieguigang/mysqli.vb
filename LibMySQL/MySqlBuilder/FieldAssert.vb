Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.Scripting

Namespace MySqlBuilder

    Public Class FieldAssert

        Public Property name As String
        Public Property op As String
        Public Property val As String
        Public Property val2 As String

        Public Overrides Function ToString() As String
            If op.TextEquals("between") Then
                Return $"({name} BETWEEN {val} AND {val2})"
            Else
                Return $"({name} {op} {val})"
            End If
        End Function

        Public Overloads Shared Operator =(field As FieldAssert, val As Date) As FieldAssert
            Return field = val.ToMySqlDateTimeString
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, val As Date) As FieldAssert
            Return field <> val.ToMySqlDateTimeString
        End Operator

        Public Overloads Shared Operator >(field As FieldAssert, val As Date) As FieldAssert
            Return field > val.ToMySqlDateTimeString
        End Operator

        Public Overloads Shared Operator <(field As FieldAssert, val As Date) As FieldAssert
            Return field < val.ToMySqlDateTimeString
        End Operator

        Public Overloads Shared Operator =(field As FieldAssert, val As String) As FieldAssert
            field.val = value(val)
            field.op = "="
            Return field
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, val As String) As FieldAssert
            field.val = value(val)
            field.op = "<>"
            Return field
        End Operator

        Public Overloads Shared Operator >(field As FieldAssert, val As String) As FieldAssert
            field.val = value(val)
            field.op = ">"
            Return field
        End Operator

        Public Overloads Shared Operator <(field As FieldAssert, val As String) As FieldAssert
            field.val = value(val)
            field.op = "<"
            Return field
        End Operator

        ''' <summary>
        ''' field LIKE '%pattern%'
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="val"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator Like(field As FieldAssert, val As String) As FieldAssert
            field.val = value(val)
            field.op = "LIKE"
            Return field
        End Operator

        Friend Shared Function value(val As String) As String
            If val.StringEmpty Then
                Return "''"
            ElseIf val.First = "~" Then
                Return val.Substring(1)
            Else
                Return $"'{val.MySqlEscaping}'"
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
            Return New FieldAssert With {.name = $"`{name}`"}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function f(name As String) As FieldAssert
            Return New FieldAssert With {.name = $"`{name}`"}
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