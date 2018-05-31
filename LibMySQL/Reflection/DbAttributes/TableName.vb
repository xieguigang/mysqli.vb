#Region "Microsoft.VisualBasic::aed97925289d31595dae8048d2e4daa9, LibMySQL\Reflection\DbAttributes\TableName.vb"

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

    '     Class TableName
    ' 
    '         Properties: Database, Name, SchemaSQL
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTableName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization

Namespace Reflection.DbAttributes

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
        ''' <param name="attr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(attr As TableName) As String
            Return attr.Name
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetTableName(Of T As Class)() As TableName
            Return GetTableName(GetType(T))
        End Function

        Public Shared Function GetTableName(schema As Type) As TableName
            Dim attrs() = schema.GetCustomAttributes(GetType(TableName), inherit:=True)

            If attrs.IsNullOrEmpty Then
                Return Nothing
            Else
                Return DirectCast(attrs(Scan0), TableName)
            End If
        End Function
    End Class
End Namespace
