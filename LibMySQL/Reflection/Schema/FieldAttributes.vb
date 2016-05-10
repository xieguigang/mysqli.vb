Imports System.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports System.Text

Namespace Reflection.Schema

    Public Class Field

        Public Property FieldName As String
        Public Property Unique As Boolean
        Public Property PrimaryKey As Boolean
        Public Property DataType As DataType
        Public Property Unsigned As Boolean
        Public Property NotNull As Boolean
        ''' <summary>
        ''' 值是自动增长的
        ''' </summary>
        ''' <returns></returns>
        Public Property AutoIncrement As Boolean
        Public Property ZeroFill As Boolean
        Public Property Binary As Boolean
        Public Property [Default] As String = String.Empty

        ''' <summary>
        ''' The property information of this custom database field attribute. 
        ''' </summary>
        ''' <remarks></remarks>
        Public Property [PropertyInfo] As PropertyInfo

        Public Property Comment As String

        Public Overrides Function ToString() As String
            Dim sBuilder As StringBuilder = New StringBuilder("`%` ".Replace("%", FieldName), 512)

            Call sBuilder.AppendFormat("{0} ", DataType.ToString)

            If Unsigned Then sBuilder.AppendFormat("{0} ", "UNSIGNED")
            If ZeroFill Then sBuilder.AppendFormat("{0} ", "ZEROFILL")
            If NotNull Then sBuilder.AppendFormat("{0} ", "NOT NULL")
            If Binary Then sBuilder.AppendFormat("{0} ", "BINARY")
            If AutoIncrement Then sBuilder.AppendFormat("{0} ", "AUTO_INCREMENT")

            If Len([Default]) > 0 Then
                Select Case DataType.MySQLType
                    Case MySqlDbType.LongText, MySqlDbType.MediumText, MySqlDbType.Text, MySqlDbType.TinyText
                        Call sBuilder.AppendFormat("DEFAULT `{0}`", [Default])
                    Case Else
                        Call sBuilder.AppendFormat("DEFAULT {0}", [Default])
                End Select
            End If

            Return sBuilder.ToString
        End Function

        Public Shared Narrowing Operator CType(field As Field) As String
            Return field.ToString
        End Operator

        Public Shared Widening Operator CType([Property] As PropertyInfo) As Field
            Dim Field As New Field
            Dim DbField As DatabaseField = GetAttribute(Of DatabaseField)([Property])

            If Not DbField Is Nothing AndAlso Len(DbField.Name) > 0 Then
                Field.FieldName = DbField.Name
                Field.Unique = Not GetAttribute(Of Unique)([Property]) Is Nothing
                Field.PrimaryKey = Not GetAttribute(Of PrimaryKey)([Property]) Is Nothing
                Field.AutoIncrement = Not GetAttribute(Of AutoIncrement)([Property]) Is Nothing
                Field.Binary = Not GetAttribute(Of Binary)([Property]) Is Nothing
                Field.NotNull = Not GetAttribute(Of NotNULL)([Property]) Is Nothing
                Field.Unsigned = Not GetAttribute(Of Unsigned)([Property]) Is Nothing
                Field.ZeroFill = Not GetAttribute(Of ZeroFill)([Property]) Is Nothing
                Field.PropertyInfo = [Property]

                Dim DataType As DataType = GetAttribute(Of DataType)([Property])
                If DataType Is Nothing Then 'Not define this custom attribute.
                    DataType = [Property].PropertyType.GetDbDataType
                End If
                Field.DataType = DataType

                Dim [Default] As [Default] = GetAttribute(Of [Default])([Property])
                If Not [Default] Is Nothing Then
                    Field.Default = [Default].DefaultValue
                End If
            Else
                Return Nothing 'This property is not define as a database field as this property has no custom attribute of [DatabaseField].
            End If

            Return Field
        End Operator
    End Class
End Namespace