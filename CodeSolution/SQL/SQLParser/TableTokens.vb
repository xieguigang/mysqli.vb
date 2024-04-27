#Region "Microsoft.VisualBasic::5803c7a3fae59e44ec8535e4d7e32e9b, G:/graphQL/src/mysqli/CodeSolution//SQL/SQLParser/TableTokens.vb"

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

    '   Total Lines: 83
    '    Code Lines: 57
    ' Comment Lines: 12
    '   Blank Lines: 14
    '     File Size: 2.65 KB


    '     Class TableTokens
    ' 
    '         Properties: comment, fields, name, original, primaryKey
    ' 
    '         Function: ParseTokens, sqlParser, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace SQLParser

    ''' <summary>
    ''' the create table sql text parts
    ''' </summary>
    Public Class TableTokens

        ''' <summary>
        ''' table name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        Public Property primaryKey As String
        Public Property fields As String()
        Public Property comment As String
        Public Property original As String

        Public Overrides Function ToString() As String
            Return original
        End Function

        Public Shared Function ParseTokens(sql As String) As TableTokens
            Dim tokens = sqlParser(sql)
            Dim tableName As String = tokens.Value(Scan0)
            Dim primaryKey As String = tokens.Name
            Dim fieldsTokens = tokens.Value _
                .Skip(1) _
                .ToArray

            Return New TableTokens With {
                .comment = tokens.Description,
                .fields = fieldsTokens,
                .name = tableName,
                .primaryKey = primaryKey,
                .original = sql
            }
        End Function

        ''' <summary>
        ''' Just parse the primary key and the data field list at here
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Private Shared Function sqlParser(SQL As String) As NamedValue(Of String())
            Dim tokens$() = SQL.LineTokens
            Dim p% = tokens.Lookup("PRIMARY KEY")
            Dim primaryKey As String
            Dim table_comment As String = Strings.Trim(tokens.Last)

            If Not table_comment.StartsWith("COMMENT = ") Then
                table_comment = Nothing
            Else
                table_comment = table_comment.GetStackValue("'", "'").Trim
            End If

            If p = -1 Then ' 没有设置主键
                p = tokens.Lookup("UNIQUE KEY")
            End If

            If p = -1 Then
                p = tokens.Lookup("KEY")
            End If

            If p = -1 Then
                primaryKey = ""
            Else
_SET_PRIMARYKEY:
                primaryKey = tokens(p)
                tokens = tokens.Take(p).ToArray
            End If

            p = tokens.Lookup(") ENGINE=")

            If Not p = -1 Then
                tokens = tokens.Take(p).ToArray
            End If

            Return New NamedValue(Of String())(primaryKey, tokens, table_comment)
        End Function
    End Class
End Namespace
