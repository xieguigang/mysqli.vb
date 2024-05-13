#Region "Microsoft.VisualBasic::b6c014a61c71d2023b588cbdf2e1669e, src\mysqli\PerformanceCounter\Logger.vb"

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

    '   Total Lines: 135
    '    Code Lines: 97
    ' Comment Lines: 18
    '   Blank Lines: 20
    '     File Size: 5.61 KB


    ' Class Logger
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetGlobalStatus, GetLogging, Run, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, pullData, Run
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ValueTypes
Imports Oracle.LinuxCompatibility.MySQL

Public Class Logger : Implements IDisposable

    ReadOnly counter As Counter

    ' ------------------------- log data ----------------------------
    ReadOnly Bytes_received As New List(Of Double)
    ReadOnly Bytes_sent As New List(Of Double)
    ReadOnly Selects As New List(Of Double)
    ReadOnly Inserts As New List(Of Double)
    ReadOnly Deletes As New List(Of Double)
    ReadOnly Updates As New List(Of Double)
    ReadOnly Client_connections As New List(Of Double)
    ReadOnly Innodb_buffer_pool_read_requests As New List(Of Double)
    ReadOnly Innodb_buffer_pool_write_requests As New List(Of Double)
    ReadOnly Innodb_data_read As New List(Of Double)

    ReadOnly global_status As New List(Of Dictionary(Of String, String))
    ' ---------------------- end of log data ------------------------

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
        Call Selects.Add(counter.NumOfSelect)
        Call Inserts.Add(counter.NumOfInsert)
        Call Deletes.Add(counter.NumOfDelete)
        Call Updates.Add(counter.NumOfUpdate)
        Call Client_connections.Add(counter.ClientConnections)
        Call timestamp.Add(Now.UnixTimeStamp)
        Call Innodb_buffer_pool_read_requests.Add(counter.Innodb_buffer_pool_read_requests)
        Call Innodb_buffer_pool_write_requests.Add(counter.Innodb_buffer_pool_write_requests)
        Call global_status.Add(counter.global_status)
        Call Innodb_data_read.Add(counter.Innodb_data_read)
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

    Public Function GetGlobalStatus() As Dictionary(Of String, String)()
        Return global_status.ToArray
    End Function

    Public Function GetLogging() As Dictionary(Of String, Double())
        Return New Dictionary(Of String, Double()) From {
            {NameOf(Bytes_received), Bytes_received.ToArray},
            {NameOf(Bytes_sent), Bytes_sent.ToArray},
            {NameOf(Selects), Selects.ToArray},
            {NameOf(Inserts), Inserts.ToArray},
            {NameOf(Deletes), Deletes.ToArray},
            {NameOf(Updates), Updates.ToArray},
            {NameOf(Innodb_buffer_pool_read_requests), Innodb_buffer_pool_read_requests.ToArray},
            {NameOf(Innodb_buffer_pool_write_requests), Innodb_buffer_pool_write_requests.ToArray},
            {NameOf(Innodb_data_read), Innodb_data_read.ToArray},
            {NameOf(Client_connections), Client_connections.ToArray},
            {NameOf(timestamp), timestamp.ToArray}
        }
    End Function

    Public Overrides Function ToString() As String
        If timestamp.Count = 0 Then
            Return "<empty>"
        Else
            Dim counter As String() = {
                $"Bytes_received:{StringFormats.Lanudry(Bytes_received.Last)}/s",
                $"Bytes_sent:{StringFormats.Lanudry(Bytes_sent.Last)}/s",
                $"Client_connections: {CInt(Client_connections.Last)}",
                $"SQL: {CInt(Selects.Last)} SELECT {CInt(Inserts.Last)} INSERT {CInt(Updates.Last)} UPDATE {CInt(Deletes.Last)} DELETE",
                $"Innodb_buffer_pool: {StringFormats.nsize(Innodb_buffer_pool_read_requests.Last)} pages/s read_requests {StringFormats.nsize(Innodb_buffer_pool_write_requests.Last)} pages/s write_requests",
                $"Innodb_data_read: {StringFormats.Lanudry(Innodb_data_read.Last)}/s"
            }

            Return $"[{timestamp.Last}] {counter.JoinBy("; ")}"
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

