Imports Oracle.LinuxCompatibility.MySQL

''' <summary>
''' Reads MySQL server global variables that are needed for the monitor but that
''' are not part of the delta-based GLOBAL STATUS counter model. These values are
''' relatively static (configuration) so they are cached after the first read.
''' </summary>
Public Class VariablesReader

    ReadOnly mysql As MySqli

    Private _innodbBufferPoolSize As Long = -1
    Private _longQueryTime As Double = -1
    Private _pidFile As String = ""

    Sub New(mysql As MySqli)
        Me.mysql = mysql
    End Sub

    ''' <summary>
    ''' Total size of the InnoDB buffer pool in bytes (innodb_buffer_pool_size).
    ''' </summary>
    ''' <returns>buffer pool size in bytes, or 0 if it could not be read</returns>
    Public Function GetInnodbBufferPoolSize() As Long
        If _innodbBufferPoolSize < 0 Then
            _innodbBufferPoolSize = CLng(ReadULong("innodb_buffer_pool_size"))
        End If
        Return _innodbBufferPoolSize
    End Function

    ''' <summary>
    ''' The configured slow query threshold in seconds (long_query_time).
    ''' </summary>
    ''' <returns>threshold in seconds, or 10 if it could not be read</returns>
    Public Function GetLongQueryTime() As Double
        If _longQueryTime < 0 Then
            Dim raw = ReadString("long_query_time")
            If Not Double.TryParse(raw, _longQueryTime) Then
                _longQueryTime = 10
            End If
        End If
        Return _longQueryTime
    End Function

    ''' <summary>
    ''' The path of the mysqld process id file (pid_file). Used to locate
    ''' the /proc/&lt;pid&gt; directory for system resource monitoring.
    ''' </summary>
    ''' <returns>pid_file path, or empty string if it could not be read</returns>
    Public Function GetPidFile() As String
        If _pidFile = "" Then
            _pidFile = ReadString("pid_file")
        End If
        Return _pidFile
    End Function

    Private Function ReadString(variable As String) As String
        Using reader As MySqlDataReader = mysql.ExecuteDataset($"SHOW GLOBAL VARIABLES LIKE '{variable}';")
            If reader.Read Then
                Return reader.GetString("Value")
            End If
        End Using
        Return ""
    End Function

    Private Function ReadULong(variable As String) As ULong
        Dim raw = ReadString(variable)
        Dim value As ULong
        If ULong.TryParse(raw, value) Then
            Return value
        End If
        Return 0
    End Function

End Class
