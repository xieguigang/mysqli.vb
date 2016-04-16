Imports System.Data.Linq.Mapping
Imports System.Runtime.CompilerServices
Imports System.Data.Common
Imports System.Reflection
Imports Microsoft.VisualBasic

''' <summary>
''' Provides the reflection operation extensions for the generic collection data to updates database.
''' </summary>
''' <remarks></remarks>
Public Module Reflector

    Private ReadOnly DbFieldEntryPoint As System.Type = GetType(ColumnAttribute)

    ''' <summary>
    ''' Load the data from SELECT sql statement into a specific type collection.(Select语句加载数据)
    ''' </summary>
    ''' <typeparam name="T">The object type of the collection will be generated.</typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="SQL">SELECT SQL.(必须为Select语句)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 主要解析<see cref="System.Data.Linq.Mapping.ColumnAttribute.Name"></see>，<see cref="System.Data.Linq.Mapping.ColumnAttribute.IsPrimaryKey"></see>，
    ''' <see cref="System.Data.Linq.Mapping.ColumnAttribute.DbType"></see>这几个参数
    ''' </remarks>
    <Extension> Public Function Load(Of T As Class)(DbTransaction As SQLProcedure, SQL As String) As T()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim DataSource As DbDataReader = DbTransaction.Execute(SQL)
        Dim Table As New List(Of T)
        Dim SchemaCache = (From Field As SchemaCache
                           In Properties
                           Let idx As Integer = DataSource.GetOrdinal(name:=Field.DbFieldName)
                           Where idx > -1
                           Select idx,
                               Field.FieldEntryPoint,
                               Field.Property).ToArray

        Do While DataSource.Read
            Dim FetchedObject As T = Activator.CreateInstance(Of T)()

            For Each SchemaField In SchemaCache
                Call SchemaField.Property.SetValue(FetchedObject, value:=DataSource.GetValue(ordinal:=SchemaField.idx))
            Next

            Call Table.Add(FetchedObject)
        Loop

        Return Table.ToArray
    End Function

    ''' <summary>
    ''' Load all of the database in the database from a specific type information.
    ''' </summary>
    ''' <param name="DataSource"></param>
    ''' <param name="TypeInfo">The type information of the database table.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Load(DataSource As System.Data.Common.DbDataReader, TypeInfo As Type) As Object()
        Dim Table As New List(Of Object)
        Dim SchemaCache = (From Field In Reflector.InternalGetSchemaCache(TypeInfo)
                           Let idx As Integer = DataSource.GetOrdinal(name:=Field.DbFieldName)
                           Where idx > -1
                           Select idx, Field.FieldEntryPoint, Field.Property).ToArray

        Do While DataSource.Read
            Dim FetchedObject As Object = Activator.CreateInstance(TypeInfo)

            For Each SchemaField In SchemaCache
                Call SchemaField.Property.SetValue(FetchedObject, value:=DataSource.GetValue(ordinal:=SchemaField.idx))
            Next

            Call Table.Add(FetchedObject)
        Loop

        Return Table.ToArray
    End Function

    ''' <summary>
    ''' 这个函数会一次性加载所有的数据
    ''' </summary>
    ''' <param name="DbTransaction"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Load(Of T As Class)(DbTransaction As SQLProcedure) As T()
        Dim ChunkBuffer As Object() = DbTransaction.Load(SchemaInfo:=GetType(T))
        Dim LQuery = (From item In ChunkBuffer Select DirectCast(item, T)).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 这个函数会一次性加载所有的数据
    ''' </summary>
    ''' <param name="DbTransaction"></param>
    ''' <param name="SchemaInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Load(DbTransaction As SQLProcedure, SchemaInfo As Type) As Object()
        Dim Properties As SchemaCache() = Reflector.InternalGetSchemaCache(SchemaInfo)
        Dim TableName As String = GetTableName(SchemaInfo)
        Dim DataSource As DbDataReader = DbTransaction.Execute(SQL:=String.Format("SELECT * FROM '{0}';", TableName))
        Dim Table As New List(Of Object)
        Dim SchemaCache = (From Field In Properties Let idx As Integer = DataSource.GetOrdinal(name:=Field.DbFieldName) Where idx > -1 Select idx, Field.FieldEntryPoint, Field.Property).ToArray

        Do While DataSource.Read
            Dim FetchedObject As Object = Activator.CreateInstance(type:=SchemaInfo)

            For Each SchemaField In SchemaCache
                Call SchemaField.Property.SetValue(FetchedObject, value:=DataSource.GetValue(ordinal:=SchemaField.idx))
            Next

            Call Table.Add(FetchedObject)
        Loop

        Return Table.ToArray
    End Function

    ''' <summary>
    ''' Loading the database table schema information using the reflection operation from the meta data storted in the type schema.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function __getSchemaCache(Of T As Class)() As SchemaCache()
        Dim TypeInfo As Type = GetType(T)
        Dim Properties As SchemaCache() = InternalGetSchemaCache(TypeInfo)
        Return Properties
    End Function

    ''' <summary>
    ''' Loading the database table schema information using the reflection operation from the meta data storted in the type schema.
    ''' </summary>
    ''' <param name="TypeInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function InternalGetSchemaCache(TypeInfo As Type) As SchemaCache()
        Dim Properties As SchemaCache() = (From [Property] As PropertyInfo
                                           In TypeInfo.GetProperties()
                                           Let attrs As Object() = [Property].GetCustomAttributes(attributeType:=DbFieldEntryPoint, inherit:=True)
                                           Where Not attrs.IsNullOrEmpty
                                           Let EntryPoint = DirectCast(attrs.First, ColumnAttribute)
                                           Select New SchemaCache With {
                                               .Property = [Property],
                                               .DbFieldName = If(String.IsNullOrEmpty(EntryPoint.Name), [Property].Name, EntryPoint.Name),
                                               .FieldEntryPoint = EntryPoint}).ToArray
        If Properties.IsNullOrEmpty Then
            Throw New DataException(String.Format(SQLITE_UNABLE_LOAD_SQL_MAPPING_SCHEMA, TypeInfo.FullName))
        Else
            Return Properties
        End If
    End Function

    Const SQLITE_UNABLE_LOAD_SQL_MAPPING_SCHEMA As String = "SQLITE_UNABLE_LOAD_SQL_MAPPING_SCHEMA::Could not load any sql schema from table_schema_info ""{0}"""

    ''' <summary>
    ''' Get the table name from the type schema.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTableName(Of T As Class)() As String
        Dim TypeInfo As System.Type = GetType(T)
        Return GetTableName(TypeInfo)
    End Function

    ''' <summary>
    ''' Get the table name from the type schema.
    ''' </summary>
    ''' <param name="typeInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetTableName(typeInfo As System.Type) As String
        Dim attrs As Object() = typeInfo.GetCustomAttributes(GetType(TableAttribute), inherit:=True)
        If attrs.IsNullOrEmpty Then
            Return typeInfo.Name
        Else
            Dim TbName As String = DirectCast(attrs.First, TableAttribute).Name
            Return If(String.IsNullOrEmpty(TbName), typeInfo.Name, TbName)
        End If
    End Function

    Public Function Delete(DbTransaction As SQLProcedure, obj As Object) As Boolean
        Dim TypeInfo = obj.GetType
        Dim TableName As String = Reflector.GetTableName(TypeInfo)
        Dim Properties As SchemaCache() = Reflector.InternalGetSchemaCache(TypeInfo)
        Dim SQL As String = SchemaCache.CreateDeleteSQL(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    <Extension> Public Function Delete(Of T As Class)(DbTransaction As SQLProcedure, obj As T) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQL As String = SchemaCache.CreateDeleteSQL(Of T)(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 批量删除数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function FlushData(Of T As Class)(DbTransaction As SQLProcedure, data As IEnumerable(Of T)) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQLTransactions As String() = (From obj As T In data Select SchemaCache.CreateDeleteSQL(Of T)(Properties, obj, TableName)).ToArray
        Return DbTransaction.ExecuteTransaction(SQL:=SQLTransactions)
    End Function

    <Extension> Public Function Update(Of T As Class)(DbTransaction As SQLProcedure, obj As T) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQL As String = SchemaCache.CreateUpdateSQL(Of T)(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Call Console.WriteLine(ex.ToString)
            Return False
        End Try
    End Function

    Public Function Update(DbTransaction As SQLProcedure, obj As Object) As Boolean
        Dim Table = New TableSchema(TypeInfo:=obj.GetType)
        Dim SQL As String = SchemaCache.CreateUpdateSQL(Table.DatabaseFields, obj, Table.TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Call Console.WriteLine(ex.ToString)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 批量更新数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function CommitData(Of T As Class)(DbTransaction As SQLProcedure, data As System.Collections.Generic.IEnumerable(Of T)) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQLTransactions As String() = (From obj As T In data Select SchemaCache.CreateUpdateSQL(Of T)(Properties, obj, TableName)).ToArray
        Return DbTransaction.ExecuteTransaction(SQL:=SQLTransactions)
    End Function

    ''' <summary>
    ''' 批量添加新的数据
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Insert(Of T As Class)(DbTransaction As SQLProcedure, data As IEnumerable(Of T)) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQLTransactions As String() = (From obj As T In data Select SchemaCache.CreateInsertSQL(Of T)(Properties, obj, TableName)).ToArray
        Return DbTransaction.ExecuteTransaction(SQL:=SQLTransactions)
    End Function

    ''' <summary>
    ''' INSERT INTO Table VALUES (value1, value2, ....)
    ''' INSERT INTO table_name (col1, col2, ...) VALUES (value1, value2, ....)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Insert(Of T As Class)(DbTransaction As SQLProcedure, obj As T) As Boolean
        Dim TableName As String = Reflector.GetTableName(Of T)()
        Dim Properties As SchemaCache() = Reflector.__getSchemaCache(Of T)()
        Dim SQL As String = SchemaCache.CreateInsertSQL(Of T)(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' INSERT INTO Table VALUES (value1, value2, ....)
    ''' INSERT INTO table_name (col1, col2, ...) VALUES (value1, value2, ....)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="DbTransaction"></param>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Insert(Of T As Class)(DbTransaction As SQLProcedure, TableSchema As TableSchema, obj As T) As Boolean
        Dim TableName As String = TableSchema.TableName
        Dim Properties As SchemaCache() = TableSchema.DatabaseFields
        Dim SQL As String = SchemaCache.CreateInsertSQL(Of T)(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Call Console.WriteLine(ex.ToString)
            Return False
        End Try
    End Function

    Public Function Insert(DbTransaction As SQLProcedure, obj As Object) As Boolean
        Dim TypeInfo As System.Type = obj.GetType
        Dim TableName As String = Reflector.GetTableName(TypeInfo)
        Dim Properties As SchemaCache() = Reflector.InternalGetSchemaCache(TypeInfo)
        Dim SQL As String = SchemaCache.CreateInsertSQL(Properties, obj, TableName)

        Try
            Call DbTransaction.Execute(SQL)
            Return True
        Catch ex As Exception
            Call Console.WriteLine(ex.ToString)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' 判断某一个实体对象是否存在于数据库之中
    ''' </summary>
    ''' <param name="DbTransaction"></param>
    ''' <param name="obj"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RecordExists(DbTransaction As SQLProcedure, obj As Object) As Boolean
        Dim TableSchema = New TableSchema(TypeInfo:=obj.GetType)
        Dim PrimaryKey = TableSchema.PrimaryKey.First
        Dim SQL As String = String.Format("SELECT * FROM '{0}' WHERE {1} = {2};", TableSchema.TableName, PrimaryKey.DbFieldName, SchemaCache.__getValue(PrimaryKey, obj))
        Dim Result = DbTransaction.Execute(SQL)
        Return Result.HasRows
    End Function
End Module
