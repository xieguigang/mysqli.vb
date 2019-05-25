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

Imports System.Dynamic
Imports Oracle.LinuxCompatibility.MySQL.Expressions
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace Scripting

    Public Class SQL(Of T As {New, MySQLTable}) : Inherits DynamicObject

        Public ReadOnly Property SQL As String
            Get
                Return buildSQL()
            End Get
        End Property

        Private Function buildSQL() As String

        End Function

        Public Shared Narrowing Operator CType(sql As SQL(Of T)) As T
            Dim wrapper As New T
            wrapper.scripting = sql
            Return wrapper
        End Operator
    End Class

    ''' <summary>
    ''' Linq to MySQL
    ''' </summary>
    ''' <typeparam name="TTable"></typeparam>
    Public Class Linq(Of TTable As {New, MySQLTable})
        Implements IEnumerable(Of SQL(Of TTable))

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield New SQL(Of TTable)
        End Function

        Public Function GetEnumerator() As IEnumerator(Of SQL(Of TTable)) Implements IEnumerable(Of SQL(Of TTable)).GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace