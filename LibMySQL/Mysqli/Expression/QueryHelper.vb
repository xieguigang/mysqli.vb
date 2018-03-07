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