#Region "Microsoft.VisualBasic::0e311803c387082ad393f431f936e411, src\mysqli\LibMySQL\Reflection\DbAttributes\DbAttributes.vb"

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

    '   Total Lines: 110
    '    Code Lines: 45
    ' Comment Lines: 49
    '   Blank Lines: 16
    '     File Size: 4.00 KB


    '     Class DbAttribute
    ' 
    ' 
    ' 
    '     Class DatabaseField
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class [Default]
    ' 
    ' 
    ' 
    '     Class Unique
    ' 
    ' 
    ' 
    '     Class PrimaryKey
    ' 
    ' 
    ' 
    '     Class NotNULL
    ' 
    ' 
    ' 
    '     Class Binary
    ' 
    ' 
    ' 
    '     Class Unsigned
    ' 
    ' 
    ' 
    '     Class ZeroFill
    ' 
    ' 
    ' 
    '     Class AutoIncrement
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
