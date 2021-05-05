Imports System.Text
Imports MySqlConnector
Imports Oracle.LinuxCompatibility.MySQL.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri
Imports any = Microsoft.VisualBasic.Scripting

Public Class QueryHandler(Of Schema) : Implements IDisposable

    Dim WithEvents MySQL As MySqli
    Dim sql As New DataTable(Of Schema)

    ''' <summary>
    ''' The sql transaction that will be commit to the mysql database.
    ''' (将要被提交至MYSQL数据库之中的SQL事务集)
    ''' </summary>
    ''' <remarks></remarks>
    Dim Transaction As New StringBuilder(2048)
    Dim p As Long

    ''' <summary>
    ''' DataSet of the table in the database.
    ''' (数据库的表之中的数据集)
    ''' </summary>
    ''' <remarks></remarks>
    Friend _listData As New List(Of Schema)(capacity:=1024)

    ''' <summary>
    ''' The error information that come from MYSQL database server.
    ''' (来自于MYSQL数据库服务器的错误信息)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ErrorMessage As String

    ''' <summary>
    ''' DataSet of the table in the database. Do not edit the data directly from this property...
    ''' (数据库的表之中的数据集，请不要直接在这个属性之上修改数据)
    ''' </summary>
    ''' <remarks></remarks>
    ReadOnly Property ListData As IEnumerable(Of Schema)
        Get
            Return _listData
        End Get
    End Property

    ''' <summary>
    ''' Execute create table sql
    ''' </summary>
    ''' <returns></returns>
    Public Function Create() As Boolean
        Dim sql As String = Me.sql.Create
#If DEBUG Then
        Console.WriteLine(sql)
#End If
        Return MySQL.Execute(sql)
    End Function

    Public Sub Delete(Record As Schema)
        Call _listData.Remove(Record)
        Call Transaction.AppendLine(sql.Delete(Record))
    End Sub

    Public Sub Insert(record As Schema)
        Call _listData.Add(record)
        Call Transaction.AppendLine(sql.Insert(record))
    End Sub

    Public Sub Update(record As Schema)
        Dim OldRecord As Schema = GetHandle(record)
        Dim Handle As Integer = _listData.IndexOf(OldRecord)

        _listData(Handle) = record

        Call Transaction.AppendLine(sql.Update(record))
    End Sub

    ''' <summary>
    ''' Get a specific record in the dataset by compaired the UNIQUE_INDEX field value.
    ''' (通过值唯一的索引字段来获取一个特定的数据记录)
    ''' </summary>
    ''' <param name="Record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetHandle(Record As Schema) As Schema
        ' Get the Index field value
        Dim [String] As String = any.ToString(sql.GetValue(Record))
        ' Use LINQ and index value find out the target item 
        Dim LQuery As IEnumerable(Of Schema) =
            From schema As Schema
            In _listData
            Let val As Object = sql.GetValue(schema)
            Let str As String = any.ToString(val)
            Where String.Equals([String], str)
            Select schema

        ' return the item handle
        Return LQuery.First
    End Function

    ''' <summary>
    ''' Query a data table using Reflection.(使用反射机制来查询一个数据表)
    ''' </summary>
    ''' <param name="SQL">Sql 'SELECT' query statement.(Sql 'SELECT' 查询语句)</param>
    ''' <returns>The target data table.(目标数据表)</returns>
    ''' <remarks></remarks>
    Public Function Query(SQL As String) As List(Of Schema)
        Dim MySql As MySqlConnection = New MySqlConnection(Me.MySQL.UriMySQL.GetConnectionString) '[ConnectionString] is a compiled mysql connection string from our class constructor.
        Dim MySqlCommand As MySqlCommand = New MySqlCommand(SQL, MySql)
        Dim Reader As MySqlDataReader = Nothing
        Dim NewTable As New List(Of Schema)

        Try
            MySql.Open()
            Reader = MySqlCommand.ExecuteReader(CommandBehavior.CloseConnection)

            Dim Ordinals = (From Field As Field In Me.sql.TableSchema.Fields 'This table schema is created from the previous reflection operations
                            Let Ordinal As Integer = Reader.GetOrdinal(Field.FieldName)
                            Where Ordinal >= 0
                            Select Ordinal, Field, TypeCast = Function(value As Object) Field.DataType.TypeCasting(value)).ToArray

            While Reader.Read 'When we call this function, the pointer will move to next line in the table automatically.  
                Dim FillObject = Activator.CreateInstance(Me.sql.TableSchema.SchemaType) 'Create a instance of specific type: our record schema. 

                For Each Field In Ordinals  'Scan all of the fields in the field list and get the field value.
                    Call Field.Field.PropertyInfo.SetValue(FillObject, Field.TypeCast(Reader.GetValue(Field.Ordinal)), Nothing)
                Next

                NewTable.Add(FillObject)
            End While
            Return NewTable 'Return the new table that we get
        Catch ex As Exception
            Me.ErrorMessage = ex.ToString
            Call Console.WriteLine(ex.ToString)
        Finally
            If Not Reader Is Nothing Then Reader.Close()
            If Not MySqlCommand Is Nothing Then MySqlCommand.Dispose()
            If Not MySql Is Nothing Then MySql.Dispose()
        End Try
        Return Nothing
    End Function

    ''' <summary>
    ''' Load the data from database server. Please notice that: every time you call this function, the transaction will be commit to the database server in.
    ''' (从数据库服务器之中加载数据，请注意：每一次加载数据都会将先前的所积累下来的事务提交至数据库服务器之上)
    ''' </summary>
    ''' <param name="Count">
    ''' The count of the record that will be read from the server. Notice: Zero or negative is stands for 
    ''' load all records in the database.
    ''' (从数据库中读取的记录数目。请注意：值0和负数值都表示加载数据库的表中的所有数据)
    ''' </param>
    ''' <remarks></remarks>
    Public Sub Fetch(Optional Count As Integer = -1)
        Call Me.Commit()  '

        If Count <= 0 Then  'Load all data when count is zero or negative.
            _listData = Query($"SELECT * FROM {sql.TableName};")
            p = _listData.Count
        Else
            Dim NewData As List(Of Schema)
            NewData = Query($"SELECT * FROM {sql.TableName} LIMIT {p},{Count};")
            _listData.AddRange(NewData)
            p += Count  'pointer move next block.
        End If
    End Sub

    ''' <summary>
    ''' Commit the transaction to the database server to make the change permanently.
    ''' (将事务集提交至数据库服务器之上以永久的修改数据库) 
    ''' </summary>
    ''' <returns>The transaction commit is successfully or not.(事务集是否被成功提交)</returns>
    ''' <remarks></remarks>
    Public Function Commit() As Boolean
        If Transaction.Length = 0 Then Return True 'No transaction will be commit to database server.

        Dim ex As Exception = Nothing

        If Not MySQL.CommitTransaction(Transaction.ToString, ex) Then 'the transaction commit failure.
            ErrorMessage = ex.ToString
            Return False
        End If
        Call Transaction.Clear()  'Clean the previous transaction when the transaction commit is successfully. 
        Return True
    End Function

    ''' <summary>
    ''' Convert the mapping object to a dataset
    ''' </summary>
    ''' <param name="schema"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Narrowing Operator CType(schema As QueryHandler(Of Schema)) As List(Of Schema)
        Return schema.ListData
    End Operator

    ''' <summary>
    ''' Initialize the mapping from a connection object
    ''' </summary>
    ''' <param name="uri"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(uri As ConnectionUri) As QueryHandler(Of Schema)
        Return New QueryHandler(Of Schema) With {.MySQL = uri}
    End Operator

    ''' <summary>
    ''' Initialize the mapping from a connection string
    ''' </summary>
    ''' <param name="uri"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(uri As String) As QueryHandler(Of Schema)
        Return New QueryHandler(Of Schema) With {.MySQL = uri}
    End Operator

    ''' <summary>
    ''' Initialize the mapping from a connection string
    ''' </summary>
    ''' <param name="xml"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Widening Operator CType(xml As XElement) As QueryHandler(Of Schema)
        Return New QueryHandler(Of Schema) With {.MySQL = CType(xml, ConnectionUri)}
    End Operator

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call Me.Commit()
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

    Public Event ThrowException(ex As Exception, SQL As String)

    Private Sub MySQL_ThrowException(ex As Exception, SQL As String) Handles MySQL.ThrowException
        RaiseEvent ThrowException(ex, SQL)
    End Sub
End Class
