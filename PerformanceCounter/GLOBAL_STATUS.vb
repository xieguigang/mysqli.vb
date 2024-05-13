Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports MySql.Data.MySqlClient
Imports Oracle.LinuxCompatibility.MySQL

''' <summary>
''' ``SHOW GLOBAL STATUS;``
''' </summary>
Public Class GLOBAL_STATUS

    ReadOnly values As StringReader

    ''' <summary>
    ''' the source data that pull from the mysql server
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property global_status As Dictionary(Of String, String)
    Public ReadOnly Property time As Date = Now

    Public ReadOnly Property Bytes_received As Double
        Get
            Return values.GetDouble("Bytes_received")
        End Get
    End Property

    Public ReadOnly Property Bytes_sent As Double
        Get
            Return values.GetDouble("Bytes_sent")
        End Get
    End Property

    Public ReadOnly Property Selects As ULong
        Get
            Return values.GetUInt64("Com_select")
        End Get
    End Property

    Public ReadOnly Property Inserts As ULong
        Get
            Return values.GetUInt64("Com_insert")
        End Get
    End Property

    Public ReadOnly Property Updates As ULong
        Get
            Return values.GetUInt64("Com_update")
        End Get
    End Property

    Public ReadOnly Property Deletes As ULong
        Get
            Return values.GetUInt64("Com_delete")
        End Get
    End Property

    ''' <summary>
    ''' number of client connection in current
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Threads_connected As Integer
        Get
            Return values.GetInt32("Threads_connected")
        End Get
    End Property

    Sub New(data As Dictionary(Of String, String))
        global_status = data
        values = StringReader.WrapDictionary(data)
    End Sub

    Public Shared Function Load(mysql As MySqli) As GLOBAL_STATUS
        ' 20240513
        '
        ' the mysql connection resource needs to be disposed automatically, or
        '
        ' MySql.Data.MySqlClient.MySqlException (0x80004005): error connecting: Timeout expired.
        ' The timeout period elapsed prior to obtaining a connection from the pool.
        ' This may have occurred because all pooled connections were in use and max pool size was reached.
        Using pull As MySqlDataReader = mysql.ExecuteDataset("SHOW GLOBAL STATUS;")
            Dim data As New Dictionary(Of String, String)

            Do While pull.Read
                Call data.Add(
                    pull.GetString("Variable_name"),
                    pull.GetString("Value"))
            Loop

            Return New GLOBAL_STATUS(data)
        End Using
    End Function
End Class