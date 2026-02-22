#Region "Microsoft.VisualBasic::31ecf2ee556341c8796b02f017940fb1, src\mysqli\LibMySQL\MySqlBuilder\IDatabase.vb"

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

'   Total Lines: 41
'    Code Lines: 18
' Comment Lines: 16
'   Blank Lines: 7
'     File Size: 1.23 KB


' Class IDatabase
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: CreateModel, getDriver, model
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' an abstract database wrapper for multiple database table models in clr types
''' </summary>
Public MustInherit Class IDatabase

    ''' <summary>
    ''' the wrapper for the mysql query functions
    ''' </summary>
    Protected mysqli As MySqli

    Public Sub New(mysqli As ConnectionUri)
        Me.mysqli = mysqli
    End Sub

    ''' <summary>
    ''' create a new data table model for create mysql query
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Function model(Of T As {New, MySQLTable})() As TableModel(Of T)
        Return New TableModel(Of T)(mysqli)
    End Function

    ''' <summary>
    ''' create a model reference to a specific table
    ''' </summary>
    ''' <param name="name">the table name</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CreateModel(name As String) As Model
        Return New Model(name, mysqli)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getDriver() As MySqli
        Return mysqli
    End Function

End Class

Public Class TableModel(Of T As {New, MySQLTable}) : Inherits Model

    Sub New(mysqli As MySqli)
        Call MyBase.New(TableName.GetTableName(Of T), mysqli)
    End Sub

    Public Function find_object(ParamArray where As FieldAssert()) As T
        Return Me.where(where).find(Of T)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function async() As AsyncModelTable
        Return New AsyncModelTable(Me)
    End Function

End Class

Public Class AsyncModelTable

    Dim table As Model

    Sub New(table As Model)
        Me.table = table
    End Sub

    ''' <summary>
    ''' WHERE
    ''' </summary>
    ''' <param name="q"></param>
    ''' <returns></returns>
    Public Function where(q As String) As AsyncModelTable
        Return New AsyncModelTable(table.where(q))
    End Function

    ''' <summary>
    ''' WHERE
    ''' </summary>
    ''' <returns></returns>
    Public Function where(list As IEnumerable(Of FieldAssert)) As AsyncModelTable
        Return New AsyncModelTable(table.where(list))
    End Function

    ''' <summary>
    ''' WHERE
    ''' </summary>
    ''' <param name="asserts"></param>
    ''' <returns></returns>
    Public Function where(ParamArray asserts As FieldAssert()) As AsyncModelTable
        Return New AsyncModelTable(table.where(asserts))
    End Function

    Public Function group_by(ParamArray fields As String()) As AsyncModelTable
        Return New AsyncModelTable(table.group_by(fields))
    End Function

    Public Function having(ParamArray asserts As FieldAssert()) As AsyncModelTable
        Return New AsyncModelTable(table.having(asserts))
    End Function

    Public Function [and](q As String) As AsyncModelTable
        Return New AsyncModelTable(table.and(q))
    End Function

    Public Function [and](ParamArray asserts As FieldAssert()) As AsyncModelTable
        Return New AsyncModelTable(table.and(asserts))
    End Function

    Public Function [or](q As String) As AsyncModelTable
        Return New AsyncModelTable(table.or(q))
    End Function

    Public Function [or](ParamArray asserts As FieldAssert()) As AsyncModelTable
        Return New AsyncModelTable(table.or(asserts))
    End Function

    Public Function find(Of T As {New, Class})(ParamArray fields As String()) As System.Threading.Tasks.Task(Of T)
        Return Task.Run(Function() table.find(Of T)(fields))
    End Function

    Public Function find(ParamArray fields As String()) As System.Threading.Tasks.Task(Of Dictionary(Of String, Object))
        Return Task.Run(Function() table.find(fields))
    End Function

    Public Function aggregate(Of T)(exp As String) As System.Threading.Tasks.Task(Of T)
        Return Task.Run(Function() table.aggregate(Of T)(exp))
    End Function

    Public Function count() As System.Threading.Tasks.Task(Of Long)
        Return Task.Run(Function() table.count())
    End Function

    Public Function [select](Of T As {New, Class})(ParamArray fields As String()) As System.Threading.Tasks.Task(Of T())
        Return Task.Run(Function() table.select(Of T)(fields))
    End Function

    Public Function [select](ParamArray fields As String()) As System.Threading.Tasks.Task(Of System.Data.DataTableReader)
        Return Task.Run(Function() table.select(fields))
    End Function

    ''' <summary>
    ''' project a field column as vector from the database
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="field">the field name to project as vector</param>
    ''' <returns></returns>
    Public Function project(Of T As IComparable)(field As String) As System.Threading.Tasks.Task(Of T())
        Return Task.Run(Function() table.project(Of T)(field))
    End Function

    ''' <summary>
    ''' UPDATE
    ''' </summary>
    ''' <param name="fields"></param>
    ''' <returns></returns>
    Public Function save(ParamArray fields As FieldAssert()) As System.Threading.Tasks.Task(Of Boolean)
        Return Task.Run(Function() table.save(fields))
    End Function

    ''' <summary>
    ''' INSERT INTO
    ''' </summary>
    ''' <param name="fields"></param>
    ''' <returns></returns>
    Public Function add(ParamArray fields As FieldAssert()) As System.Threading.Tasks.Task(Of Boolean)
        Return Task.Run(Function() table.add(fields))
    End Function

    ''' <summary>
    ''' DELETE FROM
    ''' </summary>
    ''' <returns></returns>
    Public Function delete() As System.Threading.Tasks.Task(Of Boolean)
        Return Task.Run(Function() table.delete())
    End Function

    ''' <summary>
    ''' LIMIT m,n
    ''' </summary>
    ''' <param name="m"></param>
    ''' <param name="n"></param>
    ''' <returns></returns>
    Public Function limit(m As Integer, Optional n As Integer? = Nothing) As AsyncModelTable
        Return New AsyncModelTable(table.limit(m, n))
    End Function

    ''' <summary>
    ''' LEFT JOIN
    ''' </summary>
    ''' <param name="table"></param>
    ''' <returns></returns>
    Public Function left_join(table As String) As AsyncModelTable
        Return New AsyncModelTable(Me.table.left_join(table))
    End Function

    ''' <summary>
    ''' LEFT JOIN ON
    ''' </summary>
    ''' <param name="fields"></param>
    ''' <returns></returns>
    Public Function [on](ParamArray fields As FieldAssert()) As AsyncModelTable
        Return New AsyncModelTable(table.on(fields))
    End Function

    ''' <summary>
    ''' ORDER BY
    ''' </summary>
    ''' <param name="fields"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' fields parameter data could be field name or an expression
    ''' </remarks>
    Public Function order_by(ParamArray fields As String()) As AsyncModelTable
        Return New AsyncModelTable(table.order_by(fields))
    End Function

    ''' <summary>
    ''' ORDER BY DESC
    ''' </summary>
    ''' <param name="fields"></param>
    ''' <param name="desc"></param>
    ''' <returns></returns>
    Public Function order_by(fields As String(), desc As Boolean) As AsyncModelTable
        Return New AsyncModelTable(table.order_by(fields, desc))
    End Function

    ''' <summary>
    ''' ORDER BY DESC
    ''' </summary>
    ''' <param name="field"></param>
    ''' <param name="desc"></param>
    ''' <returns></returns>
    Public Function order_by(field As String, desc As Boolean) As AsyncModelTable
        Return New AsyncModelTable(table.order_by(field, desc))
    End Function

    ''' <summary>
    ''' DISTINCT
    ''' </summary>
    ''' <returns></returns>
    Public Function distinct() As AsyncModelTable
        Return New AsyncModelTable(table.distinct)
    End Function


End Class