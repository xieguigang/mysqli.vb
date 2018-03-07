#Region "Microsoft.VisualBasic::d757da33699287d581d89ac9297c1d7a, LibMySQL\Reflection\DataModels\SqlGenerateMethods.vb"

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

    '     Module SqlGenerateMethods
    ' 
    '         Function: __fields, __getWHERE, GenerateDeleteSql, GenerateInsertSql, (+2 Overloads) GenerateInsertValues
    '                   GenerateUpdateSql
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.SQL

    ''' <summary>
    ''' 请注意，Where语句之中的变量总是<see cref="Schema.Table.Index"></see>属性值中的值
    ''' </summary>
    ''' <remarks></remarks>
    Public Module SqlGenerateMethods

        Const DELETE_SQL As String = "DELETE FROM `{0}` WHERE {1};"

        <Extension>
        Public Function GenerateDeleteSql(Schema As Table) As String
            Return String.Format(DELETE_SQL, Schema.TableName, __getWHERE(Schema.PrimaryFields))
        End Function

        Private Function __getWHERE(index As IEnumerable(Of String), Optional offset% = 0) As String
            If index.Count = 1 Then
                Return $"`{index.First}` = '%s'".Replace("%s", "{%d}").Replace("%d", offset)
            End If

            Dim array$() = index _
                .Select(Function(name, idx) $"`{name}`='{"{"}{idx + offset}{"}"}'") _
                .ToArray
            Return String.Join(" and ", array)
        End Function

        <Extension>
        Public Function GenerateUpdateSql(Schema As Table) As String
            Dim sb As New StringBuilder(512)
            Dim Fields = Schema.Fields.ToArray

            sb.AppendFormat("UPDATE `{0}` SET ", Schema.TableName)

            For i As Integer = 0 To Fields.Length - 1
                sb.AppendFormat("`{0}`='%s', ", Fields(i).FieldName)
                sb.Replace("%s", "{" & i & "}")
            Next
            sb.Remove(sb.Length - 2, 2)
            sb.Append($" WHERE {__getWHERE(Schema.PrimaryFields, offset:=Schema.Fields.Length)};")

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Schema"></param>
        ''' <param name="trimAutoIncrement">
        ''' 假若这个参数值为真的话，则表示假若有列是被标记为自动增长的，则不需要在INSERT_SQL之中在添加他的值了
        ''' </param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function GenerateInsertSql(Schema As Table, Optional trimAutoIncrement As Boolean = False) As String
            Dim sb As New StringBuilder(512)
            Dim fields$() = LinqAPI.Exec(Of String) _
 _
                () <= From field As Field
                      In Schema.__fields(trimAutoIncrement)  ' 因为需要与后面的值一一对应，所以在这里不进行排序不适用并行化
                      Let name = field.FieldName
                      Select $"`{name}`"

            Call sb.AppendFormat("INSERT INTO `{0}` (", Schema.TableName)   ' Create table name header
            Call sb.Append(String.Join(", ", fields))                       ' Fields generate
            Call sb.Append(") VALUES ")                                     ' Values formater generate
            Call sb.Append(fields.GenerateInsertValues() & ";")

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 返回字段的名称列表
        ''' </summary>
        ''' <param name="schema"></param>
        ''' <param name="trimAutoIncrement"></param>
        ''' <returns></returns>
        <Extension>
        Private Function __fields(schema As Table, trimAutoIncrement As Boolean) As Field()
            Dim fields As Field() = schema.Fields

            If trimAutoIncrement Then
                fields = fields _
                    .Where(Function(f) Not f.AutoIncrement) _
                    .ToArray
            End If

            Return fields
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function GenerateInsertValues(schema As Table, Optional stripAI As Boolean = True) As String
            Return schema _
                .__fields(stripAI) _
                .Select(Function(f) $"`{f.FieldName}`") _
                .ToArray _
                .GenerateInsertValues
        End Function

        <Extension> Public Function GenerateInsertValues(fields$()) As String
            Dim values$() = LinqAPI.Exec(Of String) _
 _
                () <= From i As Integer
                      In fields.Sequence
                      Select "'{0}'".Replace("0"c, i)

            Return "(" & String.Join(", ", values) & ")" ' End of the statement
        End Function
    End Module
End Namespace
