#Region "Microsoft.VisualBasic::efe2f80e93ae6f15cdfef9c75515f85e, G:/graphQL/src/mysqli/LibMySQL//Reflection/SQL/CreateTable.vb"

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

    '   Total Lines: 78
    '    Code Lines: 43
    ' Comment Lines: 22
    '   Blank Lines: 13
    '     File Size: 2.77 KB


    '     Class CreateTableSQL
    ' 
    '         Function: FromSchema
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Scripting

Namespace Reflection.SQL

    ''' <summary>
    ''' Generate the CREATE TABLE sql of the target table schema class object.
    ''' (生成目标数据表模式的"CREATE TABLE" sql语句)
    ''' </summary>
    ''' <remarks>
    ''' Example SQL:
    ''' 
    ''' CREATE  TABLE `Table_Name` (
    '''   `Field1` INT UNSIGNED ZEROFILL NOT NULL DEFAULT 4444 ,
    '''   `Field2` VARCHAR(45) BINARY NOT NULL DEFAULT '534534' ,
    '''   `Field3` INT UNSIGNED ZEROFILL NOT NULL AUTO_INCREMENT ,
    '''  PRIMARY KEY (`Field1`, `Field2`, `Field3`) ,
    '''  UNIQUE INDEX `Field1_UNIQUE` (`Field1` ASC) ,
    '''  UNIQUE INDEX `Field2_UNIQUE` (`Field2` ASC) );
    ''' </remarks>
    Public Class CreateTableSQL

        Friend Const CREATE_TABLE As String = "CREATE  TABLE `{0}` ("
        Friend Const PRIMARY_KEY As String = "PRIMARY KEY ({0})"
        Friend Const UNIQUE_INDEX As String = "UNIQUE INDEX `%s_UNIQUE` (`%s` ASC)"

        ''' <summary>
        ''' Generate the 'CREATE TABLE' sql command.
        ''' (生成'CREATE TABLE' sql命令)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FromSchema(schema As Table) As String
            Dim SQL As New StringBuilder(1024)
            Dim token$

            token = schema _
                .Fields _
                .Select(Function(field)
                            Return "  " & field.ToString & ","
                        End Function) _
                .JoinBy(vbCrLf)

            SQL.AppendFormat(CREATE_TABLE, schema.TableName)
            SQL.AppendLine()
            SQL.AppendLine(token)

            Dim primaryField = schema _
                .PrimaryFields _
                .Select(Function(pk) $"`{pk}`") _
                .JoinBy(", ")

            SQL.AppendFormat(PRIMARY_KEY, primaryField)
            SQL.AppendLine()

            Dim uniqueFields = schema.UniqueFields

            If uniqueFields.Count > 0 Then
                SQL.Append(", ")
                token = uniqueFields _
                    .Select(Function(unique)
                                Return UNIQUE_INDEX.Replace("%s", unique)
                            End Function) _
                    .JoinBy(", ")

                SQL.AppendLine(token)
            End If

            ' End of the create table SQL statement
            SQL.AppendLine(")")
            SQL.AppendLine($"COMMENT = '{schema.Comment.MySqlEscaping}'")
            SQL.AppendLine(";")

            Return SQL.ToString
        End Function
    End Class
End Namespace
