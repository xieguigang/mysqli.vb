Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Helper
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace MySqlBuilder

    Friend Class QueryBuilder

        Public where As New Dictionary(Of String, List(Of String))
        Public offset As Integer?
        Public page_size As Integer?
        Public left_join As New List(Of NamedCollection(Of FieldAssert))
        Public join_tmp As String

        Sub New(copy As QueryBuilder)
            If Not copy Is Nothing Then
                where = New Dictionary(Of String, List(Of String))(copy.where)
                offset = copy.offset
                page_size = copy.page_size
                left_join = New List(Of NamedCollection(Of FieldAssert))(copy.left_join)
                join_tmp = copy.join_tmp
            End If
        End Sub

        Sub New()
        End Sub

        Public Function PushWhere(type As String, val As String) As QueryBuilder
            If Not where.ContainsKey(type) Then
                where.Add(type, New List(Of String))
            End If

            where(type).Add(val)

            Return Me
        End Function

        Public Function PushWhere(type As String, vals As IEnumerable(Of String)) As QueryBuilder
            If Not where.ContainsKey(type) Then
                where.Add(type, New List(Of String))
            End If

            where(type).AddRange(vals.SafeQuery)

            Return Me
        End Function

        Public Function where_str() As String
            If where.IsNullOrEmpty Then
                Return ""
            Else
                Dim s As String = Nothing

                If where.ContainsKey("sql") Then
                    s = where("sql").JoinBy(" AND ")
                End If
                If where.ContainsKey("and") Then
                    If s.StringEmpty Then
                        s = $"({where("and").JoinBy(" AND ")})"
                    Else
                        s = $"({s}) AND ({where("and").JoinBy(" AND ")})"
                    End If
                End If
                If where.ContainsKey("or") Then
                    If s.StringEmpty Then
                        s = $"({where("or").JoinBy(" OR ")})"
                    Else
                        s = $"({s}) OR ({where("or").JoinBy(" OR ")})"
                    End If
                End If

                Return $"WHERE {s}"
            End If
        End Function

        Public Function limit_str() As String
            If offset Is Nothing Then
                Return ""
            ElseIf page_size Is Nothing Then
                Return $"LIMIT {offset}"
            Else
                Return $"LIMIT {offset},{page_size}"
            End If
        End Function

        Public Function left_join_str() As String
            If left_join.IsNullOrEmpty Then
                Return ""
            Else
                Dim str As New List(Of String)

                For Each tbl In left_join
                    str.Add($"LEFT JOIN `{tbl.name}` ON ({tbl.value.JoinBy(" AND ")})")
                Next

                Return str.JoinBy(" ")
            End If
        End Function
    End Class

    Public Class Model

        ReadOnly mysql As MySqli
        ReadOnly schema As Table
        ReadOnly query As QueryBuilder

        Public Shared ReadOnly Property GetLastMySql As String

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
            Me.mysql = conn
            Me.schema = inspectSchema(conn.Database, table)
        End Sub

        Friend Sub New(mysql As MySqli, schema As Table, query As QueryBuilder)
            Me.mysql = mysql
            Me.schema = schema
            Me.query = query
        End Sub

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

        Public Function where(q As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("sql", q)
            Return New Model(mysql, schema, query)
        End Function

        Public Function where(ParamArray asserts As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("sql", asserts.Select(Function(f) f.ToString))
            Return New Model(mysql, schema, query)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function field(name As String) As FieldAssert
            Return New FieldAssert With {.name = $"`{schema.TableName}`.`{name}`"}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function f(name As String) As FieldAssert
            Return New FieldAssert With {.name = $"`{schema.TableName}`.`{name}`"}
        End Function

        Public Function [and](q As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.PushWhere("and", q)
            Return New Model(mysql, schema, query)
        End Function

        Public Function [and]() As Model

        End Function

        Public Function find(Of T As {New, Class})() As T
            Dim where As String = query.where_str
            Dim left_join As String = query.left_join_str
            Dim sql As String = $"SELECT * FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} LIMIT 1;"
            _GetLastMySql = sql
            Dim result = mysql.ExecuteScalar(Of T)(sql)
            Return result
        End Function

        Public Function [select](Of T As {New, Class})() As T()
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim left_join As String = query.left_join_str
            Dim sql As String = $"SELECT * FROM `{schema.Database}`.`{schema.TableName}` {left_join} {where} {limit};"
            _GetLastMySql = sql
            Dim result = mysql.Query(Of T)(sql)
            Return result
        End Function

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
            _GetLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        Public Function add(ParamArray fields As FieldAssert()) As Boolean
            Dim names As String = fields.Select(Function(a) a.name).JoinBy(", ")
            Dim vals As String = fields.Select(Function(a) a.val).JoinBy(", ")
            Dim sql As String = $"INSERT INTO `{schema.Database}`.`{schema.TableName}` ({names}) VALUES ({vals});"
            _GetLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        Public Function delete() As Boolean
            Dim where As String = query.where_str
            Dim limit As String = query.limit_str
            Dim sql As String = $"DELETE FROM `{schema.Database}`.`{schema.TableName}` {where} {limit};"
            _GetLastMySql = sql
            Dim result = mysql.Execute(sql)
            Return result > 0
        End Function

        Public Function limit(m As Integer, Optional n As Integer? = Nothing) As Model
            Dim query As New QueryBuilder(Me.query)

            If n Is Nothing Then
                query.offset = m
            Else
                query.offset = m
                query.page_size = n
            End If

            Return New Model(mysql, schema, query)
        End Function

        Public Function left_join(table As String) As Model
            Dim query As New QueryBuilder(Me.query)
            query.join_tmp = table
            Return New Model(mysql, schema, query)
        End Function

        Public Function [on](ParamArray fields As FieldAssert()) As Model
            Dim query As New QueryBuilder(Me.query)

            If query.join_tmp.StringEmpty Then
                Throw New InvalidCastException
            Else
                query.left_join.Add(New NamedCollection(Of FieldAssert)(query.join_tmp, fields))
                query.join_tmp = Nothing
            End If

            Return New Model(mysql, schema, query)
        End Function

    End Class
End Namespace