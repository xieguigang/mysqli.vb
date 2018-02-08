#Region "Microsoft.VisualBasic::665b7a7a8de0d0d2b08315d71c746a15, ..\mysqli\LibMySQL\Reflection\DbAttributes\DbAttributes.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
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
Imports System.Xml.Serialization

Namespace Reflection.DbAttributes

    ''' <summary>
    ''' The field attribute in the database.
    ''' (数据库中的字段的属性)
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class DbAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' Custom attribute class to mapping the field in the data table.
    ''' (用于映射数据库中的表中的某一个字段的自定义属性类型)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property Or AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class DatabaseField : Inherits DbAttribute

        ''' <summary>
        ''' Get or set the name of the database field.
        ''' (获取或者设置数据库表中的字段的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name As String

        Sub New(Optional Name$ = "")
            Me.Name = Name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        ''' <summary>
        ''' Get the field name property.
        ''' (获取字段名)
        ''' </summary>
        ''' <param name="DbField"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Narrowing Operator CType(DbField As DatabaseField) As String
            Return DbField.Name
        End Operator
    End Class

    ''' <summary>
    ''' The name of the mysql table, this attribute can only applied on the Class/structure definition.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct, AllowMultiple:=False, Inherited:=True)>
    Public Class TableName : Inherits DbAttribute

        ''' <summary>
        ''' 数据库的表名
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public ReadOnly Property Name As String
        ''' <summary>
        ''' 这个数据表所处的数据库的名称，可选的属性
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Database As String
        <XmlText>
        Public Property SchemaSQL As String

        ''' <summary>
        ''' 使用表名来初始化这个元数据属性
        ''' </summary>
        ''' <param name="Name"></param>
        Public Sub New(Name$)
            Me.Name = Name
        End Sub

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Database) Then
                Return Name
            Else
                Return $"`{Database}`.`{Name}`"
            End If
        End Function

        ''' <summary>
        ''' Get the table name property.(获取表名称)
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Narrowing Operator CType(obj As TableName) As String
            Return obj.Name
        End Operator

        Public Shared Function GetTableName(Of T As Class)() As TableName
            Dim attrs As Object() = GetType(T).GetCustomAttributes(GetType(TableName), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            Else
                Return DirectCast(attrs(Scan0), TableName)
            End If
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class [Default] : Inherits DbAttribute

        Friend DefaultValue As String

        Public Shared Narrowing Operator CType(e As [Default]) As String
            Return e.DefaultValue
        End Operator
    End Class

    ''' <summary>
    ''' The value of this field is unique in a data table.
    ''' (本字段的值在一张表中唯一)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Unique : Inherits DbAttribute
    End Class

    ''' <summary>
    ''' This field is the primary key of the data table.
    ''' (本字段是本数据表的主键)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class PrimaryKey : Inherits DbAttribute
    End Class

    ''' <summary>
    ''' The value of this field can not be null.
    ''' (本字段的值不能为空)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class NotNULL : Inherits DbAttribute
    End Class

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Binary : Inherits DbAttribute
    End Class

    ''' <summary>
    ''' This filed value can not be a negative number, it just works on the number type.
    ''' (本字段的值不能够是一个负数值，本属性仅适用于数值类型)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Unsigned : Inherits DbAttribute
    End Class

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class ZeroFill : Inherits DbAttribute
    End Class

    ''' <summary>
    ''' When we create new row in the table, this field's value will plus 1 automatically. 
    ''' (本属性指出本字段值将会自动加1当我们在表中新添加一条记录的时候)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class AutoIncrement : Inherits DbAttribute
    End Class
End Namespace
