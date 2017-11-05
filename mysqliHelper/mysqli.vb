Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Terminal
Imports Oracle.LinuxCompatibility.MySQL
Imports mysqliEnd = Oracle.LinuxCompatibility.MySQL.MySQL

''' <summary>
''' Mysqli connection config helper.(一些比较常用的mysql连接拓展)
''' </summary>
<RunDllEntryPoint(NameOf(mysqli))> Public Module mysqli

    ''' <summary>
    ''' Initializes MySQLi and returns a resource for use with <see cref="mysqliEnd"/>
    ''' (从命令行所设置的环境变量之中初始化mysql的数据库连接的通用拓展)
    ''' </summary>
    ''' <param name="mysql"></param>
    <Extension> Public Sub init_cli(ByRef mysql As mysqliEnd)
        If mysql <= New ConnectionUri With {
            .Database = App.GetVariable("database"),
            .IPAddress = App.GetVariable("host"),
            .Password = App.GetVariable("password"),
            .Port = App.GetVariable("port"),
            .User = App.GetVariable("user")
        } = -1.0R Then

#If Not DEBUG Then
            Throw New Exception("No MySQL database connection!")
#Else
            Call "No mysqli database connection!".Warning
#End If
        End If
    End Sub

    ''' <summary>
    ''' Init connection from default config file.
    ''' </summary>
    ''' <param name="mysql"></param>
    <Extension> Public Sub init(ByRef mysql As mysqliEnd)
        If mysql <= mysqli.LoadConfig = -1.0R Then
#If Not DEBUG Then
            Throw New Exception("No MySQL database connection!")
#Else
            Call "No mysqli database connection!".Warning
#End If
        End If
    End Sub

    ''' <summary>
    ''' 使用``httpd``的``run.dll``命令行进行测试
    ''' 
    ''' ```
    ''' mysqli::TestMySQLi
    ''' ```
    ''' </summary>
    Public Sub TestMySQLi()
        Try
            Call New mysqliEnd().init
            Call "Mysqli connection config test success!".__INFO_ECHO
        Catch ex As Exception
            Call "Mysqli connection config test failure!".PrintException
        End Try
    End Sub

    Const UpdateWarning$ = "MySQLi Manager will update your connection with these information: "
    Const dataFile$ = "mysqli.dat"

    Public Sub RunConfig()
        Dim update As Action(Of ConnectionUri) =
            Sub(mysqli As ConnectionUri)
                Dim key = Rnd().ToString("F5")
                Dim encrypt As New SHA256(key, 12345678)
                Dim encrypted = mysqli.GenerateUri(AddressOf encrypt.EncryptData)

                encrypted = encrypted & "|" & key
                encrypted.SaveTo($"{App.LocalData}/{dataFile}", Encoding.ASCII)
            End Sub

        Call update.RunConfig
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="update">更新到配置文件</param>
    <Extension>
    Public Sub RunConfig(update As Action(Of ConnectionUri))
        Dim readString = Function(s$, ByRef result$)
                             result = s
                             Return Not s.StringEmpty
                         End Function
        Dim readInteger = Function(s$, ByRef i%)
                              i = Val(s)
                              Return i.ToString = s
                          End Function
        Dim database$ = STDIO.Read(Of String)("Please input the database name", readString, "smrucc-webcloud")
        Dim hostName$ = STDIO.Read(Of String)("Please input the host name", readString, "localhost")
        Dim port% = STDIO.Read(Of Integer)("Please input the port", readInteger, 3306)
        Dim user$ = STDIO.Read(Of String)("Enter your user name", readString, "root")
        Dim password$ = STDIO.InputPassword
        Dim mysqli As New ConnectionUri With {
            .Database = database,
            .IPAddress = hostName,
            .Password = password,
            .Port = port,
            .User = user
        }
        Dim confirm = STDIO.MsgBox(UpdateWarning & mysqli.GetDisplayUri)

        If confirm = MsgBoxResult.No Then
            Call "User cancel update config...".__INFO_ECHO
        Else
            Call update(mysqli)
        End If
    End Sub

    ''' <summary>
    ''' Load mysqli connection info from default config file.
    ''' </summary>
    ''' <returns></returns>
    Public Function LoadConfig() As ConnectionUri
        Dim encrypted$ = ($"{App.LocalData}/{dataFile}").ReadAllText
        Dim key = encrypted.Split("|"c).Last
        encrypted = encrypted.Replace("|" & key, "")
        Dim descrypt As New SHA256(key, 12345678)
        Dim uri As ConnectionUri = ConnectionUri.CreateObject(encrypted, AddressOf descrypt.DecryptString)
        Return uri
    End Function

    Public Function CopyToModule(exe$) As Boolean
        Dim configuration$ = $"{App.LocalData}/{dataFile}".ReadAllText
        Dim target$ = App.GetAppLocalData(exe)

        Return configuration.SaveTo(target, Encoding.ASCII)
    End Function
End Module
