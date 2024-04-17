Imports Oracle.LinuxCompatibility.MySQL.Scripting

Namespace MySqlBuilder

    Public Class FieldAssert

        Public Property name As String
        Public Property op As String
        Public Property val As String
        Public Property val2 As String
        Public Property unary_not As Boolean

        Sub New()
        End Sub

        <DebuggerStepThrough>
        Sub New(name As String)
            Me.name = name
        End Sub

        Public Function against(text As String, booleanMode As Boolean) As FieldAssert
            Dim mode As String

            If op <> NameOf(ExpressionSyntax.match) Then
                Throw New InvalidOperationException("operator is not match, this is not a match against full text search expression!")
            End If
            If booleanMode Then
                mode = "boolean"
            Else
                mode = "NATURAL LANGUAGE"
            End If

            val = text
            val2 = mode

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Dim str As String

            If op.TextEquals("between") Then
                str = $"({name} BETWEEN {val} AND {val2})"
            ElseIf op.TextEquals(NameOf(ExpressionSyntax.match)) Then
                str = $"MATCH({name}) against ('{val}' in {val2} mode)"
            ElseIf op.TextEquals("func") Then
                str = val
            Else
                str = $"({name} {op} {val})"
            End If

            If unary_not Then
                str = $"(NOT {str})"
            End If

            Return str
        End Function

        ''' <summary>
        ''' left join xxx on xx = xx
        ''' </summary>
        ''' <param name="x1"></param>
        ''' <param name="x2"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(x1 As FieldAssert, x2 As FieldAssert) As FieldAssert
            x1.op = "="
            x1.val = x2.name
            Return x1
        End Operator

        Public Overloads Shared Operator <>(x1 As FieldAssert, x2 As FieldAssert) As FieldAssert
            x1.op = "<>"
            x1.val = x2.name
            Return x1
        End Operator

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

        Public Overloads Shared Operator Not(field As FieldAssert) As FieldAssert
            field.unary_not = True
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
End Namespace