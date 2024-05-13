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

    Public ReadOnly Property NumOfSelects As Double
        Get
            Return (current.Selects - previous.Selects) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property ClientConnections As Integer
        Get
            Return current.Threads_connected
        End Get
    End Property

    Sub New(mysql As MySqli)
        Me.mysql = mysql
        Me.previous = GLOBAL_STATUS.Load(mysql)
        Me.current = Me.previous
    End Sub

    Public Function PullNext() As Counter
        previous = current
        current = GLOBAL_STATUS.Load(mysql)
        Return Me
    End Function

End Class
