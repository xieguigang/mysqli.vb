Imports Oracle.LinuxCompatibility.MySQL

Public Class Counter

    Dim previous As GLOBAL_STATUS
    Dim current As GLOBAL_STATUS

    ReadOnly mysql As MySqli

    Public ReadOnly Property deltaTime As TimeSpan
        Get
            Return current.time - previous.time
        End Get
    End Property

    Public ReadOnly Property global_status As Dictionary(Of String, String)
        Get
            Return current.global_status
        End Get
    End Property

    ''' <summary>
    ''' Bytes_received per seconds
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Bytes_received As Double
        Get
            Return (current.Bytes_received - previous.Bytes_received) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property Bytes_sent As Double
        Get
            Return (current.Bytes_sent - previous.Bytes_sent) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfSelect As Double
        Get
            Return (current.Selects - previous.Selects) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfInsert As Double
        Get
            Return (current.Inserts - previous.Inserts) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfUpdate As Double
        Get
            Return (current.Updates - previous.Updates) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfDelete As Double
        Get
            Return (current.Deletes - previous.Deletes) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property ClientConnections As Integer
        Get
            Return current.Threads_connected
        End Get
    End Property

    Public ReadOnly Property Innodb_buffer_pool_read_requests As Double
        Get
            Return (current.Innodb_buffer_pool_read_requests - previous.Innodb_buffer_pool_read_requests) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property Innodb_buffer_pool_write_requests As Double
        Get
            Return (current.Innodb_buffer_pool_write_requests - previous.Innodb_buffer_pool_write_requests) / deltaTime.TotalSeconds
        End Get
    End Property

    Sub New(mysql As MySqli)
        Me.mysql = mysql
        Me.previous = PerformanceCounter.GLOBAL_STATUS.Load(mysql)
        Me.current = Me.previous
    End Sub

    Public Function PullNext() As Counter
        previous = current
        current = PerformanceCounter.GLOBAL_STATUS.Load(mysql)
        Return Me
    End Function

End Class
