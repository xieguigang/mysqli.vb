Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' API for rundll32.exe
''' </summary>
''' 
<PackageNamespace("Mysqld", Publisher:="Oracle Corp")>
<RunDllEntryPoint("MySQL")>
Public Module RunDll

    <ExportAPI("--start", Info:="Start the embedded tiny mysqld services.")>
    Public Function StartTinyServices(args As Microsoft.VisualBasic.CommandLine.CommandLine) As Integer
        Try
            Call New TinyServer().Start()
            Return 0
        Catch ex As Exception
            Return -1
        End Try
    End Function
End Module
