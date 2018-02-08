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

        Public Shared Function GetTableName(Of T As Class)() As TableName
            Dim attrs() = GetType(T).GetCustomAttributes(GetType(TableName), inherit:=True)

            If attrs.IsNullOrEmpty Then
                Return Nothing
            Else
                Return DirectCast(attrs(Scan0), TableName)
            End If
        End Function
    End Class
End Namespace