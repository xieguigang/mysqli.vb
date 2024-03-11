#Region "Microsoft.VisualBasic::31249747a33b26bc9cab2457ffb5ee2d, LibMySQL\Extensions\DataClient.vb"

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

    ' Module DataClient
    ' 
    '     Function: GroupMerge, HasValue, OccupancyLoad, SelectALL, Truncate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Public Module DataClientExtensions

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
    <Extension>
    Public Function SelectALL(Of T As MySQLTable)(mysql As MySqli) As T()
        Dim table As TableName = GetType(T).GetAttribute(Of TableName)
        Dim SQL$ = $"SELECT * FROM `{table.Database}`.`{table.Name}`;"

        Return mysql.Query(Of T)(SQL)
    End Function

    <Extension>
    Public Function Truncate(Of T As MySQLTable)(mysqli As MySqli) As Boolean
        Dim table As TableName = GetType(T).GetAttribute(Of TableName)
        Dim SQL$

        If table.Database.StringEmpty Then
            SQL = $"TRUNCATE `{table.Name}`;"
        Else
            SQL = $"TRUNCATE `{table.Database}`.`{table.Name}`;"
        End If

        Return mysqli.Execute(SQL) > 0
    End Function

    ''' <summary>
    ''' 这个函数统计出<see cref="MySQLTable"/>所代表的二维表格之中，具有值得属性的数量占所有的属性的百分比
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <param name="o"></param>
    ''' <param name="zeroAsNull"></param>
    ''' <returns></returns>
    <Extension>
    Public Function OccupancyLoad(Of MySqlTable As Class)(
                       schema As BindProperty(Of DatabaseField)(),
                       o As MySqlTable,
                       Optional zeroAsNull As Boolean = False) As Double

        Dim n% = schema _
            .Where(Function(field)
                       Return o.HasValue(field, zeroAsNull)
                   End Function) _
            .Count
        Return n / schema.Length
    End Function

    ReadOnly emptyDate As Date = New Date

    <Extension>
    Public Function HasValue(Of MySqlTable As Class)(entity As MySqlTable, field As BindProperty(Of DatabaseField), Optional zeroAsNULL As Boolean = False) As Boolean
        With field
            Dim value As Object = .GetValue(entity)

            If .Type Is GetType(String) Then
                Return Not DirectCast(value, String).StringEmpty
            ElseIf .Type Is GetType(Boolean) Then
                Return DirectCast(value, Boolean)
            ElseIf .Type Is GetType(Char) Then
                Return Not AscW(DirectCast(value, Char)) = 0
            ElseIf .Type Is GetType(Date) Then
                Return Not DirectCast(value, Date) = emptyDate
            Else
                Dim n = CDbl(value)

                If n <> 0 Then
                    Return True
                Else
                    If zeroAsNULL Then
                        ' 空值，不增加
                        Return False
                    Else
                        Return True
                    End If
                End If
            End If
        End With
    End Function

    ''' <summary>
    ''' 将<paramref name="group"/>之中不为空的属性值填充进入
    ''' <paramref name="obj"/>之中为空值的属性上
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="obj"></param>
    ''' <param name="group"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GroupFills(Of T As Class)(obj As T, schema As BindProperty(Of DatabaseField)(), group As T()) As T
        For Each field As BindProperty(Of DatabaseField) In schema
            If obj.HasValue(field, True) Then
                ' 目标已经在当前的这个属性上面存在值了，则跳过
                Continue For
            End If

            For Each x As T In group
                If x.HasValue(field, True) Then
                    ' 存在值，则进行设置
                    field.SetValue(obj, field.GetValue(x))
                    Exit For
                End If
            Next
        Next

        Return obj
    End Function
End Module
