Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace MySqlBuilder

    Public Class Model

        ReadOnly conn As ConnectionUri
        ReadOnly schema As Table

        Sub New(conn As ConnectionUri)
            Me.conn = conn
        End Sub

        Private Sub inspectSchema()

        End Sub
    End Class
End Namespace