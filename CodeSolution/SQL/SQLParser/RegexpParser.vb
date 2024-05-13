#Region "Microsoft.VisualBasic::d8e2c183f1046700e651a8b53e482a4a, src\mysqli\CodeSolution\SQL\SQLParser\RegexpParser.vb"

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

    '   Total Lines: 48
    '    Code Lines: 30
    ' Comment Lines: 8
    '   Blank Lines: 10
    '     File Size: 1.63 KB


    '     Module RegexpParser
    ' 
    '         Function: GetCreateTableSQLText, GetDBName, SplitTableCreateInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module RegexpParser

        ''' <summary>
        ''' Parsing the create table statement in the SQL document.
        ''' </summary>
        Const SQL_CREATE_TABLE As String = "CREATE TABLE .+?;(\r|\n)"
        Const DB_NAME As String = "CREATE\s+((DATABASE)|(SCHEMA))\s+IF\s+NOT\s+EXISTS\s+`.+?`"

        Public Function GetCreateTableSQLText(sql As String) As String
            Dim createTable As Match = r.Match(sql, SQL_CREATE_TABLE, RegexOptions.Singleline)
            Dim str As String = createTable.Value

            Return str
        End Function

        <Extension>
        Public Function SplitTableCreateInternal(sql As String) As String()
            Dim mc As MatchCollection = r.Matches(sql, SQL_CREATE_TABLE, RegexOptions.Singleline)
            Dim out As String() = mc.ToArray

            Return out
        End Function

        ''' <summary>
        ''' 获取数据库的名称
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Public Function GetDBName(SQL As String) As String
            Dim name$ = r.Match(SQL, DB_NAME, RegexOptions.IgnoreCase).Value

            If String.IsNullOrEmpty(name) Then
                Return ""
            Else
                name = r.Match(name, "`.+?`").Value
                name = Mid(name, 2, Len(name) - 2)

                Return name
            End If
        End Function
    End Module
End Namespace
