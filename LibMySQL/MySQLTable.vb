#Region "Microsoft.VisualBasic::4b69750714d1518e8bc7164160243843, src\mysqli\LibMySQL\MySQLTable.vb"

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

    '   Total Lines: 25
    '    Code Lines: 9
    ' Comment Lines: 12
    '   Blank Lines: 4
    '     File Size: 1.38 KB


    ' Class MySQLTable
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' Type Fundamentals of this mysql ORM solution.(这个数据表对象是MYSQL ORM解决方案的类型基础)
''' </summary>
''' 
<XmlType("MySQLi-SQL", [Namespace]:="https://www.mysql.com/")>
Public MustInherit Class MySQLTable : Inherits SchemaMaps.SQLTable

    ''' <summary>
    ''' Generates the mysql specific ``REPLACE INTO`` SQL statement. 
    ''' (如果已经存在了一条相同主键值的记录，则删除它然后在插入更新值；
    ''' 假若不存在，则直接插入新数据，这条命令几乎等价于<see cref="GetInsertSQL"/>命令，所不同的是这个会自动处理旧记录，可能会不安全，
    ''' 因为旧记录可能会在你不知情的情况下被意外的更新了；
    ''' 并且由于需要先判断记录是否存在，执行的速度也比直接的Insert操作要慢一些，大批量数据插入不建议这个操作)
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Overloads Function GetReplaceSQL() As String
    Public MustOverride Function GetDumpInsertValue(AI As Boolean) As String

    Public MustOverride Overloads Function GetReplaceSQL(AI As Boolean) As String
    Public MustOverride Overloads Function GetInsertSQL(AI As Boolean) As String

End Class
