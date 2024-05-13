#Region "Microsoft.VisualBasic::83c3118b72b58c2802d2fafb97b4cc6d, src\mysqli\LibMySQL\Extensions\Extensions.vb"

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

    '   Total Lines: 139
    '    Code Lines: 95
    ' Comment Lines: 26
    '   Blank Lines: 18
    '     File Size: 4.95 KB


    ' Module Extensions
    ' 
    '     Properties: MySqlDbTypes, Numerics
    ' 
    '     Function: ClearTable, CopySets, GetAttribute, GetCreateTableMetaSQL, GetDbDataType
    '               OrdinalSchema
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data.Common
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Scripting
Imports Oracle.LinuxCompatibility.MySQL.Uri

Public Module Extensions

    ''' <summary>
    ''' 读取CreateTable元数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Public Function GetCreateTableMetaSQL(Of T As MySQLTable)() As String
        Dim attrs As TableName = GetType(T).GetCustomAttribute(Of TableName)

        If attrs Is Nothing Then
            Return Nothing
        Else
            Return attrs.SchemaSQL
        End If
    End Function

    ''' <summary>
    ''' 如果成功，则返回空值，如果不成功，会返回错误消息
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="mysql"></param>
    ''' <returns></returns>
    <Extension>
    Public Function ClearTable(Of T As MySQLTable)(mysql As MySqli) As String
        Dim SQL As New Value(Of String)

        If mysql.Ping = -1.0R Then
            Return "No MySQL connection!"
        End If

        If Not String.IsNullOrEmpty(SQL = GetCreateTableMetaSQL(Of T)()) Then
            Call mysql.Execute(DropTableSQL(Of T))
            Call mysql.Execute(SQL)
        Else
            Return "No ``CREATE TABLE`` SQL meta data!"
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Get the specific type of custom attribute from a property.
    ''' (从一个属性对象中获取特定的自定义属性对象)
    ''' </summary>
    ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
    ''' <param name="Property">Target property object.(目标属性对象)</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetAttribute(Of T As Attribute)([Property] As PropertyInfo) As T
        Dim attrs As Object() = [Property].GetCustomAttributes(GetType(T), True)

        If Not attrs Is Nothing AndAlso attrs.Length = 1 Then
            Dim CustomAttr As T = CType(attrs(0), T)

            If Not CustomAttr Is Nothing Then
                Return CustomAttr
            End If
        End If
        Return Nothing
    End Function

    Public ReadOnly Property MySqlDbTypes As New Dictionary(Of Type, MySqlDbType) From {
 _
            {GetType(String), MySqlDbType.Text},
            {GetType(Integer), MySqlDbType.MediumInt},
            {GetType(Long), MySqlDbType.BigInt},
            {GetType(Double), MySqlDbType.Double},
            {GetType(Decimal), MySqlDbType.Decimal},
            {GetType(Date), MySqlDbType.Date},
            {GetType(Byte), MySqlDbType.Byte},
            {GetType([Enum]), MySqlDbType.Enum}
    }

    <Extension>
    Public Function OrdinalSchema(reader As DbDataReader) As Dictionary(Of String, Integer)
        Dim schema As Dictionary(Of String, Integer) = reader.FieldCount _
            .SeqIterator _
            .ToDictionary(Function(i) reader.GetName(i),
                          Function(x) x)
        Return schema
    End Function

    ''' <summary>
    ''' Get the data type of a field in the data table.
    ''' (获取数据表之中的某一个域的数据类型)
    ''' </summary>
    ''' <param name="Type"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetDbDataType(Type As Type) As MySqlDbType
        If MySqlDbTypes.ContainsKey(Type) Then
            Return MySqlDbTypes(Type)
        Else
            Return MySqlDbType.Text
        End If
    End Function

    Public ReadOnly Property Numerics As New List(Of MySqlDbType) From {
 _
        MySqlDbType.BigInt, MySqlDbType.Bit, MySqlDbType.Byte,
        MySqlDbType.Decimal, MySqlDbType.Double,
        MySqlDbType.Enum,
        MySqlDbType.Float,
        MySqlDbType.Int16, MySqlDbType.Int24, MySqlDbType.Int32, MySqlDbType.Int64,
        MySqlDbType.MediumInt,
        MySqlDbType.TinyInt,
        MySqlDbType.UByte, MySqlDbType.UInt16, MySqlDbType.UInt24, MySqlDbType.UInt32, MySqlDbType.UInt64,
        MySqlDbType.Year
    }

    <Extension>
    Public Function CopySets(Of row As MySQLTable, T)(o As row, list As IEnumerable(Of T), setValue As Action(Of row, T)) As row()
        Dim array As T() = list.SafeQuery.ToArray

        If array.Length = 0 Then
            Return {o}
        Else
            Dim out As New List(Of row)

            For Each x As T In list
                Dim copy As row = DirectCast(o.Copy, row)
                Call setValue(copy, x)
                Call out.Add(copy)
            Next

            Return out
        End If
    End Function
End Module
