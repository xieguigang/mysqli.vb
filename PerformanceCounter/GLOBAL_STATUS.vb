Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports MySql.Data.MySqlClient
Imports Oracle.LinuxCompatibility.MySQL

''' <summary>
''' ``SHOW GLOBAL STATUS;``
''' </summary>
Public Class GLOBAL_STATUS

    ReadOnly values As StringReader

    Sub New(data As Dictionary(Of String, String))
        values = StringReader.WrapDictionary(data)
    End Sub

    Public Shared Function Load(mysql As MySqli) As GLOBAL_STATUS
        Dim pull As MySqlDataReader = mysql.ExecuteDataset("SHOW GLOBAL STATUS;")
        Dim data As New Dictionary(Of String, String)

        Do While pull.Read
            Call data.Add(
                pull.GetString("Variable_name"),
                pull.GetString("Value"))
        Loop

        Return New GLOBAL_STATUS(data)
    End Function
End Class