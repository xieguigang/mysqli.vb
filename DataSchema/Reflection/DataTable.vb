#Region "Microsoft.VisualBasic::8b8ff0abb940c3d637b94934658e64aa, LibMySQL\Reflection\DataTable.vb"

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

'     Class DataTable
' 
'         Properties: ErrorMessage, ListData
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: Commit, Create, GetHandle, Query
' 
'         Sub: Delete, (+2 Overloads) Dispose, Fetch, Insert, MySQL_ThrowException
'              Update
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports MySqlConnector
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace Reflection

    ''' <summary>
    ''' A table object of a specific table schema that mapping a table object in the mysql database.
    ''' (一个映射到MYSQL数据库中的指定的表之上的表对象)
    ''' </summary>
    ''' <typeparam name="Schema">
    ''' The table shcema which define on the custom attribut of a class.
    ''' (定义于一个类之中的自定义属性的表结构)
    ''' </typeparam>
    ''' <remarks></remarks>
    Public Class DataTable(Of Schema)

        ''' <summary>
        ''' 'DELETE' sql text generator of a record that type of schema.
        ''' </summary>
        ''' <remarks></remarks>
        Dim DeleteSQL As SQL.Delete(Of Schema)
        ''' <summary>
        ''' 'INSERT' sql text generator of a record that type of schema.
        ''' </summary>
        ''' <remarks></remarks>
        Dim InsertSQL As SQL.Insert(Of Schema)
        ''' <summary>
        ''' 'UPDATE' sql text generator of a record that type of schema.
        ''' </summary>
        ''' <remarks></remarks>
        Dim UpdateSQL As SQL.Update(Of Schema)

        ''' <summary>
        ''' The structure definition information which was parsed from the custom attribut on a class object.
        ''' (从一个类对象上面的自定义属性之中解析出来的表结构信息)
        ''' </summary>
        ''' <remarks></remarks>
        Protected TableSchema As Reflection.Schema.Table

        Friend Sub New()
            TableSchema = GetType(Schema) 'Start reflection and parsing the table structure information.

            DeleteSQL = TableSchema  'Initialize the sql generator using the table structure information that parse from the custom attribut of a class object.
            InsertSQL = TableSchema
            UpdateSQL = TableSchema
        End Sub

        ''' <summary>
        ''' Execute ``CREATE TABLE`` sql.
        ''' </summary>
        ''' <returns></returns>
        Public Function Create() As Boolean
            Dim SQL As String = CreateTableSQL.FromSchema(TableSchema)
#If DEBUG Then
            Console.WriteLine(SQL)
#End If
            Return MySQL.Execute(SQL)
        End Function

        ''' <summary>
        ''' Get a specific record in the dataset by compaired the UNIQUE_INDEX field value.
        ''' (通过值唯一的索引字段来获取一个特定的数据记录)
        ''' </summary>
        ''' <param name="Record"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetHandle(Record As Schema) As Schema
            Dim [String] As String = TableSchema.IndexProperty.GetValue(Record, Nothing).ToString 'Get the Index field value
            Dim LQuery As IEnumerable(Of Schema) = From schema As Schema
                                                   In _listData
                                                   Let str As String = TableSchema.IndexProperty.GetValue(schema, Nothing).ToString
                                                   Where String.Equals([String], str)
                                                   Select schema ' Use LINQ and index value find out the target item 
            Return LQuery.First  'return the item handle
        End Function

        ''' <summary>
        ''' Delete a record in the table. Please notice that, in order to decrease the usage of CPU and networking traffic, the 
        ''' change is not directly affect on the database server, it will be store as a delete sql in the memory and accumulated 
        ''' as a transaction, the change of the database will not happen until you call the commit method to make this change 
        ''' permanently in the database.
        ''' (删除表中的一条记录。请注意：为了减少服务器的计算资源和网络流量的消耗，在使用本模块对数据库作出修改的时候，更改并不会直接提
        ''' 交至数据库之中的，而是将修改作为一条SQL语句存储下来并对这些命令进行积累作为一个事务存在，即数据库不会受到修改的影响直到你将
        ''' 本事务提交至数据库服务器之上)
        ''' </summary>
        ''' <param name="Record"></param>
        ''' <remarks></remarks>
        Public Sub Delete(Record As Schema)
            Dim SQL As String = DeleteSQL.Generate(Record)

            Call _listData.Remove(Record)
            Call Transaction.AppendLine(SQL)
        End Sub

        ''' <summary>
        ''' Insert a record in the table. Please notice that, in order to decrease the usage of CPU and networking traffic, the 
        ''' change is not directly affect on the database server, it will be store as a insert sql in the memory and accumulated 
        ''' as a transaction, the change of the database will not happen until you call the commit method to make this change 
        ''' permanently in the database.
        ''' (向表中插入一条新记录。请注意：为了减少服务器的计算资源和网络流量的消耗，在使用本模块对数据库作出修改的时候，更改并不会直接提
        ''' 交至数据库之中的，而是将修改作为一条SQL语句存储下来并对这些命令进行积累作为一个事务存在，即数据库不会受到修改的影响直到你将
        ''' 本事务提交至数据库服务器之上)
        ''' </summary>
        ''' <param name="Record"></param>
        ''' <remarks></remarks>
        Public Overloads Sub Insert(Record As Schema)
            Dim SQL As String = InsertSQL.Generate(Record)

            Call _listData.Add(Record)
            Call Transaction.AppendLine(SQL)
        End Sub

        ''' <summary>
        ''' Update a record in the table. Please notice that, in order to decrease the usage of CPU and networking traffic, the 
        ''' change is not directly affect on the database server, it will be store as a update sql in the memory and accumulated 
        ''' as a transaction, the change of the database will not happen until you call the commit method to make this change 
        ''' permanently in the database.
        ''' (修改表中的一条记录。请注意：为了减少服务器的计算资源和网络流量的消耗，在使用本模块对数据库作出修改的时候，更改并不会直接提
        ''' 交至数据库之中的，而是将修改作为一条SQL语句存储下来并对这些命令进行积累作为一个事务存在，即数据库不会受到修改的影响直到你将
        ''' 本事务提交至数据库服务器之上)
        ''' </summary>
        ''' <param name="Record"></param>
        ''' <remarks></remarks>
        Public Sub Update(Record As Schema)
            Dim SQL As String = UpdateSQL.Generate(Record)
            Dim OldRecord As Schema = GetHandle(Record)
            Dim Handle As Integer = _listData.IndexOf(OldRecord)

            _listData(Handle) = Record

            Call Transaction.AppendLine(SQL)
        End Sub
    End Class
End Namespace
