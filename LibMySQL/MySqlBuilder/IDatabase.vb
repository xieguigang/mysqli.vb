#Region "Microsoft.VisualBasic::21ce3eea108f67e196f5392298d51fef, G:/graphQL/src/mysqli/LibMySQL//MySqlBuilder/IDatabase.vb"

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

    '   Total Lines: 37
    '    Code Lines: 15
    ' Comment Lines: 16
    '   Blank Lines: 6
    '     File Size: 1.15 KB


    ' Class IDatabase
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CreateModel, model
    ' 
    ' /********************************************************************************/

#End Region

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
    Protected Function model(Of T As MySQLTable)() As Model
        Return New Model(TableName.GetTableName(Of T), mysqli)
    End Function

    ''' <summary>
    ''' create a model reference to a specific table
    ''' </summary>
    ''' <param name="name">the table name</param>
    ''' <returns></returns>
    Public Function CreateModel(name As String) As Model
        Return New Model(name, mysqli)
    End Function

End Class

