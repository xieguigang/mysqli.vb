Imports Oracle.LinuxCompatibility.LibMySQL.PerformanceCounter

' Rolling history buffers for the dashboard sparklines. Each series keeps at most
' `Capacity` most-recent samples; once full it drops the oldest (FIFO via Queue).
' Only numeric rate metrics are tracked; the capacity is fixed so memory stays bounded.

Public Class MetricHistory
    Public Const Capacity As Integer = 40
    Private selectQ As New Queue(Of Double)()
    Private insertQ As New Queue(Of Double)()
    Private updateQ As New Queue(Of Double)()
    Private deleteQ As New Queue(Of Double)()
    Private createQ As New Queue(Of Double)()
    Private alterQ As New Queue(Of Double)()
    Private dropQ As New Queue(Of Double)()
    Private readKbQ As New Queue(Of Double)()
    Private writeKbQ As New Queue(Of Double)()
    Private recvKbQ As New Queue(Of Double)()
    Private sentKbQ As New Queue(Of Double)()
    Private hitQ As New Queue(Of Double)()
    Private usageQ As New Queue(Of Double)()
    Private connsQ As New Queue(Of Double)()

    Private Sub Push(q As Queue(Of Double), v As Double)
        q.Enqueue(v)
        While q.Count > Capacity
            q.Dequeue()
        End While
    End Sub

    Public Sub Sample(c As Counter)
        Push(selectQ, c.NumOfSelect)
        Push(insertQ, c.NumOfInsert)
        Push(updateQ, c.NumOfUpdate)
        Push(deleteQ, c.NumOfDelete)
        Push(createQ, c.NumOfCreate)
        Push(alterQ, c.NumOfAlter)
        Push(dropQ, c.NumOfDrop)
        Push(readKbQ, c.Innodb_data_read)
        Push(writeKbQ, c.Innodb_data_written)
        Push(recvKbQ, c.Bytes_received)
        Push(sentKbQ, c.Bytes_sent)
        Push(hitQ, c.BufferPoolHitRate)
        Push(usageQ, c.BufferPoolUsage)
        Push(connsQ, c.ClientConnections)
    End Sub

    Public ReadOnly Property SelectSeries() As Double()
        Get
            Return selectQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property InsertSeries() As Double()
        Get
            Return insertQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property UpdateSeries() As Double()
        Get
            Return updateQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property DeleteSeries() As Double()
        Get
            Return deleteQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property CreateSeries() As Double()
        Get
            Return createQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property AlterSeries() As Double()
        Get
            Return alterQ.ToArray()
        End Get
    End Property

    Public ReadOnly Property DropSeries() As Double()
        Get
            Return dropQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property ReadKbSeries() As Double()
        Get
            Return readKbQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property WriteKbSeries() As Double()
        Get
            Return writeKbQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property RecvKbSeries() As Double()
        Get
            Return recvKbQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property SentKbSeries() As Double()
        Get
            Return sentKbQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property HitSeries() As Double()
        Get
            Return hitQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property UsageSeries() As Double()
        Get
            Return usageQ.ToArray()
        End Get
    End Property
    Public ReadOnly Property ConnsSeries() As Double()
        Get
            Return connsQ.ToArray()
        End Get
    End Property
End Class