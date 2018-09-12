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

        Public Overrides Function ToString() As String
            Return $"Dim {Field} As {Type}"
        End Function

    End Class
End Namespace