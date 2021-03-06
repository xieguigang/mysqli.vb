﻿#Region "Microsoft.VisualBasic::a5d8aeaa798c98ac433ca116b293331e, LibMySQL\Mysqli\Expression\Table.vb"

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

'     Class Table
' 
'         Properties: MySQL, Schema
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
'         Operators: -, ^, +, <, <=
'                    >, >=
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace Expressions

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
        ''' Create a new table model from <see cref="MySqli"/> connection uri model
        ''' </summary>
        ''' <param name="uri"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(uri As ConnectionUri)
            Call Me.New(New MySqli(uri))
        End Sub

        ''' <summary>
        ''' Create a new table model from <see cref="MySqli"/> interface
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(table As Table(Of TTable)) As MySqli
            Return table.MySQL
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >=(table As Table(Of TTable), SQL$) As TTable()
            Return table <= SQL
        End Operator

        ''' <summary>
        ''' 添加新的记录
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="insertRow"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(table As Table(Of TTable), insertRow As TTable) As Boolean
            Return table.MySQL.ExecInsert(insertRow)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(table As Table(Of TTable), deleteRow As TTable) As Boolean
            Return table.MySQL.ExecDelete(deleteRow)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(table As Table(Of TTable), updateRow As TTable) As Boolean
            Return table.MySQL.ExecUpdate(updateRow)
        End Operator

        ''' <summary>
        ''' 查询单条记录
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(table As Table(Of TTable), SQL$) As TTable
            Return table.MySQL.ExecuteScalar(Of TTable)(SQL)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator >(table As Table(Of TTable), WHERE$) As TTable
            Return table.MySQL.ExecuteScalarAuto(Of TTable)(WHERE)
        End Operator
#End Region
    End Class
End Namespace
