Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON
Imports r = System.Text.RegularExpressions.Regex

Public Module UriBuilder

#Region "Parser"

    ''' <summary>
    ''' 解析url格式的链接字符串
    ''' </summary>
    ''' <param name="uri"></param>
    ''' <returns></returns>
    <Extension> Public Function UriParser(uri As String) As ConnectionUri
        Dim temp$ = r.Match(uri, SERVERSITE).Value
        Dim server$() = temp.Split("/"c).Last.Split(":"c)
        Dim profiles$() = Mid(uri, InStr(uri, "client?", CompareMethod.Text) + 7).Split("%"c)
        Dim fields = profiles _
            .Select(Function(s) s.GetTagValue("=")) _
            .ToDictionary(Function(f) f.Name,
                          Function(f) f.Value)
#If DEBUG Then
        Call fields.GetJson(indent:=True).__DEBUG_ECHO
#End If

        Dim newURI As New ConnectionUri With {
            .IPAddress = server.First,
            .Port = CInt(Val(server.Last)),
            .User = fields.TryGetValue("user"),
            .Password = fields.TryGetValue("password"),
            .Database = fields.TryGetValue("database")
        }

        Return newURI
    End Function

    ReadOnly defaultPort As DefaultValue(Of String) = "3306"

    ''' <summary>
    ''' ``Database={0}; Data Source={1}; User Id={2}; Password={3}; Port={4}``
    ''' </summary>
    ''' <param name="cnn"></param>
    ''' <returns></returns>
    Public Function MySQLParser(cnn As String) As ConnectionUri
        Dim Database$ = cnn.getField("Database=")
        Dim User$ = cnn.getField("User Id=")
        Dim Password$ = cnn.getField("Password=")
        Dim IPAddress$ = cnn.getField("Data Source=")
        Dim ServicesPort$ = r _
            .Match(cnn, "Port=\d+", RegexOptions.IgnoreCase) _
            .Value _
            .GetTagValue("="c) _
            .Value

        Dim Uri As New ConnectionUri With {
            .User = User,
            .Database = Database,
            .Password = Password,
            .IPAddress = IPAddress,
            .Port = ServicesPort Or defaultPort
        }

        Return Uri
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function getField(cnn$, fieldToken$) As String
        Return r.Match(cnn, fieldToken & "\S+", RegexOptions.IgnoreCase) _
            .Value _
            .Remove(fieldToken, RegexOptions.IgnoreCase) _
            .TrimSeperator
    End Function

    <Extension> Private Function TrimSeperator(str As String) As String
        If String.IsNullOrEmpty(str) Then
            Return ""
        Else
            If str.Last = ";"c Then
                str = Mid(str, 1, Len(str) - 1)
            End If
        End If

        Return str
    End Function
#End Region

    ''' <summary>
    ''' ```
    ''' Database={0}; Data Source={1}; User Id={2}; Password={3}; Port={4};
    ''' ```
    ''' </summary>
    Public Const MYSQL_CONNECTION$ = "Database={0}; Data Source={1}; User Id={2}; Password={3}; Port={4}"

    <Extension>
    Friend Function BuildConnectionString(uri As ConnectionUri) As String
        Dim tokens As New List(Of String)

        With uri
            If Not .Database.StringEmpty Then
                tokens += $"Database={ .Database}"
            End If
            If Not .IPAddress.StringEmpty Then
                tokens += $"Data Source={ .IPAddress}"
            End If
            If Not .Password.StringEmpty Then
                tokens += $"Password={ .Password}"
            End If
            If Not .User.StringEmpty Then
                tokens += $"User Id={ .User}"
            End If
            If .Port <> 3306 Then
                tokens += $"Port={ .Port}"
            End If
        End With

        Dim cnn$ = $"{tokens.JoinBy("; ")}; charset=utf8;"

        If uri.TimeOut >= 0 Then
            ' 假若在这里timeout大于零的话，会在链接字符串中设置超时选项
            ' 等于零是无限等待
            cnn &= $" default command timeout={uri.TimeOut};"
        End If

        Return cnn
    End Function
End Module
