#Region "Microsoft.VisualBasic::5991bbbb3bd4069473d1f2be9743c6cc, src\mysqli\LibMySQL\Mysqli\MySqli.vb"

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

    '   Total Lines: 620
    '    Code Lines: 357
    ' Comment Lines: 178
    '   Blank Lines: 85
    '     File Size: 23.93 KB


    ' Class MySqli
    ' 
    '     Properties: LastError, LastInsertedId, UriMySQL, Version
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: __executeAggregate, __throwExceptionHelper, CommitInserts, CommitTransaction, (+2 Overloads) Connect
    '               ConnectDatabase, CreateQuery, CreateSchema, ExecDelete, ExecInsert
    '               ExecReplace, ExecUpdate, Execute, ExecuteAggregate, ExecuteDataset
    '               ExecuteScalar, ExecuteScalarAuto, Fetch, ForEach, Ping
    '               Project, Query, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose
    ' 
    '     Operators: +, <=, >=
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON
Imports MySql.Data.MySqlClient
Imports Oracle.LinuxCompatibility.MySQL.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Scripting
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' MySql database server connection module.
''' </summary>
''' <remarks>(与MySql数据库服务器之间的通信操作的封装模块)</remarks>
Public Class MySqli : Implements IDisposable

    ''' <summary>
    ''' A error occurred during the execution of a sql command or transaction.
    ''' (在执行SQL命令或者提交一个事务的时候发生了错误) 
    ''' </summary>
    ''' <param name="Ex">
    ''' The detail information of the occurred error.
    ''' (所发生的错误的详细信息)
    ''' </param>
    ''' <remarks></remarks>
    Public Event ThrowException(Ex As Exception, SQL As String)

    Dim _lastError As Exception
    Dim _lastMySql As String

    ''' <summary>
    ''' 20240319
    ''' 
    ''' <see cref="App.LogException(Exception, ByRef String)"/> method will create 
    ''' too much log file in the app data directory, this may crash the filesystem
    ''' when too much error happends. so use this log file module instead the app 
    ''' log exception method.
    ''' </summary>
    Dim _log As LogFile
    Dim _logfile As String

    ''' <summary>
    ''' A Formatted connection string using for the connection established to the database server. 
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property UriMySQL As ConnectionUri
    Public ReadOnly Property LastInsertedId As Long
    Public ReadOnly Property LastError As Exception
        Get
            Return _lastError
        End Get
    End Property

    ''' <summary>
    ''' get last mysql statement that executed.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property LastMySql As String
        Get
            Return _lastMySql
        End Get
    End Property

    Public ReadOnly Property Version As String
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Using MySQL As New MySqlConnection(_UriMySQL)
                Return MySQL.ServerVersion
            End Using
        End Get
    End Property

    Public Sub New(strUri As String, Optional logfile As String = Nothing)
        Call Me.New(CType(strUri, ConnectionUri), logfile)
    End Sub

    ''' <summary>
    ''' Creates a new mysql session object from connection uri.
    ''' </summary>
    ''' <param name="uri"></param>
    Sub New(uri As ConnectionUri, Optional logfile As String = Nothing)
        _logfile = If(logfile, uri.error_log)
        Connect(uri)
    End Sub

    Public Overrides Function ToString() As String
        Return UriMySQL.GetJson
    End Function

    ''' <summary>
    ''' Connect to the database server using a assigned mysql connection helper object. The function returns the ping value of the client to the MYSQL database server.
    ''' (使用一个由用户所指定参数的连接字符串生成器来打开一个对服务器的连接，之后返回客户端对数据库服务器的ping值) 
    ''' </summary>
    ''' <param name="MySQLConnection"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(MySQLConnection As ConnectionUri, Optional OnCreateSchema As Boolean = False) As Double
        Dim tempName$ = MySQLConnection.Database

        If OnCreateSchema Then
            MySQLConnection.Database = ""
            _UriMySQL = MySQLConnection
            Call CreateSchema(Name:=MySQLConnection.Database)
            MySQLConnection.Database = tempName
        End If

        Return Connect(ConnectionString:=MySQLConnection.GetConnectionString)
    End Function

    Public Function ConnectDatabase(DbName As String) As MySqli
        Dim uri As New ConnectionUri(UriMySQL, DbName)
        Return New MySqli(uri)
    End Function

    Private Function CreateSchema(Name As String) As Boolean
        Const CREATE_SCHEMA As String = "CREATE DATABASE /*!32312 IF NOT EXISTS*/ {0};"
        Return Me.Execute(String.Format(CREATE_SCHEMA, Name)) = 0
    End Function

    ''' <summary>
    ''' Connect to the database server using a assigned mysql connection string.
    ''' (使用一个由用户所制定的连接字符串连接MySql数据库服务器) 
    ''' </summary>
    ''' <param name="ConnectionString"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Connect(ConnectionString As String) As Double
        _UriMySQL = ConnectionString
        _logfile = If(_logfile, $"{App.AppSystemTemp}/{UriMySQL.Database}_{LogFile.NowTimeNormalizedString}.log")
        _logfile = _logfile.GetFullPath
        _log = New LogFile(_logfile, autoFlush:=True)

        Call VBDebugger.EchoLine($"mysqli_error_log: {_logfile}")

        Return Ping()
    End Function

    ''' <summary>
    ''' Executes the query, and returns the first column of the first row in the 
    ''' result set returned by the query. Additional columns or rows are ignored.
    ''' (请注意，这个函数会自动判断添加``LIMIT 1``限定在SQL语句末尾)
    ''' </summary>
    ''' <returns>如果数据表之中没有符合条件的结果数据，那么这个函数将会返回空值</returns>
    ''' <param name="SQL">
    ''' 这个函数会自动进行判断添加``LIMIT 1``限定，所以不需要刻意担心
    ''' </param>
    Public Function ExecuteScalar(Of T As {New, Class})(SQL As String) As T
        Dim result As DataSet = Fetch(SQL.Trim.EnsureLimit1)
        Dim reader As DataTableReader = result?.CreateDataReader
        Dim value As T = DbReflector.ReadFirst(Of T)(reader)
        _lastMySql = SQL
        Return value
    End Function

    ''' <summary>
    ''' 执行聚合函数并返回值
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    Public Function ExecuteAggregate(Of T)(SQL As String) As T
        Try
            Return ExecuteAggregateInternal(Of T)(SQL)
        Catch ex As Exception
            ex = __throwExceptionHelper(ex, SQL, False)
            Call ex.PrintException
            Call _log.LogException(ex, "mysqli_execute_aggregate")

            Return Nothing
        Finally

        End Try
    End Function

    Private Function ExecuteAggregateInternal(Of T)(SQL As String) As T
        Dim Result As DataSet = Fetch(SQL)
        Dim Reader As DataTableReader = Result.CreateDataReader

        Call Reader.Read()

        ' 直接类型转换可能会存在类型不匹配的错误
        ' 因为在mysql表之中所有的字段值都是基本类型，所以在这里将结果值转换为字符串
        ' 在通过字符串转换为目标类型值
        ' 这个间接的转换方法比较安全，不容易崩溃
        Dim objValue$ = InputHandler.ToString(Reader.GetValue(Scan0))
        Dim value As T = InputHandler.CTypeDynamic(objValue, GetType(T))
        _lastMySql = SQL
        Return value
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="[where]">只需要给出条件WHERE表达式即可，函数会自动生成SQL查询语句</param>
    ''' <returns></returns>
    Public Function ExecuteScalarAuto(Of T As {New, Class})([where] As String) As T
        Dim Tbl As TableName = TableName.GetTableName(Of T)
        Dim SQL As String = $"SELECT * FROM `{Tbl.Database}`.`{Tbl.Name}` WHERE {where} LIMIT 1;"
        Return ExecuteScalar(Of T)(SQL)
    End Function

    Public Function ExecuteDataset(SQL As String) As MySqlDataReader
        Dim MySQL As New MySqlConnection(_UriMySQL)
        Dim MySqlCommand As New MySqlCommand(SQL) With {
            .Connection = MySQL
        }

        _lastMySql = SQL

        Try
            Call MySQL.Open()
            Return MySqlCommand.ExecuteReader
        Catch ex As Exception
            ex = __throwExceptionHelper(ex, SQL, False)
            Call ex.PrintException
            Call _log.LogException(ex, "mysqli_execute_dataset")

            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Execute a DML/DDL sql command and then return the row number that the row was affected 
    ''' by this command, and you should open a connection to a database server before you call 
    ''' this function. 
    ''' (执行一个DML/DDL命令并且返回受此命令的执行所影响的行数，你应该在打开一个数据库服务器的连接之
    ''' 后调用本函数，执行SQL语句发生错误时会返回负数)
    ''' </summary>
    ''' <param name="SQL">DML/DDL sql command, not a SELECT command(DML/DDL 命令，而非一个SELECT语句)</param>
    ''' <returns>
    ''' Return the row number that was affected by the DML/DDL command, if the databse 
    ''' server connection is interrupt or errors occurred during the executes, this 
    ''' function will return a negative number.
    ''' (返回受DML/DDL命令所影响的行数，假若数据库服务器断开连接或者在命令执行的期间发生错误，
    ''' 则这个函数会返回一个负数)
    ''' </returns>
    ''' <remarks></remarks>
    Public Function Execute(SQL$, Optional throwExp As Boolean = False) As Integer
        Using MySQL As New MySqlConnection(_UriMySQL)
            Dim MySqlCommand As New MySqlCommand(SQL) With {
                .Connection = MySQL
            }
            Dim isInsert As Boolean = InStr(SQL.Trim, "insert", CompareMethod.Text) > 0
            Dim i%

            _lastMySql = SQL

            Try
                MySQL.Open()
                i = MySqlCommand.ExecuteNonQuery

                If isInsert Then
                    _LastInsertedId = MySqlCommand.LastInsertedId
                End If

                Return i
            Catch ex As Exception
                If throwExp Then
                    __throwExceptionHelper(ex, SQL, throwExp:=True)
                Else
                    ex = __throwExceptionHelper(ex, SQL, False)

                    Call ex.PrintException
                    Call _log.LogException(ex, "mysqli_execute_sql")
                End If

                Return -1
            Finally
                MySQL.Close()
            End Try
        End Using
    End Function

    ''' <summary>
    ''' Execute a 'SELECT' query command and then returns the query result of this sql command.
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <returns>
    ''' this function may returns nothing when mysql query error happends
    ''' </returns>
    ''' <remarks>(执行一个'SELECT'查询命令之后返回本查询命令的查询结果。请注意，这个工具并不会自动关闭数据库连接，
    ''' 请在使用完毕之后手工Close掉，以节省服务器资源) </remarks>
    Public Function Fetch(SQL$, Optional throwEx As Boolean = False) As DataSet
        Dim MySql As New MySqlConnection(_UriMySQL)
        Dim data As New DataSet
        Dim MySqlCommand As New MySqlCommand(SQL) With {
            .Connection = MySql
        }
        Dim Adapter As New MySqlDataAdapter() With {
            .SelectCommand = MySqlCommand
        }

        _lastMySql = SQL

        Try
            Call Adapter.Fill(data)
        Catch ex As Exception
            If throwEx Then
                Throw New Exception(SQL, ex)
            Else

                ' wrap the mysql execute context into exception
                ' and set the last error
                ex = __throwExceptionHelper(ex, SQL, False)

                Call ex.PrintException
                Call _log.LogException(ex, "mysqli_fetch")

                Return Nothing
            End If
        End Try

        Return data
    End Function

    Public Iterator Function ForEach(Of T As {New, Class})(SQL As String, Optional throwEx As Boolean = True) As IEnumerable(Of T)
        Dim Result As DataSet = Fetch(SQL)
        Dim Reader As DataTableReader = Result.CreateDataReader
        Dim Err As Value(Of String) = ""

        For Each item As T In DbReflector.Load(Of T)(Reader, getErr:=Err)
            Yield item
        Next

        If Not Err.Value.StringEmpty Then
            If throwEx Then
                Dim ex As New Exception(SQL)
                ex = New Exception(Err, ex)
                Throw ex
            End If
        End If
    End Function

    ''' <summary>
    ''' 使用这个函数进行批量数据的查询操作，基于反射操作的ORM解决方案。
    ''' 假若只需要查询一条数据库记录的话，则推荐使用<see cref="ExecuteScalar(Of T)(String)"/>函数以获取更高的性能
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="SQL"></param>
    ''' <param name="throwExp"></param>
    ''' <returns>
    ''' this function will returns nothing if the mysql query error, 
    ''' this is different with empty array when for query no data.
    ''' </returns>
    Public Function Query(Of T As Class)(SQL As String, Optional throwExp As Boolean = True) As T()
        Dim Result As DataSet = Fetch(SQL)
        Dim Reader As DataTableReader = Result?.CreateDataReader
        Dim Err As Value(Of String) = ""
        Dim table As T()

        If Reader Is Nothing Then
            ' mysql query error
            Return Nothing
        Else
            table = DbReflector.Load(Of T)(Reader, getErr:=Err).ToArray
        End If

        If Not Err.Value.StringEmpty Then
            Dim ex As New Exception(SQL)
            ex = New Exception(Err, ex)

            If throwExp Then
                Throw ex
            Else
                Call _log.LogException(ex, "mysqli_fetch_query")
                Return Nothing
            End If
        Else
            Return table
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="sql"></param>
    ''' <param name="field"></param>
    ''' <param name="throwExp"></param>
    ''' <returns>
    ''' this function will returns nothing if the mysql query error
    ''' </returns>
    Public Function Project(Of T As IComparable)(sql As String, field As String, Optional throwExp As Boolean = True) As T()
        Dim Result As DataSet = Fetch(sql)
        Dim Reader As DataTableReader = Result?.CreateDataReader
        Dim Err As Value(Of String) = ""
        Dim array As T()

        If Reader Is Nothing Then
            Return Nothing
        Else
            array = DbReflector.LoadProject(Of T)(Reader, field, getErr:=Err).ToArray
        End If

        If Not Err.Value.StringEmpty Then
            Dim ex As New Exception(sql)
            ex = New Exception(Err, ex)

            If throwExp Then
                Throw ex
            Else
                Call _log.LogException(ex, "mysqli_fetch_query")
                Return Nothing
            End If
        Else
            Return array
        End If
    End Function

    Public Function CreateQuery(SQL As String) As MySqlDataReader
        Dim MySql As MySqlConnection = New MySqlConnection(_UriMySQL) '[ConnectionString] is a compiled mysql connection string from our class constructor.
        Dim MySqlCommand As MySqlCommand = New MySqlCommand(SQL, MySql)
        _lastMySql = SQL
        Try
            MySql.Open()
            Return MySqlCommand.ExecuteReader(CommandBehavior.CloseConnection)
        Catch ex As Exception
            ex = __throwExceptionHelper(ex, SQL, False)
            Call _log.LogException(ex, "mysqli_create_query")
            Call ex.PrintException
            Return Nothing
        Finally

        End Try
    End Function

#Region ""

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExecUpdate(SQL As MySQLTable, Optional throwExp As Boolean = False) As Boolean
        Return Execute(SQL.GetUpdateSQL, throwExp) > 0
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExecInsert(SQL As MySQLTable, Optional throwExp As Boolean = False) As Boolean
        Return Execute(SQL.GetInsertSQL, throwExp) > 0
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExecDelete(SQL As MySQLTable, Optional throwExp As Boolean = False) As Boolean
        Return Execute(SQL.GetDeleteSQL, throwExp) > 0
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExecReplace(SQL As MySQLTable, Optional throwExp As Boolean = False) As Boolean
        Return Execute(SQL.GetReplaceSQL, throwExp) > 0
    End Function
#End Region

    Public Function CommitInserts(transaction As IEnumerable(Of MySQLTable), Optional ByRef ex As Exception = Nothing) As Boolean
        Dim SQL$ = transaction _
            .Select(Function(x) x.GetInsertSQL) _
            .JoinBy(vbLf)
        Return CommitTransaction(SQL, ex)
    End Function

    ''' <summary>
    ''' Commit a transaction command collection to the database server and then return the 
    ''' result that this transaction is commit successfully or not. 
    ''' (向数据库服务器提交一个事务之后返回本事务是否被成功提交)
    ''' </summary>
    ''' <param name="transaction"></param>
    ''' <returns>
    ''' Return the result that this transaction is commit succeedor not.
    ''' (返回本事务是否被成功提交至数据库服务器)
    ''' </returns>
    ''' <remarks></remarks>
    Public Function CommitTransaction(transaction$, Optional ByRef excep As Exception = Nothing) As Boolean
        Using MyConnection As New MySqlConnection(_UriMySQL)
            MyConnection.Open()

            Dim MyCommand As MySqlCommand = MyConnection.CreateCommand()
            Dim MyTrans As MySqlTransaction

            ' Start a local transaction
            MyTrans = MyConnection.BeginTransaction()

            ' Must assign both transaction object and connection
            ' to Command object for a pending local transaction
            MyCommand.Connection = MyConnection
            MyCommand.Transaction = MyTrans
            _lastMySql = transaction
            Try
                MyCommand.CommandText = transaction
                MyCommand.ExecuteNonQuery()
                MyTrans.Commit()

                Return True
            Catch e As Exception
                Try
                    MyTrans.Rollback()
                Catch ex As MySqlException
                    e = New Exception(__throwExceptionHelper(ex, transaction, False).ToString, e)
                End Try
                excep = e
                Return False
            Finally
                MyConnection.Close()
            End Try
        End Using
    End Function

    ''' <summary>
    ''' wrap the sql execute context and set the <see cref="LastError"/>
    ''' </summary>
    ''' <param name="ex"></param>
    ''' <param name="SQL$"></param>
    ''' <param name="throwExp"></param>
    ''' <returns></returns>
    Private Function __throwExceptionHelper(ex As Exception, SQL$, throwExp As Boolean) As Exception
        Dim url As New ConnectionUri(UriMySQL)

        url.Password = "********"
        ex = New Exception(url.GetJson, ex)
        ex = New Exception(SQL, ex)
        _lastError = ex

        If throwExp Then
            Throw ex
        Else
            Return ex
        End If
    End Function

    ''' <summary>
    ''' Test the connection of the client to the mysql database server and then 
    ''' return the communication delay time between the client and the server. 
    ''' This function should be call after you connection to a database server.
    ''' (测试客户端和MySql数据库服务器之间的通信连接并且返回二者之间的通信延时。
    ''' 这个函数应该在你连接上一个数据库服务器之后进行调用，-1值表示客户端与服务器之间通信失败.)
    ''' </summary>
    ''' <returns>当函数返回一个负数的时候，表明Ping操作失败，即无数据库服务器连接</returns>
    ''' <remarks></remarks>
    Public Function Ping() As Double
        Dim Flag As Boolean
        Dim DelayTime As Double

        Using MySql = New MySqlConnection(_UriMySQL)
            Try
                MySql.Open()
            Catch ex As Exception
                ex = __throwExceptionHelper(ex, "null", False)
                Call ex.PrintException
                Call _log.LogException(ex, $"mysqli_ping_{UriMySQL.Database}")
                Return -1
            End Try

            Dim DelayTimer = Stopwatch.StartNew
            Flag = MySql.Ping
            DelayTime = DelayTimer.ElapsedMilliseconds

            MySql.Close()
        End Using

        If Flag Then
            Return DelayTime
        Else
            Return -1
        End If
    End Function

    ''' <summary>
    ''' Open a mysql connection using a specific connection string
    ''' </summary>
    ''' <param name="strUri">The mysql connection string</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(strUri As String) As MySqli
        Return New MySqli(CType(strUri, ConnectionUri))
    End Operator

    ''' <summary>
    ''' Open a  mysql  connection using a connection helper object
    ''' </summary>
    ''' <param name="uri">The connection helper object</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(uri As ConnectionUri) As MySqli
        Return New MySqli(uri)
    End Operator

    ''' <summary>
    ''' ``mysql.Connect(cnn)``, 返回-1表示与数据库服务器通信失败
    ''' </summary>
    ''' <param name="mysql"></param>
    ''' <param name="cnn"></param>
    ''' <returns></returns>
    Public Shared Operator <=(mysql As MySqli, cnn As ConnectionUri) As Double
        Return mysql.Connect(cnn)
    End Operator

    Public Shared Operator >=(mysql As MySqli, cnn As ConnectionUri) As Double
        Throw New NotSupportedException
    End Operator

    Public Shared Operator +(mysqli As MySqli, res As MySQLTable) As MySqli
        mysqli.ExecInsert(res)
        Return mysqli
    End Operator

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call _log.Save()
                Call _log.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
