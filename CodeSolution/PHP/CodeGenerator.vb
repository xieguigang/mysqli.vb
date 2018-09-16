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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace PHP

    Public Module CodeGenerator

        ReadOnly phpTypes As New Dictionary(Of MySqlDbType, String) From {
            {MySqlDbType.BigInt, "integer"},
            {MySqlDbType.Double, "double"},
            {MySqlDbType.VarChar, "string"}
        }

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

            Return $"[{fields.JoinBy(", " & vbLf)}]"
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
