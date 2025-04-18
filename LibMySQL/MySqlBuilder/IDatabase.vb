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

End Class