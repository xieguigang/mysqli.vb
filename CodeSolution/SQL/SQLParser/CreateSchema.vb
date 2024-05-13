#Region "Microsoft.VisualBasic::826ea4bf31121d6d583026aac8fed8e9, src\mysqli\CodeSolution\SQL\SQLParser\CreateSchema.vb"

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

    '   Total Lines: 59
    '    Code Lines: 43
    ' Comment Lines: 7
    '   Blank Lines: 9
    '     File Size: 2.29 KB


    '     Module CreateSchema
    ' 
    '         Function: CreateSchemaInner, ParseTableName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateSchema

        Private Function ParseTableName(tableName As String) As String
            Dim tokens As String() = r.Matches(tableName, "`.+?`").ToArray
            tableName = tokens.Last
            tableName = Mid(tableName, 2, Len(tableName) - 2)
            Return tableName
        End Function

        ''' <summary>
        ''' Create a MySQL table schema object.
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSchemaInner(tokens As TableTokens) As Table
            Dim primaryKeys$()
            Dim tableName = ParseTableName(tableName:=tokens.name)
            Dim primaryKey = r.Match(tokens.primaryKey, "\(`.+?`\)").Value

            If Not String.IsNullOrEmpty(primaryKey) Then
                primaryKey = r.Replace(primaryKey, "\(\d+\)", "")
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKeys = Strings.Split(primaryKey, "`,`")
            Else
                primaryKeys = New String() {}
            End If

            Dim comment As String = tokens.comment
            Dim fieldList = tokens.fields _
                .Select(Function(si) si.CreateField) _
                .ToDictionary(Function(field)
                                  Return field.FieldName
                              End Function)

            tokens.original = ASCII.ReplaceQuot(tokens.original, "\'")

            ' The database fields reflection result {Name, Attribute}
            ' Assuming at least only one primary key in a table
            Dim tableSchema As New Table(fieldList) With {
                .TableName = tableName,
                .PrimaryFields = primaryKeys.AsList,
                .Index = primaryKey,
                .Comment = comment,
                .SQL = tokens.original
            }

            Return tableSchema
        End Function
    End Module
End Namespace
