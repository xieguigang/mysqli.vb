#Region "Microsoft.VisualBasic::3bce5b118c5b8dacdf091f220dbcf96e, src\mysqli\PerformanceCounter\GLOBAL_STATUS.vb"

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

    '   Total Lines: 108
    '    Code Lines: 73
    ' Comment Lines: 18
    '   Blank Lines: 17
    '     File Size: 3.31 KB


    ' Class GLOBAL_STATUS
    ' 
    '     Properties: Bytes_received, Bytes_sent, Deletes, global_status, Innodb_buffer_pool_read_requests
    '                 Innodb_buffer_pool_write_requests, Innodb_data_read, Inserts, Selects, Threads_connected
    '                 time, Updates
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Load
    ' 
    ' /********************************************************************************/

#End Region

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

    Public ReadOnly Property Innodb_buffer_pool_read_requests As ULong
        Get
            Return values.GetUInt64("Innodb_buffer_pool_read_requests")
        End Get
    End Property

    Public ReadOnly Property Innodb_buffer_pool_write_requests As ULong
        Get
            Return values.GetUInt64("Innodb_buffer_pool_write_requests")
        End Get
    End Property

    Public ReadOnly Property Innodb_data_read As ULong
        Get
            Return values.GetUInt64("Innodb_data_read")
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
