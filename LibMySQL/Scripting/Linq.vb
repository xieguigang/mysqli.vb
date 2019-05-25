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


Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Scripting

    ''' <summary>
    ''' Linq to MySQL
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Linq(Of T As {New, MySQLTable})
        Implements IEnumerable(Of T)

        Public Overrides Function ToString() As String
            Return New Table(GetType(T)).fullName
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace