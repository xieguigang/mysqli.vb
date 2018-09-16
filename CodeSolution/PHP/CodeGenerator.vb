#Region "Microsoft.VisualBasic::5272c760ac6f97c6872eca6006de3fd9, CodeSolution\PHP\CodeGenerator.vb"

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

'     Module CodeGenerator
' 
'         Function: GenerateClass, GenerateCode
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace PHP

    ''' <summary>
    ''' Auto code generator for php.NET framework
    ''' 
    ''' > https://github.com/GCModeller-Cloud/php-dotnet/blob/master/Framework/MVC/MySql/schemaDriver.php
    ''' </summary>
    Public Module CodeGenerator

        ReadOnly phpTypes As New Dictionary(Of MySqlDbType, String) From {
            {MySqlDbType.BigInt, "integer"},
            {MySqlDbType.Double, "double"},
            {MySqlDbType.VarChar, "string"}
        }

        <Extension>
        Public Function GenerateCode(mysqlDoc As StreamReader) As String
            Dim tables As Table() = mysqlDoc.LoadSQLDoc
            Dim functions$() = tables _
                .Select(AddressOf SchemaFunction) _
                .ToArray
            Dim loads = tables _
                .Select(Function(table)
                            Return $"MVC\MySql\SchemaInfo::WriteCache(""${table.TableName}"", self::{table.TableName}());"
                        End Function) _
                .ToArray

            Return $"<?php

Imports(""MVC.MySql.schemaDriver"");

/**
 * {tables.First.Database}.mysqli.class
*/
class {tables.First.Database} {{

    public static function LoadCache() {{
        {loads.JoinBy(vbLf & New String(" "c, 8))}
    }}

    {functions.JoinBy(vbLf)}
}}"
        End Function

        <Extension>
        Public Function SchemaDescrib(describ As NamedCollection(Of SchemaDescribe)) As String
            Dim fields$() = describ _
                .Select(Function(field)
                            Dim keyValues As New List(Of String) From {
                                $"""{NameOf(field.Field)}"" => ""{field.Field}""",
                                $"""{NameOf(field.Key)}"" => ""{field.Key}""",
                                $"""{NameOf(field.Null)}"" => ""{field.Null}""",
                                $"""{NameOf(field.Type)}"" => ""{field.Type}""",
                                $"""{NameOf(field.Extra)}"" => ""{field.Extra}""",
                                $"""{NameOf(field.Default)}"" => ""{field.Default}"""
                            }
                            Dim fieldValue$ = $"""{field.Field}"" => [{keyValues.JoinBy(", ")}]"

                            Return fieldValue
                        End Function) _
                .ToArray

            Return $"[
            {fields.JoinBy(", " & vbLf & New String(" "c, 12))}
        ]"
        End Function

        <Extension>
        Public Function SchemaFunction(table As Table) As String
            Dim schema = Reflection.SchemaDescribe.FromTable(table)
            Dim array = schema.SchemaDescrib

            Return $"
    /**
     * MySql table: ``{table.Database}.{table.TableName}``
     *
     * @return array MySql schema table array.
    */
    public static function {table.TableName}() {{
        return {array};
    }}"
        End Function
    End Module
End Namespace
