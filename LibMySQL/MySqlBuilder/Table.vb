Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Helper
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace MySqlBuilder

    Public Class Model

        ReadOnly mysql As MySqli
        ReadOnly schema As Table
        ReadOnly query As QueryBuilder
        ReadOnly chain As Model

        Dim m_getLastMySql As String

        ''' <summary>
        ''' get last execute mysql script
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GetLastMySql As String
            Get
                If chain Is Me Then
                    Return m_getLastMySql
                Else
                    Return chain.m_getLastMySql
                End If
            End Get
        End Property

        Public ReadOnly Property GetLastError As Exception
            Get
                Return mysql.LastError
            End Get
        End Property

        ''' <summary>
        ''' get the error message of the last <see cref="Exception"/>.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GetLastErrorMessage As String
            Get
                If mysql.LastError Is Nothing Then
                    Return ""
                Else
                    Return mysql.LastError.Message
                End If
            End Get
        End Property

        Public ReadOnly Property mysqli As ConnectionUri
            Get
                Return New ConnectionUri(mysql.UriMySQL)
            End Get
        End Property

        ''' <summary>
        ''' cache of the table schema
        ''' </summary>
        Shared ReadOnly cache As New Dictionary(Of String, Table)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="table">the table name</param>
        ''' <param name="conn"></param>
        Sub New(table As String, conn As ConnectionUri)
            Call Me.New(table, CType(conn, MySqli))
        End Sub

        Sub New(table As String, mysqli As MySqli)
            Me.mysql = mysqli
            Me.schema = inspectSchema(mysqli.UriMySQL.Database, table)
            Me.chain = Me
        End Sub

        Friend Sub New(mysql As MySqli, schema As Table, query As QueryBuilder, chain As Model)
            Me.mysql = mysql
            Me.schema = schema
            Me.query = query
            Me.chain = chain
        End Sub

        Public Overrides Function ToString() As String
            Return $"`{schema.Database}`.`{schema.TableName}`({schema.FieldNames.JoinBy(", ")})"
        End Function

        ''' <summary>
        ''' inspect of the database table schema
        ''' </summary>
        ''' <param name="database"></param>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Private Function inspectSchema(database As String, table As String) As Table
            Dim sql As String = $"describe `{database}`.`{table}`;"

            If Not cache.ContainsKey(sql) Then
                Dim schema = mysql.Query(Of FieldDescription)(sql) _
                    .ToDictionary(Function(f) f.Field,
                                  Function(f)
                                      Return f.CreateField
                                  End Function)
                Dim model As New Table(schema) With {
                    .Database = database,
                    .TableName = table
                }

                SyncLock cache
                    cache(sql) = model
                End SyncLock
            End If

            Return cache(sql)
        End Function

        ''' <summary>
        ''' WHERE
        ''' </summary>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function where(q As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("sql", q)
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' WHERE
        ''' </summary>
        ''' <param name="asserts"></param>
        ''' <returns></returns>
        Public Function where(ParamArray asserts As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("sql", asserts.Select(Function(f) f.ToString))
            Return New Model(mysql, schema, query, chain)
        End Function

        Public Function group_by(ParamArray fields As String()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.group_by = fields
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' `table`.`field`
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function field(name As String) As FieldAssert
            Return New FieldAssert With {.name = $"`{schema.TableName}`.`{name}`"}
        End Function

        ''' <summary>
        ''' `table`.`field`
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function f(name As String) As FieldAssert
            Return New FieldAssert With {.name = $"`{schema.TableName}`.`{name}`"}
        End Function

        Public Function [and](q As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("and", q)
            Return New Model(mysql, schema, query, chain)
        End Function

        Public Function [and](ParamArray asserts As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("and", asserts.Select(Function(f) f.ToString))
            Return New Model(mysql, schema, query, chain)
        End Function

        Public Function [or](q As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("or", q)
            Return New Model(mysql, schema, query, chain)
        End Function

        Public Function [or](ParamArray asserts As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("or", asserts.Select(Function(f) f.ToString))
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' SELECT LIMIT 1
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns>
        ''' this function returns nothing if not found
        ''' </returns>
        Public Function find(Of T As {New, Class})(ParamArray fields As String()) As T
            Dim where As String = If(query?.where_str, "")
            Dim left_join As String = If(query?.left_join_str, "")
            Dim order_by As String = If(query?.order_by_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim fieldSet As String = If(fields.IsNullOrEmpty, "*", fields.JoinBy(", "))
            Dim sql As String = $"SELECT {fieldSet} FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} {group_by} {order_by} LIMIT 1;"
            chain.m_getLastMySql = sql
            Dim result = mysql.ExecuteScalar(Of T)(sql)
            Return result
        End Function

        Public Function count() As Integer
            Dim where As String = If(query?.where_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim distinct As String = If(query?.distinct_str, "")
            Dim sql As String = $"SELECT count({distinct} *) FROM `{schema.Database}`.`{schema.TableName}` {where} {group_by};"
            chain.m_getLastMySql = sql
            Dim result = mysql.ExecuteAggregate(Of Integer)(sql)
            Return result
        End Function

        ''' <summary>
        ''' SELECT
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function [select](Of T As {New, Class})(ParamArray fields As String()) As T()
            Dim where As String = If(query?.where_str, "")
            Dim limit As String = If(query?.limit_str, "")
            Dim left_join As String = If(query?.left_join_str, "")
            Dim order_by As String = If(query?.order_by_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim distinct As String = If(query?.distinct_str, "")
            Dim fieldSet As String = If(fields.IsNullOrEmpty, "*", fields.JoinBy(", "))
            ' 20240324 group by should before the order by
            ' or the sql expression syntax error
            Dim sql As String = $"SELECT {distinct} {fieldSet} FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} {group_by} {order_by} {limit};"
            chain.m_getLastMySql = sql
            Dim result = mysql.Query(Of T)(sql)
            Return result
        End Function

        ''' <summary>
        ''' UPDATE
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function save(ParamArray fields As FieldAssert()) As Boolean
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim setFields As New List(Of String)

            For Each field As FieldAssert In fields
                If field.op <> "=" Then
                    Throw New InvalidOperationException
                End If

                Call setFields.Add($"{field.name} = {field.val}")
            Next

            Dim sql As String = $"UPDATE `{schema.Database}`.`{schema.TableName}` SET {setFields.JoinBy(", ")} {where} {limit};"
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function add(ParamArray fields As FieldAssert()) As Boolean
            Dim names As String = fields.Select(Function(a) a.name).JoinBy(", ")
            Dim vals As String = fields.Select(Function(a) a.val).JoinBy(", ")
            Dim sql As String = $"INSERT INTO `{schema.Database}`.`{schema.TableName}` ({names}) VALUES ({vals});"
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' DELETE FROM
        ''' </summary>
        ''' <returns></returns>
        Public Function delete() As Boolean
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim sql As String = $"DELETE FROM `{schema.Database}`.`{schema.TableName}` {where} {limit};"
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' LIMIT m,n
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function limit(m As Integer, Optional n As Integer? = Nothing) As Model
            Dim query As New QueryBuilder(Me.query)

            If n Is Nothing Then
                query.offset = m
            Else
                query.offset = m
                query.page_size = n
            End If

            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' LEFT JOIN
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Function left_join(table As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.join_tmp = table
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' LEFT JOIN ON
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function [on](ParamArray fields As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)

            If query.join_tmp.StringEmpty Then
                Throw New InvalidCastException
            Else
                query.left_join.Add(New NamedCollection(Of FieldAssert)(query.join_tmp, fields))
                query.join_tmp = Nothing
            End If

            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' ORDER BY
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function order_by(ParamArray fields As String()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.order_by = fields
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' ORDER BY DESC
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <param name="desc"></param>
        ''' <returns></returns>
        Public Function order_by(fields As String(), desc As Boolean) As Model
            Dim query As New QueryBuilder(Me.query)
            query.order_by = fields
            query.order_desc = desc
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' ORDER BY DESC
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="desc"></param>
        ''' <returns></returns>
        Public Function order_by(field As String, desc As Boolean) As Model
            Dim query As New QueryBuilder(Me.query)
            query.order_by = {field}
            query.order_desc = desc
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' DISTINCT
        ''' </summary>
        ''' <returns></returns>
        Public Function distinct() As Model
            Dim query As New QueryBuilder(Me.query)
            query.distinct = True
            Return New Model(mysql, schema, query, chain)
        End Function

    End Class
End Namespace