Imports System.Threading
Imports Microsoft.VisualBasic.ValueTypes
Imports Oracle.LinuxCompatibility.MySQL

Public Class Logger : Implements IDisposable

    ReadOnly counter As Counter
    ReadOnly Bytes_received As New List(Of Double)
    ReadOnly Bytes_sent As New List(Of Double)
    ReadOnly selects As New List(Of Double)

    ''' <summary>
    ''' unix timestamp
    ''' </summary>
    ReadOnly timestamp As New List(Of Double)
    ''' <summary>
    ''' sampling resolution, data in time unit seconds
    ''' </summary>
    ReadOnly resolution As Double = 1

    Private disposedValue As Boolean

    Sub New(mysql As MySqli, Optional resolution As Double = 1)
        Me.counter = New Counter(mysql)
        Me.resolution = resolution * 1000
    End Sub

    Public Sub Run()
        Do While App.Running AndAlso Not disposedValue
            Call pullData()
        Loop
    End Sub

    Private Sub pullData()
        Call counter.PullNext()
        Call Bytes_received.Add(counter.Bytes_received)
        Call Bytes_sent.Add(counter.Bytes_sent)
        Call selects.Add(counter.NumOfSelects)
        Call timestamp.Add(Now.UnixTimeStamp)
        Call VBDebugger.EchoLine(ToString)
        Call Thread.Sleep(resolution)
    End Sub

    Public Function Run(timespan As TimeSpan) As Logger
        Dim t0 = Now

        Do While (Now - t0) < timespan AndAlso App.Running AndAlso Not disposedValue
            Call pullData()
        Loop

        Return Me
    End Function

    Public Function GetLogging() As Dictionary(Of String, Double())
        Return New Dictionary(Of String, Double()) From {
            {NameOf(Bytes_received), Bytes_received.ToArray},
            {NameOf(Bytes_sent), Bytes_sent.ToArray},
            {NameOf(selects), selects.ToArray},
            {NameOf(timestamp), timestamp.ToArray}
        }
    End Function

    Public Overrides Function ToString() As String
        If timestamp.Count = 0 Then
            Return "<empty>"
        Else
            Return $"[{timestamp.Last}] Bytes_received:{StringFormats.Lanudry(Bytes_received.Last)}/sec Bytes_sent:{StringFormats.Lanudry(Bytes_sent.Last)}/sec SQL: {CInt(selects.Last)} SELECT"
        End If
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
            End If

            ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
            ' TODO: 将大型字段设置为 null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
    ' Protected Overrides Sub Finalize()
    '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
