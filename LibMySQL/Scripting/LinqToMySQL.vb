Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser

Namespace Scripting

    Public Module LinqToMySQL

        Public Function BuildSQL(Of T)(linq As Expression(Of Func(Of IEnumerable(Of T)))) As String
            Dim query As MethodCallExpression = linq.Body
            Dim tokens As New List(Of NamedValue(Of String()))
            Dim arg As Expression
            Dim arguments = Function()
                                If query.Arguments.IsNullOrEmpty OrElse query.Arguments.Count = 1 Then
                                    Return {}
                                End If

                                Return query.Arguments _
                                    .Skip(1) _
                                    .Select(Function(a) a.ToString) _
                                    .ToArray
                            End Function

            tokens += New NamedValue(Of String()) With {
                .Name = query.Method.Name,
                .Value = arguments()
            }

            ' convert expression tree to token list
            Do While Not query.Arguments.IsNullOrEmpty
                ' 第一个参数永远是linq表达式的一部分
                arg = query.Arguments.First

                If TypeOf arg Is MethodCallExpression Then
                    query = arg
                ElseIf TypeOf arg Is UnaryExpression Then
                    arg = DirectCast(arg, UnaryExpression).Operand

                    If TypeOf arg Is MethodCallExpression Then
                        query = arg
                    Else
                        Exit Do
                    End If
                Else
                    Throw New NotImplementedException(arg.GetType.FullName)
                End If

                tokens += New NamedValue(Of String()) With {
                    .Name = query.Method.Name,
                    .Value = arguments()
                }
            Loop

            Return tokens.MySQL
        End Function

        <Extension>
        Public Function MySQL(tokens As IEnumerable(Of NamedValue(Of String()))) As String
            Dim expression = tokens.GroupBy(Function(t) t.Name).ToDictionary(Function(g) g.Key, Function(g) g.ToArray)
            Dim parts As New List(Of String)

            ' 在一个linq表达式之中,Select是必定存在的
            Dim rowVar$ = expression!Select.parameterName
            Dim projections = expression!Select.projections

            Dim sql$ = parts.JoinBy(" ")
            Return sql
        End Function

        <Extension>
        Private Function projections(expression As NamedValue(Of String())()) As String()

        End Function

        <Extension>
        Private Function parameterName(expression As NamedValue(Of String())()) As String
            Dim token = expression _
                .Where(Function(t) t.Value.Length = 1) _
                .Select(Function(t) t.Value(Scan0).GetTagValue("=>", trim:=True)) _
                .Where(Function(t) t.Value.IsPattern("Convert[(].+[)]")) _
                .First

            Return token.Name
        End Function
    End Module
End Namespace