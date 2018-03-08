#Region "Microsoft.VisualBasic::d3f5036455ce23211b03981e15115398, LibMySQL\Mysqli\Expression\QueryHelper.vb"

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

    '     Module QueryHelper
    ' 
    '         Function: [And], [Or], contact, Find, SelectAll
    '                   SelectALL, (+2 Overloads) Where
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Expressions

    ''' <summary>
    ''' MySQL query helper
    ''' </summary>
    Public Module QueryHelper

        <Extension>
        Public Function SelectAll(Of T As {New, MySQLTable})(table As Table(Of T)) As T()
            Return table <= $"SELECT * FROM `{table.Schema.TableName}`;"
        End Function

        <Extension>
        Public Function SelectALL(Of T As {New, MySQLTable})(arg As WhereArgument(Of T)) As T()
            Dim table = arg.table.Schema
            Dim SQL$ = arg.GetSQL(scalar:=False)
            Return arg.table <= SQL
        End Function

        ''' <summary>
        ''' ``SELECT * FROM table WHERE ... LIMIT 1;``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="arg"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Find(Of T As {New, MySQLTable})(arg As WhereArgument(Of T)) As T
            Dim table = arg.table.Schema
            Dim SQL$ = arg.GetSQL(scalar:=True)
            Return arg.table < SQL
        End Function

        <Extension>
        Public Function Update(Of T As {New, MySQLTable})(arg As WhereArgument(Of T), ParamArray setFields$()) As Boolean
            Dim table = arg.table.Schema
            Dim SQL$ = $"UPDATE `{table.TableName}` SET {setFields.JoinBy(", ")} WHERE {arg.condition};"
            Return arg.table.MySQL.Execute(SQL) > 0
        End Function

        ''' <summary>
        ''' 默认是AND关系
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table"></param>
        ''' <param name="test$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Where(Of T As {New, MySQLTable})(table As Table(Of T), ParamArray test$()) As WhereArgument(Of T)
            Return New WhereArgument(Of T) With {
                .table = table,
                .condition = $"( {test.JoinBy(" AND ")} )"
            }
        End Function

        ''' <summary>
        ''' ``WHERE ...``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="table">The mysql table</param>
        ''' <param name="condition$">The where condition.</param>
        ''' <returns></returns>
        <Extension>
        Public Function Where(Of T As {New, MySQLTable})(table As Table(Of T), condition$) As WhereArgument(Of T)
            Return New WhereArgument(Of T) With {
                .table = table,
                .condition = condition
            }
        End Function

        <Extension>
        Public Function [And](Of T As {New, MySQLTable})(where As WhereArgument(Of T), ParamArray test$()) As WhereArgument(Of T)
            Return New WhereArgument(Of T) With {
                .table = where.table,
                .condition = where.contact(test, "AND")
            }
        End Function

        <Extension>
        Private Function contact(Of T As {New, MySQLTable})(where As WhereArgument(Of T), test$(), op$) As String
            Return $"( {where.condition & $" {op} " & $"( {test.JoinBy(" AND ")} )"} )"
        End Function

        <Extension>
        Public Function [Or](Of T As {New, MySQLTable})(where As WhereArgument(Of T), ParamArray test$()) As WhereArgument(Of T)
            Return New WhereArgument(Of T) With {
                .table = where.table,
                .condition = where.contact(test, "OR")
            }
        End Function
    End Module
End Namespace
