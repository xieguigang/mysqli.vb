#Region "Microsoft.VisualBasic::ea42e80bb4f6d7a7cf602113226a48a9, G:/graphQL/src/mysqli/CodeSolution//SQL/SQLParser/SQLParser.vb"

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

    '   Total Lines: 95
    '    Code Lines: 75
    ' Comment Lines: 9
    '   Blank Lines: 11
    '     File Size: 3.88 KB


    '     Module SQLParser
    ' 
    '         Function: CreateSchemaTable, (+2 Overloads) LoadSQLDoc, LoadSQLDocFromStream, ParseTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace SQLParser

    Public Module SQLParser

        Public Function ParseTable(SQL As String) As Reflection.Schema.Table
            Dim createTableSQL As String = RegexpParser.GetCreateTableSQLText(SQL)
            Dim tokens As TableTokens = TableTokens.ParseTokens(createTableSQL.Replace(vbLf, vbCr))

            Try
                Return CreateSchemaTable(tokens)
            Catch ex As Exception
                Dim dump As New StringBuilder
                Call dump.AppendLine(SQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(NameOf(createTableSQL) & "   ====> ")
                Call dump.AppendLine(createTableSQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine($"TableName:={tokens.name}")
                Call dump.AppendLine(New String("-"c, 120))
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(String.Join(vbCrLf & "  >  ", tokens.fields))

                Throw New Exception(dump.ToString, ex)
            End Try
        End Function

        ''' <summary>
        ''' Loading the table schema from a specific SQL doucment.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function LoadSQLDoc(path$) As Table()
            Using file As New StreamReader(New FileStream(path, FileMode.Open))
                Return file.LoadSQLDoc
            End Using
        End Function

        Public Function LoadSQLDocFromStream(sqlDoc As String) As Table()
            Dim DB As String = GetDBName(sqlDoc)
            Dim tables = (From table As String
                          In sqlDoc.SplitTableCreateInternal
                          Let tokens As TableTokens = TableTokens.ParseTokens(sql:=table)
                          Select tokens).ToArray
            Dim SqlSchema = tables _
                .Select(Function(ti)
                            Dim tbl = CreateSchemaTable(ti)
                            tbl.Database = DB
                            Return tbl
                        End Function) _
                .ToArray

            Return SqlSchema
        End Function

        <Extension>
        Public Function LoadSQLDoc(stream As StreamReader, Optional ByRef raw As String = Nothing) As Table()
            With stream.ReadToEnd.Replace("<", "&lt;")
                raw = .ByRef
                Return LoadSQLDocFromStream(.ByRef)
            End With
        End Function

        ''' <summary>
        ''' Create a MySQL table schema object.
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateSchemaTable(token As TableTokens) As Reflection.Schema.Table
            Try
                Return CreateSchemaInner(token)
            Catch ex As Exception
                With New StringBuilder
                    Call .AppendLine("CreateTableSQL")
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(token.original)
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine($"tableName   ===>  {token.name}")
                    Call .AppendLine($"primaryKey  ===>  {token.primaryKey}")
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine("fields")
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(String.Join(vbCrLf, token.fields))

                    Throw New Exception(.ToString, ex)
                End With
            End Try
        End Function
    End Module
End Namespace
