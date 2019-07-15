Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection

    ''' <summary>
    ''' ``DESCRIBE `db`.`tableName```
    ''' </summary>
    Public Class SchemaDescribe

        Public Property Field As String
        Public Property Type As String
        Public Property Null As String
        Public Property Key As String
        Public Property [Default] As String
        Public Property Extra As String
        Public Property Note As String

        Public Property MySqlType As MySqlDbType

        Public ReadOnly Property IsAutoIncrement As Boolean
            Get
                Dim isAutoIncre = Extra = "auto_increment"
                Dim isInt32 = InStr(Type, "int") > 0

                Return isInt32 AndAlso isAutoIncre
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"Dim {Field} As {Type}"
        End Function

        Public Shared Function FromTable(schema As Table) As NamedCollection(Of SchemaDescribe)
            Dim name$ = schema.TableName
            Dim describs = schema _
                .Fields _
                .Select(AddressOf FromField) _
                .ToArray

            Return New NamedCollection(Of SchemaDescribe)(name, describs) With {
                .description = schema.Comment
            }
        End Function

        Public Shared Function FromField(field As Field) As SchemaDescribe
            Return New SchemaDescribe With {
                .Field = field.FieldName,
                .Type = field.DataType.ToString,
                .MySqlType = field.DataType.MySQLType,
                .[Default] = field.Default,
                .Extra = "" Or "auto_increment".When(field.AutoIncrement),
                .Key = "" Or "PRI".When(field.PrimaryKey),
                .Null = "YES" Or "NO".When(field.NotNull),
                .Note = field.Comment
            }
        End Function
    End Class
End Namespace