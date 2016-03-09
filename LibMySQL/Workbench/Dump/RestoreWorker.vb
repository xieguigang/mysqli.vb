Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Workbench.Dump

    Public Class RestoreWorker

        Public ReadOnly Property MySQL As MySQL

        Sub New(uri As Oracle.LinuxCompatibility.MySQL.ConnectionUri)
            MySQL = New MySQL
            Call MySQL.Connect(uri)
        End Sub

        ''' <summary>
        ''' 会需要动态编译
        ''' </summary>
        ''' <param name="dumpDir"></param>
        ''' <returns></returns>
        Public Function ImportsData(dumpDir As String, Optional dbName As String = "") As Boolean
            Dim SQLs As String() = FileIO.FileSystem.GetFiles(
                dumpDir,
                FileIO.SearchOption.SearchTopLevelOnly,
                "*.sql").ToArray(Function(file) FileIO.FileSystem.ReadAllText(file))

            If String.IsNullOrEmpty(dbName) Then
                dbName = FileIO.FileSystem.GetDirectoryInfo(dumpDir).Name
                dbName = dbName.NormalizePathString
            End If

            Call MySQL.Execute($"CREATE SCHEMA `{dbName}` ;")

            Dim Tables = SQLs.ToArray(Of KeyValuePair)(Function(sql) CodeGenerator.GenerateClass(sql, ""))

            Throw New NotImplementedException
        End Function
    End Class
End Namespace