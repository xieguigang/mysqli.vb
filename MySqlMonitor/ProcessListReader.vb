Imports Oracle.LinuxCompatibility.MySQL

''' <summary>
''' Reads the current list of running queries from ``SHOW PROCESSLIST`` and
''' identifies the slow queries (queries whose execution time exceeds the
''' configured long_query_time threshold and that are not in the Sleep state).
''' </summary>
Public Class ProcessListReader

    ReadOnly mysql As MySqli

    Sub New(mysql As MySqli)
        Me.mysql = mysql
    End Sub

    ''' <summary>
    ''' Pull the list of currently executing slow queries.
    ''' </summary>
    ''' <param name="longQueryThreshold">slow query threshold in seconds</param>
    ''' <param name="maxRows">maximum number of slow queries to return (most recent first)</param>
    ''' <returns></returns>
    Public Function GetSlowQueries(Optional longQueryThreshold As Double = 10,
                                   Optional maxRows As Integer = 20) As List(Of SlowQueryInfo)
        Dim result As New List(Of SlowQueryInfo)

        Using reader As MySqlDataReader = mysql.ExecuteDataset("SHOW PROCESSLIST;")
            Do While reader.Read
                Dim command As String = SafeGetString(reader, "Command")
                Dim info As String = SafeGetString(reader, "Info")

                ' Skip idle connections and empty statements
                If command = "Sleep" OrElse String.IsNullOrWhiteSpace(info) Then
                    Continue Do
                End If

                Dim timeSec As Double = SafeGetDouble(reader, "Time")

                If timeSec >= longQueryThreshold Then
                    result.Add(New SlowQueryInfo With {
                        .Id = SafeGetULong(reader, "Id"),
                        .User = SafeGetString(reader, "User"),
                        .Host = SafeGetString(reader, "Host"),
                        .Database = SafeGetString(reader, "db"),
                        .State = SafeGetString(reader, "State"),
                        .TimeSec = timeSec,
                        .Sql = info
                    })
                End If
            Loop
        End Using

        ' Sort by execution time descending so the most expensive queries show first
        result.Sort(Function(a, b) b.TimeSec.CompareTo(a.TimeSec))

        If result.Count > maxRows Then
            result.RemoveRange(maxRows, result.Count - maxRows)
        End If

        Return result
    End Function

    Private Shared Function SafeGetString(reader As MySqlDataReader, column As String) As String
        Try
            Dim ordinal = reader.GetOrdinal(column)
            If reader.IsDBNull(ordinal) Then
                Return ""
            End If
            Return reader.GetString(ordinal)
        Catch
            Return ""
        End Try
    End Function

    Private Shared Function SafeGetDouble(reader As MySqlDataReader, column As String) As Double
        Try
            Dim ordinal = reader.GetOrdinal(column)
            If reader.IsDBNull(ordinal) Then
                Return 0
            End If
            Return reader.GetDouble(ordinal)
        Catch
            Return 0
        End Try
    End Function

    Private Shared Function SafeGetULong(reader As MySqlDataReader, column As String) As ULong
        Try
            Dim ordinal = reader.GetOrdinal(column)
            If reader.IsDBNull(ordinal) Then
                Return 0
            End If
            Return reader.GetUInt64(ordinal)
        Catch
            Return 0
        End Try
    End Function

End Class
