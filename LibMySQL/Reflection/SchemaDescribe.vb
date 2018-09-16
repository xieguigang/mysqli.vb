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
    End Class
End Namespace