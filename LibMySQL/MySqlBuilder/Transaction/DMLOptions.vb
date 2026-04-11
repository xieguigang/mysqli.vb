Imports System.ComponentModel

Namespace MySqlBuilder

    ''' <summary>
    '''  Data manipulation options for delete/update/insert
    ''' </summary>
    Public Enum DMLOptions
        <Description("")> None
        <Description("DELAYED")> Delayed
        <Description("IGNORE")> Ignore
    End Enum
End Namespace