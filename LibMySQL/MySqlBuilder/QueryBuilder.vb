#Region "Microsoft.VisualBasic::af3c407c5db6ead899b88b2b3a030c70, src\mysqli\LibMySQL\MySqlBuilder\QueryBuilder.vb"

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

    '   Total Lines: 136
    '    Code Lines: 111
    ' Comment Lines: 0
    '   Blank Lines: 25
    '     File Size: 4.30 KB


    '     Class QueryBuilder
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: build_where_str, distinct_str, group_by_str, left_join_str, limit_str
    '                   order_by_str, (+2 Overloads) PushWhere, where_str
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace MySqlBuilder

    Friend Class FilterConditions

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

    Friend Class QueryBuilder

        Public where As FilterConditions
        Public offset As Integer?
        Public page_size As Integer?
        Public left_join As New List(Of NamedCollection(Of FieldAssert))
        Public join_tmp As String
        Public distinct As Boolean
        Public order_by As String()
        Public order_desc As Boolean
        Public group_by As String()
        Public having As FilterConditions

        Sub New(copy As QueryBuilder)
            If Not copy Is Nothing Then
                where = New FilterConditions(copy.where)
                offset = copy.offset
                page_size = copy.page_size
                left_join = New List(Of NamedCollection(Of FieldAssert))(copy.left_join)
                join_tmp = copy.join_tmp
                order_desc = copy.order_desc
                order_by = copy.order_by
                distinct = copy.distinct
                group_by = copy.group_by
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

        Public Function group_by_str() As String
            If group_by.IsNullOrEmpty Then
                Return ""
            End If

            Return $"GROUP BY {group_by.JoinBy(", ")}"
        End Function

        Public Function order_by_str() As String
            If order_by.IsNullOrEmpty Then
                Return ""
            End If

            Return $"ORDER BY {order_by.JoinBy(", ")} {If(order_desc, "DESC", "")}"
        End Function

        Public Function distinct_str() As String
            If Not distinct Then
                Return ""
            Else
                Return "DISTINCT"
            End If
        End Function

        Public Function where_str() As String
            If where.IsNullOrEmpty Then
                Return ""
            Else
                Return $"WHERE {where.build_where_str()}"
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
                    str.Add($"LEFT JOIN {FieldAssert.EnsureSafeName(tbl.name)} ON ({tbl.value.JoinBy(" AND ")})")
                Next

                Return str.JoinBy(" ")
            End If
        End Function
    End Class

End Namespace
