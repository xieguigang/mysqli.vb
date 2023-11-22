Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.Helper

    ''' <summary>
    ''' result of mysql ``describ table``
    ''' </summary>
    Public Class FieldDescription

        <DatabaseField("Field")> Public Property Field As String
        <DatabaseField("Type")> Public Property Type As String
        <DatabaseField("Null")> Public Property Null As String
        <DatabaseField("Key")> Public Property Key As String
        <DatabaseField("Default")> Public Property [Default] As String
        <DatabaseField("Extra")> Public Property Extra As String

        Public Function CreateField() As Field
            Return New Field With {
                .FieldName = Field,
                .AutoIncrement = Extra.TextEquals("auto_increment"),
                .DataType = CreateDataType.CreateDataType(type_define:=Type),
                .NotNull = Null.TextEquals("NO"),
                .[Default] = [Default],
                .PrimaryKey = Key.TextEquals("PRI"),
                .Unsigned = Type.IndexOf("unsigned") > -1
            }
        End Function

    End Class
End Namespace