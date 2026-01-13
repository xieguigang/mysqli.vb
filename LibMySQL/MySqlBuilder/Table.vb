#Region "Microsoft.VisualBasic::fda1967bd762b3fc91c345b9d7e3ca51, src\mysqli\LibMySQL\MySqlBuilder\Table.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 436
'    Code Lines: 264
' Comment Lines: 115
'   Blank Lines: 57
'     File Size: 16.50 KB


'     Class Model
' 
'         Properties: GetLastError, GetLastErrorMessage, GetLastMySql, mysqli
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: (+2 Overloads) [and], [on], (+2 Overloads) [or], (+2 Overloads) [select], add
'                   aggregate, count, delete, distinct, f
'                   field, find, getDriver, group_by, inspectSchema
'                   left_join, limit, (+3 Overloads) order_by, project, save
'                   selectSql, ToString, (+2 Overloads) where
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Oracle.LinuxCompatibility.MySQL.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Helper
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace MySqlBuilder

    Public Interface IModel

        Function field(name As String) As FieldAssert

    End Interface

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class Model : Implements IModel, IInsertModel(Of Model)

        ReadOnly query As QueryBuilder

        ''' <summary>
        ''' the source chain model
        ''' </summary>
        Friend ReadOnly chain As Model
        ''' <summary>
        ''' current model table schema
        ''' </summary>
        Friend ReadOnly schema As Table
        ''' <summary>
        ''' the mysqli connection and data driver
        ''' </summary>
        Friend ReadOnly mysql As MySqli

        Friend m_getLastMySql As String
        Friend m_opt As InsertOptions = InsertOptions.None

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

        ''' <summary>
        ''' get the clr exception error data for the last sql query
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GetLastError As Exception
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getDriver() As MySqli
            Return mysql
        End Function

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
                ' default throw exception on create model
                Dim desc As FieldDescription() = mysql.Query(Of FieldDescription)(sql, throwExp:=True)
                Dim schema = desc.ToDictionary(Function(f) f.Field, Function(f) f.CreateField)
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
        ''' <returns></returns>
        Public Function where(list As IEnumerable(Of FieldAssert)) As Model
            Return where(list.ToArray)
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
            query.having = New FilterConditions
            Return New Model(mysql, schema, query, chain)
        End Function

        Public Function having(ParamArray asserts As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.having.PushWhere("sql", asserts.Select(Function(f) f.ToString))
            Return New Model(mysql, schema, query, chain)
        End Function

        ''' <summary>
        ''' `table`.`field`
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function field(name As String) As FieldAssert Implements IModel.field
            Static prefix As New Regex("([^\.]+\.)|(`[^\.]+`\.)", RegexPythonRawString)

            Dim prefix_s As Match = prefix.Match(name)

            If prefix_s.Success Then
                ' already has table name prefix
                Return New FieldAssert With {.name = name}
            End If

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
            Return field(name)
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
            Dim result = mysql.ExecuteScalar(Of T)(findSql(fields))
            Return result
        End Function

        Private Function findSql(fields As String()) As String
            Dim where As String = If(query?.where_str, "")
            Dim left_join As String = If(query?.left_join_str, "")
            Dim order_by As String = If(query?.order_by_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim fieldSet As String = If(fields.IsNullOrEmpty, "*", fields.Select(AddressOf FieldAssert.EnsureSafeName).JoinBy(", "))
            Dim sql As String = $"SELECT {fieldSet} FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} {group_by} {order_by} LIMIT 1;"
            chain.m_getLastMySql = sql
            Return sql
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns>
        ''' this function returns nothing if no data could be found
        ''' </returns>
        Public Function find(ParamArray fields As String()) As Dictionary(Of String, Object)
            Dim hasLimit1 = query?.limit_str Is Nothing
            Dim sql As String = selectSql(fields, assign_sql:=True)
            Dim reader As System.Data.DataTableReader = mysql.Fetch(If(hasLimit1, sql, sql.Trim(";"c) & " LIMIT 1"))?.CreateDataReader
            Dim value As Dictionary(Of String, Object) = DbReflector.ReadFirst(reader)
            Return value
        End Function

        Public Function aggregate(Of T)(exp As String) As T
            Dim where As String = If(query?.where_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim sql As String = $"SELECT {exp} FROM `{schema.Database}`.`{schema.TableName}` {where} {group_by};"
            chain.m_getLastMySql = sql
            Dim result = mysql.ExecuteAggregate(Of T)(sql)
            Return result
        End Function

        Public Function count() As Long
            Dim where As String = If(query?.where_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim sql As String = $"SELECT count(*) FROM `{schema.Database}`.`{schema.TableName}` {where} {group_by};"
            chain.m_getLastMySql = sql
            ' bigint in mysql for count(*)
            Dim result = mysql.ExecuteAggregate(Of Long)(sql)
            Return result
        End Function

        ''' <summary>
        ''' SELECT
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function [select](Of T As {New, Class})(ParamArray fields As String()) As T()
            Dim sql As String = selectSql(fields, assign_sql:=True)
            Dim result = mysql.Query(Of T)(sql)

            Return result
        End Function

        Public Function [select](ParamArray fields As String()) As System.Data.DataTableReader
            Dim sql As String = selectSql(fields, assign_sql:=True)
            Dim resultSet As System.Data.DataSet = mysql.Fetch(sql)
            Dim result As System.Data.DataTableReader = resultSet?.CreateDataReader

            Return result
        End Function

        Private Function selectSql(fields As String(), assign_sql As Boolean) As String
            Dim where As String = If(query?.where_str, "")
            Dim limit As String = If(query?.limit_str, "")
            Dim left_join As String = If(query?.left_join_str, "")
            Dim order_by As String = If(query?.order_by_str, "")
            Dim group_by As String = If(query?.group_by_str, "")
            Dim distinct As String = If(query?.distinct_str, "")
            Dim fieldSet As String = If(fields.IsNullOrEmpty, "*", fields.Select(AddressOf FieldAssert.EnsureSafeName).JoinBy(", "))
            ' 20240324 group by should before the order by
            ' or the sql expression syntax error
            Dim sql As String = $"SELECT {distinct} {fieldSet} FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} {group_by} {order_by} {limit};"
            If assign_sql Then
                chain.m_getLastMySql = sql
            End If
            Return sql
        End Function

        ''' <summary>
        ''' SQL SELECT
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function select_sql(ParamArray fields As String()) As String
            Return selectSql(fields, assign_sql:=False).Trim(";"c, " "c)
        End Function

        ''' <summary>
        ''' project a field column as vector from the database
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="field">the field name to project as vector</param>
        ''' <returns></returns>
        Public Function project(Of T As IComparable)(field As String) As T()
            Dim sql As String = selectSql({field}, assign_sql:=True)
            Dim fieldName As String = FieldAssert.ParseFieldName(field)
            Dim vector As T() = mysql.Project(Of T)(sql, fieldName)
            Return vector
        End Function

        ''' <summary>
        ''' SQL UPDATE SET
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function save_sql(ParamArray fields As FieldAssert()) As String
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim setFields As New List(Of String)

            For Each field As FieldAssert In fields
                If field.op <> "=" Then
                    Throw New InvalidOperationException
                End If

                Call setFields.Add($"{field.GetSafeName} = {field.val}")
            Next

            Return $"UPDATE `{schema.Database}`.`{schema.TableName}` SET {setFields.JoinBy(", ")} {where} {limit};"
        End Function

        ''' <summary>
        ''' UPDATE
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function save(ParamArray fields As FieldAssert()) As Boolean
            Dim sql As String = save_sql(fields)
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' Create commit task data for make batch insert into current table
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function batch_insert(Optional max_blocksize As Integer = 1024, Optional opt As InsertOptions = InsertOptions.None) As CommitInsert
            Return New CommitInsert(Me, opt, maxBlockSize:=max_blocksize)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="blockSize"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' just create an in-memory cache of the batch transaction data
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function open_transaction(Optional blockSize As Integer = 1024) As CommitTransaction
            Return New CommitTransaction(Me, blockSize)
        End Function

        ''' <summary>
        ''' set delayed options for insert into
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' this delayed options will be reste to no-delayed after insert has been called
        ''' </remarks>
        Public Function delayed() As Model Implements IInsertModel(Of Model).delayed
            m_opt = InsertOptions.Delayed
            Return Me
        End Function

        Public Function ignore() As Model Implements IInsertModel(Of Model).ignore
            m_opt = InsertOptions.Ignore
            Return Me
        End Function

        Public Function clearOption() As Model Implements IInsertModel(Of Model).clearOption
            m_opt = InsertOptions.None
            Return Me
        End Function

        ''' <summary>
        ''' INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function add(ParamArray fields As FieldAssert()) As Boolean Implements IInsertModel(Of Model).add
            Dim sql As String = add_sql(fields)
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' SQL INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function add_sql(ParamArray fields As FieldAssert()) As String
            Dim names As String = fields.Select(Function(a) a.GetSafeName).JoinBy(", ")
            Dim vals As String = fields.Select(Function(a) a.val).JoinBy(", ")
            Dim sql As String = $"INSERT {m_opt.Description} INTO `{schema.Database}`.`{schema.TableName}` ({names}) VALUES ({vals});"
            Return sql
        End Function

        ''' <summary>
        ''' DELETE FROM
        ''' </summary>
        ''' <returns></returns>
        Public Function delete() As Boolean
            Dim sql As String = delete_sql()
            chain.m_getLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        ''' <summary>
        ''' SQL DELETE FROM
        ''' </summary>
        ''' <returns></returns>
        Public Function delete_sql() As String
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim sql As String = $"DELETE FROM `{schema.Database}`.`{schema.TableName}` {where} {limit};"
            Return sql
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
                Throw New InvalidCastException("no target table for make left join! A left_join function should be call at first!")
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
        ''' <remarks>
        ''' fields parameter data could be field name or an expression
        ''' </remarks>
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
