Imports System.Data.Common
Imports System.Text
Imports System.Data.Linq.Mapping
Imports System.Data.Entity.Core

''' <summary>
''' The API interface wrapper of the SQLite.(SQLite的存储引擎的接口)
''' </summary>
''' <remarks></remarks>
Public Class SQLProcedure : Implements System.IDisposable

    Dim URLConnection As DbConnection

    ''' <summary>
    ''' Get the filename of the connected SQLite database file.(返回数据库文件的文件位置)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property URL As String

    Protected Sub New()
    End Sub

    ''' <summary>
    ''' Create a table in current database file for the specific table schema <para>T</para> . 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateTableFor(Of T As Class)() As String
        Return CreateTableFor(TypeInfo:=GetType(T))
    End Function

    ''' <summary>
    ''' Create a table in current database file for the specific table schema <paramref name="TypeInfo"></paramref> . 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateTableFor(TypeInfo As System.Type) As String
        Dim TableSchema = New TableSchema(TypeInfo)
        Dim SQL As String = SchemaCache.CreateTableSQL(TableSchema.DatabaseFields, TableSchema.TableName)
        Call Me.Execute(SQL)

        Dim p As Integer = Me.Load(Of TableDump).Count + 1
        Dim TableSchemaDumpInfo = (From Field As SchemaCache
                                   In TableSchema
                                   Select New TableDump With {
                                       .Guid = p.MoveNext,
                                       .DbType = Field.DbType,
                                       .FieldName = Field.DbFieldName,
                                       .IsPrimaryKey = If(Field.FieldEntryPoint.IsPrimaryKey, 1, 0),
                                       .TableName = TableSchema.TableName}).ToArray '由于需要生成递增的Guid，故而这里不能再使用并行拓展了
        TableSchema = New TableSchema(GetType(TableDump))
        For Each item As TableDump In TableSchemaDumpInfo
            Call Me.Insert(TableSchema, item)
        Next

        Return SQL
    End Function

    ''' <summary>
    ''' Get a value to knows that wether the target table is exists in the database or not.
    ''' (判断某一个数据表是否存在)
    ''' </summary>
    ''' <param name="TableName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExistsTable(TableName As String) As Boolean
        Dim SQL As String = "SELECT COUNT(*) FROM sqlite_master where type='table' and name='" & TableName & "';"

        Using EXECommand As IDbCommand = Me.URLConnection.CreateCommand

            EXECommand.Connection = Me.URLConnection
            EXECommand.CommandText = SQL
            Return CType(EXECommand.ExecuteScalar, Integer) > 0
        End Using
    End Function

    ''' <summary>
    ''' Delete the target table.
    ''' </summary>
    ''' <param name="SchemaInfo"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DeleteTable(SchemaInfo As Type) As Boolean
        Dim TbName As String = Reflector.GetTableName(SchemaInfo)
        Call Execute("DROP TABLE '" & TbName & "';")
        Return True
    End Function

    ''' <summary>
    ''' Delete the target table.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DeleteTable(TableName As String) As Boolean
        Call Execute("DROP TABLE '" & TableName & "';")
        Return True
    End Function

    ''' <summary>
    ''' Get a value to knows that wether the target table is exists in the database or not.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExistsTable(Schema As Type) As Boolean
        Dim TbName As String = Reflector.GetTableName(Schema)
        Return ExistsTable(TbName)
    End Function

    Public Function ExistsTableForType(Of T As Class)() As Boolean
        Dim TbName As String = Reflector.GetTableName(Of T)()
        Return ExistsTable(TbName)
    End Function

    Const FILEIO_EXCEPTION As String = "Maybe we have a wrong file place for ""file:///{0}"", shuch as no sufficient privilege or a readonly place."

    ''' <summary>
    ''' Establishing the protocol of the SQLite connection between you program and the database file "<paramref name="url"></paramref>".
    ''' </summary>
    ''' <param name="url">The path of the SQLite database file.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateSQLTransaction(url As String) As SQLProcedure
        url = FileIO.FileSystem.GetFileInfo(url).FullName.Replace("\", "/")

        Try

            If Not FileIO.FileSystem.FileExists(url) Then
                Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(url))
                Call SQLiteConnection.CreateFile(databaseFileName:=url)
            End If
        Catch ex As Exception
            ex = New Exception(String.Format(FILEIO_EXCEPTION, url), ex)
            Throw ex
        End Try

        Dim URLConnection As DbConnection

        URLConnection = New SQLiteConnection(String.Format("Data Source=""{0}"";Pooling=true;FailIfMissing=false", url))
        URLConnection.Open()

        Dim DBI As SQLProcedure = New SQLProcedure With {._URL = url, .URLConnection = URLConnection}
        Dim DumpInfoSchema As Type = GetType(TableDump)

        If Not DBI.ExistsTable(DumpInfoSchema) Then
            Dim TableSchema = New TableSchema(DumpInfoSchema)
            Dim SQL As String = SchemaCache.CreateTableSQL(TableSchema.DatabaseFields, TableSchema.TableName)
            Call DBI.Execute(SQL)
        End If

        Return DBI
    End Function

    Public Overrides Function ToString() As String
        Return "file:///" & _URL
    End Function

    ''' <summary>
    ''' Batch execute a SQL collection as a SQL transaction. 
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ExecuteTransaction(SQL As String()) As Boolean
        Dim TrSQLBuilder As StringBuilder = New StringBuilder(1024)

        Call TrSQLBuilder.AppendLine("BEGIN IMMEDIATE")

        For Each Line As String In SQL
            Call TrSQLBuilder.AppendLine(Line)
        Next

        Call TrSQLBuilder.AppendLine("COMMIT")
        Call Execute(TrSQLBuilder.ToString)

        Return True
    End Function

    ''' <summary>
    ''' If the SQL is a SELECT statement, then this function returns a table object, if not then it returns nothing.
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Execute(SQL As String) As DbDataReader
        Using EXECommand As IDbCommand = Me.URLConnection.CreateCommand

            EXECommand.Connection = Me.URLConnection
            EXECommand.CommandText = SQL

            Try
                If InStr("insert, delete, update", SQL.Trim.Split.First.ToLower, CompareMethod.Text) > 0 Then
                    Dim i As Integer = EXECommand.ExecuteNonQuery()

                    If i = 0 Then
                        Throw New Exception("No data row was effected!")
                    End If
                ElseIf InStr("drop, create", SQL.Trim.Split.First.ToLower, CompareMethod.Text) > 0 Then
                    Call EXECommand.ExecuteNonQuery()
                Else
                    Return EXECommand.ExecuteReader()
                End If
            Catch ex As Exception
                Dim Err As String = String.Format(SQL_EXECUTE_ERROR, SQL, ex.ToString)
                Throw New EntitySqlException(Err)
            End Try
        End Using

        Return Nothing
    End Function

    Const SQL_EXECUTE_ERROR As String =
 _
        "Error occurred while trying to execute sql:  " & vbCrLf &
        "      -----> {0}" & vbCrLf & vbCrLf &
 _
        "Internal Exception Details:" & vbCrLf &
        "{1}"

    ''' <summary>
    ''' If the SQL is a SELECT statement, then this function returns a table object, if not then it returns nothing.
    ''' </summary>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Execute(SQL As String, ParamArray argvs As String()) As DbDataReader
        Return Me.Execute(String.Format(SQL, argvs))
    End Function

    Public Sub CloseTransaction()
        Call Me.URLConnection.Close()
        Call Me.URLConnection.Dispose()
    End Sub

    ''' <summary>
    ''' Export the data dump in the format of a INSERT INTO SQL statement for transfer the data in this database into another database.
    ''' </summary>
    ''' <typeparam name="Table">The table schema of the target table which will be transfer.</typeparam>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateSQLDump(Of Table As Class)() As String
        Dim SQLBuilder As StringBuilder = New StringBuilder(2048)
        Dim SchemaCache As SchemaCache() = Reflector.__getSchemaCache(Of Table)()
        Dim TableName As String = Reflector.GetTableName(Of Table)()
        Dim SQL As String = [Interface].SchemaCache.CreateTableSQL(SchemaCache, TableName)

        Call SQLBuilder.AppendLine("/* CREATE_TABLE_SCHEMA_INFORMATION */")
        Call SQLBuilder.AppendLine(SQL)
        Call SQLBuilder.AppendLine()
        Call SQLBuilder.AppendLine("/* DATA_STORAGES */")

        Dim LQuery As String() = (From ItemRowObject As Table
                                  In Me.Load(Of Table)()
                                  Select [Interface].SchemaCache.CreateInsertSQL(Of Table)(SchemaCache, ItemRowObject, TableName)).ToArray

        For Each Line As String In LQuery
            Call SQLBuilder.AppendLine(Line)
        Next

        Call SQLBuilder.AppendLine()
        Call SQLBuilder.AppendLine("/* END_OF_SQL_DUMP */")

        Return SQLBuilder.ToString
    End Function

    ''' <summary>
    ''' 转储整个数据库
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DbDump(DumpFile As String) As Boolean
        Dim Tables = (From item In Me.Load(Of TableDump)() Select item Group By item.TableName Into Group).ToArray

        Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(DumpFile))

        For Each Table In Tables
            Dim SQLDump As String = ___SQLDump(Table.Group.ToArray)
            Call FileIO.FileSystem.WriteAllText(DumpFile, SQLDump, append:=True)
        Next

        Return True
    End Function

    ''' <summary>
    ''' Export the data dump in the format of a INSERT INTO SQL statement for transfer the data in this database into another database.
    ''' </summary>
    ''' <param name="Table">The table schema of the target table which will be transfer.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ___SQLDump(Table As TableDump()) As String
        Dim SQLBuilder As StringBuilder = New StringBuilder(2048)
        Dim TableName As String = Table.First.TableName
        Dim SQL As String = [Interface].SchemaCache.CreateTableSQL(Table)

        Call SQLBuilder.AppendLine("/* CREATE_TABLE_SCHEMA_INFORMATION */")
        Call SQLBuilder.AppendLine(SQL)
        Call SQLBuilder.AppendLine()
        Call SQLBuilder.AppendLine(String.Format("/* DATA_STORAGES  ""{0}"" */", TableName))

        Dim DbReader = Me.Execute("SELECT * FROM '{0}';", TableName)
        Dim SchemaCache = (From item In Table Select Field = item, p = DbReader.GetOrdinal(item.FieldName)).ToArray

        Do While DbReader.Read
            Dim Values As String = String.Join(", ", (From p In SchemaCache
                                                      Let value As Object = DbReader.GetValue(p.p)
                                                      Let s As String = $"'{Scripting.ToString(value)}'"
                                                      Select s).ToArray)
            Dim Columns As String = String.Join(", ", (From p In SchemaCache Select p.Field.FieldName).ToArray)
            Dim InsertSQL As String = String.Format("INSERT INTO '{0}' ({1}) VALUES ({2}) ;", TableName, Columns, Values)
            Call SQLBuilder.AppendLine(InsertSQL)
        Loop

        Call SQLBuilder.AppendLine()
        Call SQLBuilder.AppendLine(String.Format("/* END_OF_SQL_DUMP  ""{0}"" */", TableName))

        Return SQLBuilder.ToString
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call Me.CloseTransaction()
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