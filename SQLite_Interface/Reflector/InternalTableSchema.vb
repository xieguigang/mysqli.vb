Public Class InternalTableSchema
    Implements System.Collections.Generic.IEnumerable(Of SchemaCache)

    Public Property TableName As String
    Public Property DatabaseFields As SchemaCache()

    ''' <summary>
    ''' FieldName, DbType
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PrimaryKey As SchemaCache()

    Sub New(TypeInfo As Type)
        TableName = InternalGetTableName(TypeInfo)
        DatabaseFields = InternalGetSchemaCache(TypeInfo)
        PrimaryKey = (From item In DatabaseFields Where item.FieldEntryPoint.IsPrimaryKey Select item).ToArray
    End Sub

    Public Shared Function CreateObject(Of T As Class)() As InternalTableSchema
        Return New InternalTableSchema(TypeInfo:=GetType(T))
    End Function

    Public Overrides Function ToString() As String
        Return TableName
    End Function

    Public Iterator Function GetEnumerator() As IEnumerator(Of SchemaCache) Implements IEnumerable(Of SchemaCache).GetEnumerator
        For Each item In DatabaseFields
            Yield item
        Next
    End Function

    Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
