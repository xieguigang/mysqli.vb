Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

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
            Dim table As Type = Nothing

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
                    ElseIf TypeOf arg Is MemberExpression Then
                        Dim from = DirectCast(arg, MemberExpression)

                        table = from.Type.GenericTypeArguments(Scan0)
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

            Return tokens.MySQL(table)
        End Function

        <Extension>
        Public Function MySQL(tokens As IEnumerable(Of NamedValue(Of String())), table As Table) As String
            Dim expression = tokens.GroupBy(Function(t) t.Name).ToDictionary(Function(g) g.Key, Function(g) g.ToArray)
            Dim parts As New List(Of String)
            Dim tableName$ = $"`{table.Database}`.`{table.TableName}`"
            Dim getOption = Function(nameKey As String)
                                Return expression.TryGetValue(nameKey, [default]:={}).SingleOrDefault
                            End Function

            ' 在一个linq表达式之中,Select是必定存在的
            Dim rowVar$ = expression!Select.parameterName
            Dim projections = expression!Select.projections
            Dim condition$ = getOption("Where").whereExpression(rowVar)
            Dim distinct = getOption("Distinct")
            Dim orderBy = rowVar.orderExpression(getOption("OrderBy"), getOption("OrderByDescending"))

            parts += "SELECT"
            parts += projections.projectionExpression(rowVar)

            parts += "FROM"
            parts += tableName

            If Not condition.StringEmpty Then
                parts += "WHERE"
                parts += condition
            End If

            If Not distinct.IsEmpty Then
                parts += "DISTINCT"
            End If

            If Not orderBy.StringEmpty Then
                parts += "ORDER BY"
                parts += orderBy
            End If

            Dim sql$ = parts.JoinBy(" ")
            Return sql
        End Function

        <Extension>
        Private Function orderExpression(rowVar$, asc As NamedValue(Of String()), desc As NamedValue(Of String())) As String
            If Not asc.IsEmpty Then
                Dim key = asc.Value(Scan0)
                Return key.Replace($"{rowVar}.", "") & " ASC"
            ElseIf Not desc.IsEmpty Then
                Dim key = desc.Value(Scan0)
                Return key.Replace($"{rowVar}.", "") & " DESC"
            Else
                Return Nothing
            End If
        End Function

        <Extension>
        Private Function whereExpression(test As NamedValue(Of String()), rowVar$) As String
            If test.IsEmpty Then
                Return ""
            End If

            Dim expression = test.Value(Scan0) _
                .GetTagValue("=>", trim:=True) _
                .Value _
                .Replace($"{rowVar}.", "")

            Return expression
        End Function

        <Extension>
        Private Function projectionExpression(proj As NamedValue(Of String)(), rowVar$) As String
            Dim list As New List(Of String)

            If proj.Length = 1 AndAlso proj(Scan0).Name = "*" Then
                Return "*"
            End If

            For Each field In proj
                list += $"{field.Value.Replace($"{rowVar}.", "")} AS {field.Name}"
            Next

            Return list.JoinBy(", ")
        End Function

        <Extension>
        Private Function projections(expression As NamedValue(Of String())()) As NamedValue(Of String)()
            Dim token = expression _
                .Where(Function(t) t.Value.Length = 1) _
                .Select(Function(t) t.Value(Scan0).GetTagValue("=>", trim:=True)) _
                .Where(Function(t) Not t.Value.IsPattern("Convert[(].+[)]")) _
                .First
            Dim proj$ = token.Value

            If proj.StartsWith("new VB$AnonymousType") Then
                proj = proj.GetStackValue("(", ")")
                Return proj.projectFields.ToArray
            Else
                If token.Name = proj Then
                    Return {
                        New NamedValue(Of String) With {.Name = "*", .Value = Nothing}
                    }
                Else
                    Throw New NotImplementedException
                End If
            End If
        End Function

        ''' <summary>
        ''' Returns [fieldName => value expression]
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        <Extension>
        Private Iterator Function projectFields(expression As String) As IEnumerable(Of NamedValue(Of String))
            Dim chars As CharPtr = expression
            Dim buffer As New List(Of Char)
            Dim c As Char
            Dim funcStack As New Stack(Of Char)
            Dim valueExpression As Boolean = False

            ' 字段之间使用逗号分隔
            Do While Not chars.EndRead
                c = ++chars

                If valueExpression Then

                    If c = "("c Then
                        Call funcStack.Push("("c)
                    ElseIf c = ")"c Then
                        Call funcStack.Pop()
                    End If

                    If c = ","c AndAlso funcStack.Count = 0 Then
                        Yield buffer.CharString.GetTagValue("=", trim:=True)

                        buffer *= 0
                        valueExpression = False

                        Continue Do
                    End If

                ElseIf c = ","c Then
                    Yield buffer.CharString.GetTagValue("=", trim:=True)

                    buffer *= 0
                    valueExpression = False

                    Continue Do
                ElseIf c = "="c Then
                    valueExpression = True
                End If

                buffer += c
            Loop

            If buffer > 0 Then
                Yield buffer.CharString.GetTagValue("=", trim:=True)
            End If
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