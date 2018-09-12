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

    End Class
End Namespace