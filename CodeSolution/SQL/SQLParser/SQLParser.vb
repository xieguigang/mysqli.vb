#Region "Microsoft.VisualBasic::48960782e360995cf3c6f912bfa3c6b7, CodeSolution\SQL\SQLParser.vb"

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

' Module SQLParser
' 
'     Function: __createDataType, (+2 Overloads) __createField, __createSchema, __createSchemaInner, __getDBName
'               __getNumberValue, __parseTable, __splitInternal, __sqlParser, (+2 Overloads) LoadSQLDoc
'               LoadSQLDocFromStream, ParseTable
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Public Module SQLParser

        Public Function ParseTable(SQL As String) As Reflection.Schema.Table
            Dim createTableSQL As String = RegexpParser.GetCreateTableSQLText(SQL)
            Dim tokens As TableTokens = TableTokens.ParseTokens(createTableSQL.Replace(vbLf, vbCr))

            Try
                Return __parseTable(SQL, tokens)
            Catch ex As Exception
                Dim dump As New StringBuilder
                Call dump.AppendLine(SQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(NameOf(createTableSQL) & "   ====> ")
                Call dump.AppendLine(createTableSQL)
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine($"TableName:={tokens.Name}")
                Call dump.AppendLine(New String("-"c, 120))
                Call dump.AppendLine(vbCrLf)
                Call dump.AppendLine(String.Join(vbCrLf & "  >  ", tokens.fields))

                Throw New Exception(dump.ToString, ex)
            End Try
        End Function

        Private Function __parseTable(SQL As String, Tokens As TableTokens) As Reflection.Schema.Table
            Dim DB As String = GetDBName(SQL)
            Dim TableName As String = Tokens.name
            Dim PrimaryKey As String = Tokens.name
            Dim FieldsTokens As String() = Tokens.fields
            Dim Table As Table = SetValue(Of Table).InvokeSet(
                __createSchema(FieldsTokens,
                               TableName,
                               PrimaryKey,
                               SQL, comment:=Nothing),
                NameOf(Reflection.Schema.Table.Database),
                DB)
            Return Table
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
            Dim setValue = New SetValue(Of Table)().GetSet(NameOf(Table.Database))
            Dim SqlSchema = LinqAPI.Exec(Of Table) _
                                                   _
            () <= From table
                  In tables
                  Let tbl As Table = __createSchema(
                      table.fields,
                      table.name,
                      table.primaryKey,
                      table.original,
                      comment:=table.comment)
                  Select setValue(tbl, DB)

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
        Private Function __createSchema(token As TableTokens) As Reflection.Schema.Table
            Try
                Return CreateSchemaInner(token)
            Catch ex As Exception

                With New StringBuilder
                    Call .AppendLine(NameOf(createTableSQL))
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(createTableSQL)
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine($"{NameOf(tableName)}   ===>  {tableName}")
                    Call .AppendLine($"{NameOf(primaryKey)}  ===>  {primaryKey}")
                    Call .AppendLine(vbCrLf)
                    Call .AppendLine(NameOf(fields))
                    Call .AppendLine(New String("="c, 120))
                    Call .AppendLine(String.Join(vbCrLf, fields))

                    Throw New Exception(.ToString, ex)
                End With

            End Try
        End Function
    End Module
End Namespace