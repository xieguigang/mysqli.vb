#Region "Microsoft.VisualBasic::0874cb978f99815fa4e0b538e18c84cf, src\mysqli\PerformanceCounter\Counter.vb"

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

    '   Total Lines: 98
    '    Code Lines: 76
    ' Comment Lines: 4
    '   Blank Lines: 18
    '     File Size: 2.95 KB


    ' Class Counter
    ' 
    '     Properties: Bytes_received, Bytes_sent, ClientConnections, deltaTime, global_status
    '                 Innodb_buffer_pool_read_requests, Innodb_buffer_pool_write_requests, Innodb_data_read, NumOfDelete, NumOfInsert
    '                 NumOfSelect, NumOfUpdate
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: PullNext
    ' 
    ' /********************************************************************************/

#End Region

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

    Public ReadOnly Property NumOfCreate As Double
        Get
            Return (current.Creates - previous.Creates) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfAlter As Double
        Get
            Return (current.Alters - previous.Alters) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property NumOfDrop As Double
        Get
            Return (current.Drops - previous.Drops) / deltaTime.TotalSeconds
        End Get
    End Property

    ''' <summary>
    ''' Slow queries per second (delta of Slow_queries / delta time).
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NumOfSlow As Double
        Get
            Return (current.Slow_queries - previous.Slow_queries) / deltaTime.TotalSeconds
        End Get
    End Property

    Public ReadOnly Property ClientConnections As Integer
        Get
            Return current.Threads_connected
        End Get
    End Property

    ''' <summary>
    ''' The number of threads that are not sleeping (Threads_running), i.e. active queries.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ThreadsRunning As Integer
        Get
            Return current.Threads_running
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

    Public ReadOnly Property Innodb_data_read As Double
        Get
            Return (current.Innodb_data_read - previous.Innodb_data_read) / deltaTime.TotalSeconds
        End Get
    End Property

    ''' <summary>
    ''' InnoDB data bytes written per second (Innodb_data_written delta / delta time).
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Innodb_data_written As Double
        Get
            Return (current.Innodb_data_written - previous.Innodb_data_written) / deltaTime.TotalSeconds
        End Get
    End Property

    ''' <summary>
    ''' Number of times a page had to be read from disk because it was not in the buffer pool,
    ''' per second (Innodb_buffer_pool_reads delta / delta time).
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Innodb_buffer_pool_disk_reads As Double
        Get
            Return (current.Innodb_buffer_pool_reads - previous.Innodb_buffer_pool_reads) / deltaTime.TotalSeconds
        End Get
    End Property

    ''' <summary>
    ''' Buffer pool hit rate in range [0, 1]. Computed from the cumulative read requests
    ''' versus physical disk reads (read requests served from memory / total read requests).
    ''' Returns 1.0 when there are no read requests yet.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BufferPoolHitRate As Double
        Get
            Dim total = current.Innodb_buffer_pool_read_requests
            If total <= 0 Then
                Return 1.0
            End If
            Dim fromMem = total - current.Innodb_buffer_pool_reads
            Return Math.Min(1.0, fromMem / CDbl(total))
        End Get
    End Property

    ''' <summary>
    ''' Buffer pool usage ratio in range [0, 1], i.e. the fraction of the pool that is occupied
    ''' by data pages: Innodb_buffer_pool_pages_data / Innodb_buffer_pool_pages_total.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property BufferPoolUsage As Double
        Get
            Dim total = current.Innodb_buffer_pool_pages_total
            If total <= 0 Then
                Return 0.0
            End If
            Return Math.Min(1.0, CDbl(current.Innodb_buffer_pool_pages_data) / CDbl(total))
        End Get
    End Property

    ''' <summary>
    ''' Number of times a page request had to wait because no clean page was available, per second.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Innodb_buffer_pool_wait_free_rate As Double
        Get
            Return (current.Innodb_buffer_pool_wait_free - previous.Innodb_buffer_pool_wait_free) / deltaTime.TotalSeconds
        End Get
    End Property

    ''' <summary>
    ''' Temporary tables created on disk instead of in memory, per second
    ''' (Created_tmp_disk_tables delta / delta time).
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CreatedTmpDiskTablesRate As Double
        Get
            Return (current.Created_tmp_disk_tables - previous.Created_tmp_disk_tables) / deltaTime.TotalSeconds
        End Get
    End Property

    Sub New(mysql As MySqli)
        Me.mysql = mysql
        Me.previous = PerformanceCounter.GLOBAL_STATUS.Load(mysql)
        Me.current = Me.previous
    End Sub

    ''' <summary>
    ''' Pull the new statues data from mysql server
    ''' </summary>
    ''' <returns>returns the current counter object itself.</returns>
    Public Function PullNext() As Counter
        previous = current
        current = PerformanceCounter.GLOBAL_STATUS.Load(mysql)
        Return Me
    End Function

End Class

