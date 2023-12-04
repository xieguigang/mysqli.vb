Imports Oracle.LinuxCompatibility.MySQL.Uri

Public MustInherit Class IDatabase

    Protected mysqli As ConnectionUri

    Public Sub New(mysqli As ConnectionUri)
        Me.mysqli = mysqli
    End Sub

End Class
