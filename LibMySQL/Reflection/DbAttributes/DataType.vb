#Region "Microsoft.VisualBasic::e7ab697936d869960da0764801eb7e31, LibMySQL\Reflection\DbAttributes\DataType.vb"

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

    '     Class DataType
    ' 
    '         Properties: MySQLType, ParameterValue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToMySqlDateTimeString, ToString, TypeCasting
    ' 
    '     Module CTypeDynamicsExtensions
    ' 
    '         Function: TypeHandler, UInt32_2_UInteger
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default

Namespace Reflection.DbAttributes

    ''' <summary>
    ''' Please notice that some data type in mysql database is not allow combine with some specific field 
    ''' attribute, and I can't find out this potential error in this code. So, when your schema definition can't 
    ''' create a table then you must check this combination is correct or not in the mysql.
    ''' (请注意：在MySql数据库中有一些数据类型是不能够和一些字段的属性组合使用的，我不能够在本代码中检查出此潜在
    ''' 的错误。故，当你定义的对象类型无法创建表的时候，请检查你的字段属性的组合是否有错误？)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class DataType : Inherits DbAttribute

        Dim _type As MySqlDbType
        Dim _argvs As String
        Dim typeCaster As Func(Of Object, Object)

        Public ReadOnly Property MySQLType As MySqlDbType
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _type
            End Get
        End Property

        Public ReadOnly Property ParameterValue As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _argvs
            End Get
        End Property

        Sub New(type As MySqlDbType, Optional argvs$ = "")
            Me._type = type
            Me._argvs = argvs
            Me.typeCaster = type.TypeHandler
        End Sub

        ReadOnly defaultEmpty As DefaultValue(Of String) = ""

        ''' <summary>
        ''' 显示mysql数据库之中的数据类型的定义字符串
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return _type.ToString & ($" ({_argvs})" Or defaultEmpty)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(dataType As DataType) As MySqlDbType
            Return dataType._type
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(Type As MySqlDbType) As DataType
            Return New DataType(Type)
        End Operator

        ''' <summary>
        ''' 可能由于操作系统的语言或者文化的差异，直接使用<see cref="DateTime"></see>的ToString方法所得到的字符串可能会在一些环境配置之下
        ''' 无法正确的插入MySQL数据库之中，所以需要使用本方法在将对象实例进行转换为SQL语句的时候进行转换为字符串
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ToMySqlDateTimeString(value As DateTime) As String
            With value
                Return String.Format("{0}-{1}-{2} {3}:{4}:{5}", .Year, .Month, .Day, .Hour, .Minute, .Second)
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TypeCasting(value As Object) As Object
            Return typeCaster(value)
        End Function
    End Class

    Public Module CTypeDynamicsExtensions

        ReadOnly _typeCasting As New Dictionary(Of MySqlDbType, Func(Of Object, Object)) From {
 _
            {MySqlDbType.UInt32, AddressOf UInt32_2_UInteger},
            {MySqlDbType.Int32, Function(value As Object) If(IsDBNull(value), Nothing, value)},
            {MySqlDbType.Text, Function(value As Object) If(IsDBNull(value), "", CStr(value))},
            {MySqlDbType.String, Function(value As Object) If(IsDBNull(value), "", CStr(value))},
            {MySqlDbType.VarChar, Function(value As Object) If(IsDBNull(value), "", CStr(value))},
            {MySqlDbType.Byte, Function(value As Object) value},
            {MySqlDbType.Bit, Function(value As Object) value},
            {MySqlDbType.LongBlob, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Byte()))},
            {MySqlDbType.Blob, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Byte()))},
            {MySqlDbType.MediumBlob, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Byte()))},
            {MySqlDbType.TinyBlob, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Byte()))},
            {MySqlDbType.Double, Function(value As Object) If(IsDBNull(value), Nothing, value)},
            {MySqlDbType.LongText, Function(value As Object) If(IsDBNull(value), "", value)},
            {MySqlDbType.Int64, Function(value As Object) If(IsDBNull(value), Nothing, CLng(value))},
            {MySqlDbType.Decimal, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Decimal))},
            {MySqlDbType.DateTime, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Date))},
            {MySqlDbType.Date, Function(value As Object) If(IsDBNull(value), Nothing, CType(value, Date))},
            {MySqlDbType.Boolean, Function(value As Object) If(IsDBNull(value) OrElse value = 0, False, True)}
        }

        Private Function UInt32_2_UInteger(value As Object) As Object
            If IsDBNull(value) Then
                Return Nothing
            Else
                Return CType(Val(CStr(value)), UInteger)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TypeHandler(type As MySqlDbType) As Func(Of Object, Object)
            Return _typeCasting(type)
        End Function
    End Module
End Namespace
