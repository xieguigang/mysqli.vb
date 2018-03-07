#Region "Microsoft.VisualBasic::70b73e0ce5c13896efb129feeb123efb, LibMySQL\Mysqli\Linq.vb"

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

    ' Class Linq
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Sub: __init
    ' 
    '     Operators: <=, >=
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.Expressions
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' Linq to MySQL
''' </summary>
''' <typeparam name="TTable"></typeparam>
Public Class Linq(Of TTable As {New, MySQLTable}) : Inherits Table(Of TTable)

    Dim __reflector As Reflection.DbReflector

    Sub New(uri As ConnectionUri)
        Call MyBase.New(uri)
        Call __init()
    End Sub

    Sub New(Engine As MySqli)
        Call MyBase.New(Engine)
        Call __init()
    End Sub

    Sub New(base As Table(Of TTable))
        Call MyBase.New(base.MySQL)
        Call __init()
    End Sub

    Private Sub __init()
        __reflector = New Reflection.DbReflector(MySQL.UriMySQL)
    End Sub

    Public Overloads Shared Operator <=(DBI As Linq(Of TTable), SQL As String) As IEnumerable(Of TTable)
        Dim err As String = ""
        Dim query As IEnumerable(Of TTable) = DBI.__reflector.AsQuery(Of TTable)(SQL, getError:=err)
        Return query
    End Operator

    Public Overloads Shared Operator >=(DBI As Linq(Of TTable), SQL As String) As IEnumerable(Of TTable)
        Return DBI <= SQL
    End Operator
End Class
