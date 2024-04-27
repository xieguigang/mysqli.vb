#Region "Microsoft.VisualBasic::987d2ee96f72e5a2363360f8007d4e33, G:/graphQL/src/mysqli/Reflector//test2.vb"

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

    '   Total Lines: 67
    '    Code Lines: 52
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.06 KB


    ' Module test2
    ' 
    '     Sub: Main
    '     Class T
    ' 
    '         Properties: A, B, C
    ' 
    '         Function: GetDeleteSQL, GetDumpInsertValue, (+2 Overloads) GetInsertSQL, (+2 Overloads) GetReplaceSQL, GetUpdateSQL
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Module test2

    <TableName("Test")>
    Public Class T : Inherits MySQLTable

        <DatabaseField("A")> Public Property A As String
        <DatabaseField("B")> Public Property B As Double
        <DatabaseField("C")> Public Property C As Date

        Public Overrides Function GetReplaceSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetReplaceSQL(AI As Boolean) As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetDumpInsertValue(AI As Boolean) As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetInsertSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetInsertSQL(AI As Boolean) As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetUpdateSQL() As String
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetDeleteSQL() As String
            Throw New NotImplementedException()
        End Function
    End Class

    Sub Main()
        Dim uri$ = "https://localhost:3306/client?user=root&password=&database=test"
        Dim table As New Table(Of T)(uri)
        Dim A As FieldArgument = NameOf(T.A)
        Dim B As FieldArgument = NameOf(T.B)
        Dim C As FieldArgument = NameOf(T.C)

        Dim SQL = table _
            .Where(
                A <= "555",
                C <= #2017-6-9 12:56:36#,
                B <= -99.369) _
            .And(
                A <= "666",
                B <= 10) _
            .Or(
                A <= "999") _
            .GetSQL(scalar:=True)

        Dim all = table.SelectAll

        println(SQL)

        Pause()
    End Sub
End Module
