''' <summary>
''' Information about a single currently executing slow query captured from
''' ``SHOW PROCESSLIST``.
''' </summary>
Public Class SlowQueryInfo

    ''' <summary>Connection / process id of the query.</summary>
    Public Property Id As ULong

    ''' <summary>User that runs the query.</summary>
    Public Property User As String

    ''' <summary>Client host (host:port) of the connection.</summary>
    Public Property Host As String

    ''' <summary>Default database of the connection (may be empty).</summary>
    Public Property Database As String

    ''' <summary>Current execution state (e.g. "Sending data", "Sorting result").</summary>
    Public Property State As String

    ''' <summary>How long the query has been running, in seconds.</summary>
    Public Property TimeSec As Double

    ''' <summary>The SQL text being executed.</summary>
    Public Property Sql As String

    Public Overrides Function ToString() As String
        Return $"[{Id}] {User}@{Host} ({Database}) {TimeSec}s - {Sql}"
    End Function

End Class
