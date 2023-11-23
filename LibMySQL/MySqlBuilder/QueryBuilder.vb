Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace MySqlBuilder

    Friend Class QueryBuilder

        Public where As New Dictionary(Of String, List(Of String))
        Public offset As Integer?
        Public page_size As Integer?
        Public left_join As New List(Of NamedCollection(Of FieldAssert))
        Public join_tmp As String

        Sub New(copy As QueryBuilder)
            If Not copy Is Nothing Then
                where = New Dictionary(Of String, List(Of String))(copy.where)
                offset = copy.offset
                page_size = copy.page_size
                left_join = New List(Of NamedCollection(Of FieldAssert))(copy.left_join)
                join_tmp = copy.join_tmp
            End If
        End Sub

        Sub New()
        End Sub

        Public Function PushWhere(type As String, val As String) As QueryBuilder
            If Not where.ContainsKey(type) Then
                where.Add(type, New List(Of String))
            End If

            where(type).Add(val)

            Return Me
        End Function

        Public Function PushWhere(type As String, vals As IEnumerable(Of String)) As QueryBuilder
            If Not where.ContainsKey(type) Then
                where.Add(type, New List(Of String))
            End If

            where(type).AddRange(vals.SafeQuery)

            Return Me
        End Function

        Public Function where_str() As String
            If where.IsNullOrEmpty Then
                Return ""
            Else
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
                        s = $"({where("or").JoinBy(" OR ")})"
                    Else
                        s = $"({s}) OR ({where("or").JoinBy(" OR ")})"
                    End If
                End If

                Return $"WHERE {s}"
            End If
        End Function

        Public Function limit_str() As String
            If offset Is Nothing Then
                Return ""
            ElseIf page_size Is Nothing Then
                Return $"LIMIT {offset}"
            Else
                Return $"LIMIT {offset},{page_size}"
            End If
        End Function

        Public Function left_join_str() As String
            If left_join.IsNullOrEmpty Then
                Return ""
            Else
                Dim str As New List(Of String)

                For Each tbl In left_join
                    str.Add($"LEFT JOIN `{tbl.name}` ON ({tbl.value.JoinBy(" AND ")})")
                Next

                Return str.JoinBy(" ")
            End If
        End Function
    End Class

End Namespace