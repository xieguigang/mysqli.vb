#Region "Microsoft.VisualBasic::e10c5d9877d130a1900781050d9aab53, ..\mysqli\LibMySQL\MYSQL.Client\Table.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

''' <summary>
''' The mysql table model.(数据查询工具)
''' </summary>
''' <typeparam name="TTable"></typeparam>
Public Class Table(Of TTable As MySQLTable)

    ''' <summary>
    ''' The mysqli interface
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MySQL As MySQL
    Public ReadOnly Property Schema As Table

    ''' <summary>
    ''' Create a new table model from mysqli uri model
    ''' </summary>
    ''' <param name="uri"></param>
    Sub New(uri As ConnectionUri)
        Call Me.New(New MySQL(uri))
    End Sub

    ''' <summary>
    ''' Create a new table model from mysqli interface
    ''' </summary>
    ''' <param name="engine"></param>
    Sub New(engine As MySQL)
        Me.MySQL = engine
        Schema = New Table(GetType(TTable))
    End Sub

    ''' <summary>
    ''' Show this table name
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function ToString() As String
        Return Schema.TableName
    End Function

#Region "Operator"

    Public Shared Narrowing Operator CType(table As Table(Of TTable)) As MySQL
        Return table.MySQL
    End Operator

    Public Shared Widening Operator CType(Engine As MySQL) As Table(Of TTable)
        Return New Table(Of TTable)(Engine)
    End Operator

    Public Shared Operator <=(table As Table(Of TTable), SQL As String) As TTable()
        If String.Equals(SQL.Trim.Split.First, "SELECT", StringComparison.OrdinalIgnoreCase) Then
            Return table.MySQL.Query(Of TTable)(SQL)
        Else
            If table.MySQL.Execute(SQL) > 0 Then
                Return New TTable() {}
            Else
                Return Nothing
            End If
        End If
    End Operator

    Public Shared Operator >=(table As Table(Of TTable), SQL As String) As TTable()
        Return table <= SQL
    End Operator

    ''' <summary>
    ''' 添加新的记录
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="insertRow"></param>
    ''' <returns></returns>
    Public Shared Operator +(table As Table(Of TTable), insertRow As TTable) As Boolean
        Return table.MySQL.ExecInsert(insertRow)
    End Operator

    Public Shared Operator -(table As Table(Of TTable), deleteRow As TTable) As Boolean
        Return table.MySQL.ExecDelete(deleteRow)
    End Operator

    Public Shared Operator ^(table As Table(Of TTable), updateRow As TTable) As Boolean
        Return table.MySQL.ExecUpdate(updateRow)
    End Operator

    ''' <summary>
    ''' 查询单条记录
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    Public Shared Operator <(table As Table(Of TTable), SQL As String) As TTable
        Return table.MySQL.ExecuteScalar(Of TTable)(SQL)
    End Operator

    Public Shared Operator >(table As Table(Of TTable), WHERE As String) As TTable
        Return table.MySQL.ExecuteScalarAuto(Of TTable)(WHERE)
    End Operator
#End Region
End Class

''' <summary>
''' MySQL query helper
''' </summary>
Public Module QueryHelper

    <Extension>
    Public Function SelectAll(Of T As MySQLTable)(table As Table(Of T)) As T()
        Return table <= $"SELECT * FROM `{table.Schema.TableName}`;"
    End Function

    <Extension>
    Public Function SelectALL(Of T As MySQLTable)(arg As WhereArgument(Of T)) As T()
        Dim table = arg.table.Schema
        Dim SQL$ = arg.GetSQL(scalar:=False)
        Return arg.table <= SQL
    End Function

    <Extension>
    Public Function Find(Of T As MySQLTable)(arg As WhereArgument(Of T)) As T
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
    Public Function Where(Of T As MySQLTable)(table As Table(Of T), ParamArray test$()) As WhereArgument(Of T)
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
    Public Function Where(Of T As MySQLTable)(table As Table(Of T), condition$) As WhereArgument(Of T)
        Return New WhereArgument(Of T) With {
            .table = table,
            .condition = condition
        }
    End Function

    <Extension>
    Public Function [And](Of T As MySQLTable)(where As WhereArgument(Of T), ParamArray test$()) As WhereArgument(Of T)
        Return New WhereArgument(Of T) With {
            .table = where.table,
            .condition = where.contact(test, "AND")
        }
    End Function

    <Extension>
    Private Function contact(Of T As MySQLTable)(where As WhereArgument(Of T), test$(), op$) As String
        Return $"( {where.condition & $" {op} " & $"( {test.JoinBy(" AND ")} )"} )"
    End Function

    <Extension>
    Public Function [Or](Of T As MySQLTable)(where As WhereArgument(Of T), ParamArray test$()) As WhereArgument(Of T)
        Return New WhereArgument(Of T) With {
            .table = where.table,
            .condition = where.contact(test, "OR")
        }
    End Function
End Module

Public Structure FieldArgument

    ReadOnly Name$

    Sub New(name$)
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Shared Operator <=(field As FieldArgument, value As Object) As String
        Return $"`{field.Name}` = '{Scripting.ToString(value)}'"
    End Operator

    Public Shared Operator >=(field As FieldArgument, value As Object) As String
        Throw New NotImplementedException
    End Operator

    Public Shared Widening Operator CType(name$) As FieldArgument
        Return New FieldArgument(name)
    End Operator
End Structure

Public Structure WhereArgument(Of T As MySQLTable)

    Dim table As Table(Of T)
    Dim condition$

    Public Function GetSQL(Optional scalar As Boolean = False) As String
        If scalar Then
            Return $"SELECT * FROM `{table.Schema.TableName}` WHERE {condition} LIMIT 1;"
        Else
            Return $"SELECT * FROM `{table.Schema.TableName}` WHERE {condition};"
        End If
    End Function

    Public Overrides Function ToString() As String
        Return condition
    End Function
End Structure
