Imports Microsoft.VisualBasic.Linq

Namespace MySqlBuilder.Expression

    Public Class FilterConditions

        Public where As New Dictionary(Of String, List(Of String))

        Sub New()
        End Sub

        ''' <summary>
        ''' make condition filter value copy
        ''' </summary>
        ''' <param name="clone"></param>
        Sub New(clone As FilterConditions)
            If Not clone.IsNullOrEmpty Then
                where = New Dictionary(Of String, List(Of String))(clone.where)
            End If
        End Sub

        Public Sub PushWhere(type As String, val As String)
            If Not where.ContainsKey(type) Then
                Call where.Add(type, New List(Of String))
            End If

            Call where(type).Add(val)
        End Sub

        Public Sub PushWhere(type As String, vals As IEnumerable(Of String))
            If Not where.ContainsKey(type) Then
                Call where.Add(type, New List(Of String))
            End If

            Call where(type).AddRange(vals.SafeQuery)
        End Sub

        Friend Function build_where_str() As String
            Dim s As String = Nothing

            If where.ContainsKey("sql") Then
                s = where("sql").JoinBy(" AND ")
            End If
            If where.ContainsKey("and") Then
                If s.StringEmpty Then
                    s = $"({where("and").JoinBy(" AND ")})"
                Else
                    s = $"({s}) AND ({where("and").JoinBy(" AND ")})"
                End If
            End If
            If where.ContainsKey("or") Then
                If s.StringEmpty Then
                    s = $"({where("or").JoinBy(" AND ")})"
                Else
                    s = $"({s}) OR ({where("or").JoinBy(" AND ")})"
                End If
            End If

            Return s
        End Function
    End Class

End Namespace