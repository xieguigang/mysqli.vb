﻿#Region "Microsoft.VisualBasic::e1c2946724de7ece0787a6d49f441fc8, LibMySQL\Mysqli\Table.vb"

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

' Class Table
' 
'     Properties: MySQL, Schema
' 
'     Function: ToString
' 
'     Sub: (+2 Overloads) New
' 
'     Operators: -, ^, +, <, <=
'                >, >=
' 
' Module QueryHelper
' 
'     Function: [And], [Or], contact, Find, SelectAll
'               SelectALL, (+2 Overloads) Where
' 
' Structure FieldArgument
' 
'     Function: ToString
' 
'     Sub: New
' 
'     Operators: <=, >=
' 
' Structure WhereArgument
' 
'     Function: GetSQL, ToString
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' The mysql table model.(数据查询工具)
''' </summary>
''' <typeparam name="TTable"></typeparam>
Public Class Table(Of TTable As {New, MySQLTable})

    ''' <summary>
    ''' The mysqli interface
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property MySQL As MySqli
    Public ReadOnly Property Schema As Table

    ''' <summary>
    ''' Create a new table model from mysqli uri model
    ''' </summary>
    ''' <param name="uri"></param>
    Sub New(uri As ConnectionUri)
        Call Me.New(New MySqli(uri))
    End Sub

    ''' <summary>
    ''' Create a new table model from mysqli interface
    ''' </summary>
    ''' <param name="engine"></param>
    Sub New(engine As MySqli)
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

    Public Shared Narrowing Operator CType(table As Table(Of TTable)) As MySqli
        Return table.MySQL
    End Operator

    Public Shared Widening Operator CType(Engine As MySqli) As Table(Of TTable)
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