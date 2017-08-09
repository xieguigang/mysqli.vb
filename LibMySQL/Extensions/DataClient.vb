#Region "Microsoft.VisualBasic::6cb194be519b096347dc199c9c98c579, ..\mysqli\LibMySQL\Extensions\DataClient.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Public Module DataClient

    ''' <summary>
    ''' 从数据库之中加载所有的数据到程序的内存之中，只推荐表的数据量比较小的使用，
    ''' 使用这个函数加载完数据到内存之中后，进行内存之中的查询操作，会很明显提升应用程序的执行性能
    ''' 
    ''' ```SQL
    ''' SELECT * FROM `{table.Database}`.`{table.TableName}`;
    ''' ```
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="mysql"></param>
    ''' <returns></returns>
    <Extension> Public Function SelectALL(Of T As SQLTable)(mysql As MySQL) As T()
        Dim table As TableName = GetType(T).GetAttribute(Of TableName)
        Dim SQL$ = $"SELECT * FROM `{table.Database}`.`{table.Name}`;"
        Return mysql.Query(Of T)(SQL)
    End Function

    <Extension>
    Public Function OccupancyLoad(schema As BindProperty(Of DatabaseField)(),
                                  o As SQLTable,
                                  Optional ZeroAsNull As Boolean = False) As Double
        Dim i As Integer

        For Each field As BindProperty(Of DatabaseField) In schema
            Dim value = field.GetValue(o)

            If field.Type Is GetType(String) Then
                i += If(Not DirectCast(value, String).StringEmpty, 1, 0)
            ElseIf field.Type Is GetType(Boolean) Then
                i += If(DirectCast(value, Boolean), 1, 0)
            ElseIf field.Type Is GetType(Char) Then
                i += If(AscW(DirectCast(value, Char)) = 0, 0, 1)
            ElseIf field.Type Is GetType(Date) Then
                i += If(DirectCast(value, Date) = New Date, 0, 1)
            Else
                Dim n = CDbl(value)

                If n <> 0 Then
                    i += 1
                Else
                    If ZeroAsNull Then
                        ' 空值，不增加
                    Else
                        i += 1
                    End If
                End If
            End If
        Next

        Return i / schema.Length
    End Function
End Module
