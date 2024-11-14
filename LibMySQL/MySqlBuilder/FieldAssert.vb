#Region "Microsoft.VisualBasic::7dd86cecdf16fa95a3ebc9ed779a2dac, src\mysqli\LibMySQL\MySqlBuilder\FieldAssert.vb"

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

'   Total Lines: 269
'    Code Lines: 207
' Comment Lines: 18
'   Blank Lines: 44
'     File Size: 9.31 KB


'     Class FieldAssert
' 
'         Properties: name, op, unary_not, val, val2
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: against, ParseFieldName, ToString, value
'         Operators: (+2 Overloads) <, (+8 Overloads) <>, (+8 Overloads) =, (+2 Overloads) >, (+2 Overloads) Like
'                    (+2 Overloads) Not
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser
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

        Public Function sounds_like(q As String) As FieldAssert
            op = "SOUNDS LIKE"
            val = value(q)

            Return Me
        End Function

        ''' <summary>
        ''' test current field value is null?
        ''' </summary>
        ''' <returns></returns>
        Public Function is_nothing() As FieldAssert
            op = "is"
            val = "null"

            Return Me
        End Function

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

        ''' <summary>
        ''' this function will make sure that the special word example as 
        ''' mysql keyword: select/insert/etc will be wrapped as `name`, 
        ''' so that this will not cause the mysql syntax error.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSafeName() As String
            Return EnsureSafeName(name)
        End Function

        ''' <summary>
        ''' ensure that the given field name is safe when build sql query text
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function EnsureSafeName(name As String) As String
            If name.StringEmpty Then
                Throw New Exception("the given name string is empty, can not be used as reference name!")
            End If

            If name.Last = "`"c Then
                ' `name`
                ' is already safe
                Return name
            ElseIf name.IndexOf("."c) > -1 OrElse
                name.IndexOf(" "c) > -1 OrElse
                name.IndexOf("*"c) > -1 Then

                ' is already safe
                Return name
            Else
                Return $"`{name}`"
            End If
        End Function

        ''' <summary>
        ''' call on the function, current value is the function its first parameter
        ''' </summary>
        ''' <param name="func"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function call_on(func As String, ParamArray args As String()) As FieldAssert
            Dim pars = {ToString()}.JoinIterates(args).ToArray
            Dim calls As String = $"{func}({pars.JoinBy(", ")})"

            Return New FieldAssert() With {
                .op = "func",
                .val = calls
            }
        End Function

        ''' <summary>
        ''' generates the sql expression
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim str As String

            If op.TextEquals("or") AndAlso name.TextEquals("or") Then
                ' a or b
                str = $"(({val}) OR ({val2}))"
            ElseIf op.TextEquals("between") Then
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

        Public Overloads Shared Operator =(field As FieldAssert, any As Object) As FieldAssert
            If any Is Nothing Then
                Return field.is_nothing
            End If

            Select Case any.GetType
                Case GetType(String), GetType(Char) : Return field = CStr(any)
                Case GetType(Integer), GetType(SByte), GetType(Short), GetType(Long) : Return field = CLng(any)
                Case GetType(UInteger), GetType(Byte), GetType(UShort), GetType(ULong) : Return field = CULng(any)
                Case GetType(Single), GetType(Double) : Return field = CDbl(any)
                Case GetType(Date) : Return field = CDate(any)
                Case Else
                    Throw New NotImplementedException(any.GetType.FullName)
            End Select
        End Operator

        Public Overloads Shared Operator =(field As FieldAssert, i As ULong) As FieldAssert
            field.val = i
            field.op = "="
            Return field
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, i As ULong) As FieldAssert
            field.val = i
            field.op = "<>"
            Return field
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator =(field As FieldAssert, i As Long) As FieldAssert
            Return AssignOperator(field, "=", i)
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, i As Long) As FieldAssert
            field.val = i
            field.op = "<>"
            Return field
        End Operator

        Public Overloads Shared Operator =(field As FieldAssert, i As UInteger) As FieldAssert
            field.val = i
            field.op = "="
            Return field
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, i As UInteger) As FieldAssert
            field.val = i
            field.op = "<>"
            Return field
        End Operator

        Public Overloads Shared Operator =(field As FieldAssert, d As Double) As FieldAssert
            field.val = d
            field.op = "="
            Return field
        End Operator

        Public Overloads Shared Operator <>(field As FieldAssert, d As Double) As FieldAssert
            field.val = d
            field.op = "<>"
            Return field
        End Operator

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="op"></param>
        ''' <param name="val_exp">
        ''' the value expression, should be wrapped if it is a string before call this helper function
        ''' </param>
        ''' <returns></returns>
        Private Shared Function AssignOperator(field As FieldAssert, op As String, val_exp As String) As FieldAssert
            If field.op = "func" Then
                ' assert of the function value
                Return New FieldAssert With {
                    .name = field.ToString,
                    .op = op,
                    .val = val_exp
                }
            Else
                field.val = val_exp
                field.op = op
                Return field
            End If
        End Function

        Public Overloads Shared Operator <>(field As FieldAssert, any As Object) As FieldAssert
            Return Not (field = any)
        End Operator

        ''' <summary>
        ''' the date value will be format to string via <see cref="ToMySqlDateTimeString"/> for avoid mysql format error
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="val"></param>
        ''' <returns></returns>
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
            If field.op = "func" Then
                field = New FieldAssert With {.name = field.ToString}
            End If

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
            field.val = value(val, [like]:=True)
            field.op = "LIKE"
            Return field
        End Operator

        Public Overloads Shared Operator Not(field As FieldAssert) As FieldAssert
            field.unary_not = True
            Return field
        End Operator

        Public Overloads Shared Operator Or(a As FieldAssert, b As FieldAssert) As FieldAssert
            Dim assert As New FieldAssert() With {
                .name = "OR",
                .op = "OR",
                .val = a.ToString,
                .val2 = b.ToString
            }

            Return assert
        End Operator

        Public Shared Function ParseFieldName(field As String, Optional strict As Boolean = False) As String
            If field Is Nothing OrElse field = "" Then
                If strict Then
                    Throw New InvalidProgramException("the required field name should not be empty!")
                Else
                    Return ""
                End If
            ElseIf Not field.Contains("."c) Then
                ' no table name prefix
                Return field
            End If

            If Not field.Contains("`"c) Then
                ' a.b
                Return field.Split("."c).Last
            End If

            ' `a`.b
            ' a.`b`
            ' `a`.`b`
            ' a or b may contains the . dot symbol
            Dim escape As Boolean = False
            Dim buf As New CharBuffer
            Dim tokens As New List(Of String)

            For Each c As Char In field
                If escape Then
                    If c = "`"c Then
                        escape = False
                        tokens.Add(New String(buf.PopAllChars))
                    Else
                        buf += c
                    End If
                Else
                    If c = "`"c Then
                        escape = True
                        tokens.Add(New String(buf.PopAllChars))
                    ElseIf c = "."c Then
                        tokens.Add(New String(buf.PopAllChars))
                        tokens.Add(".")
                    Else
                        buf += c
                    End If
                End If
            Next

            If buf > 0 Then
                tokens.Add(New String(buf.PopAllChars))
            End If

            Return tokens.Where(Function(s) s.Length > 0 AndAlso s <> "."c).Last
        End Function

        Friend Shared Function value(val As String, Optional [like] As Boolean = False) As String
            If val.StringEmpty Then
                Return "''"
            ElseIf val.First = "~" Then
                Return val.Substring(1)
            Else
                Return $"'{val.MySqlEscaping([like])}'"
            End If
        End Function

        Public Shared Function MySqlEscaping(val As String) As String
            Return val.MySqlEscaping([like]:=True)
        End Function
    End Class
End Namespace
