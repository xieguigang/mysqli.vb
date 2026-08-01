
''' <summary>
''' A single snapshot of the mysqld process resources.
''' </summary>
Public Class ProcessSnapshot

    ''' <summary>CPU usage in percent [0, 100+].</summary>
    Public Property CpuPercent As Double = 0

    ''' <summary>Resident memory size in bytes (VmRSS).</summary>
    Public Property MemoryBytes As Long = 0

    ''' <summary>Number of threads in the process.</summary>
    Public Property ThreadCount As Integer = 0

    ''' <summary>Whether the /proc interface for the pid was readable.</summary>
    Public Property Available As Boolean = False

    ''' <summary>Human readable note, e.g. "N/A" reason when not available.</summary>
    Public Property Note As String = ""

End Class