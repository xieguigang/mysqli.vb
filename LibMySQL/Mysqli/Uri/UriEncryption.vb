Imports System.Runtime.CompilerServices

Public Module UriEncryption

#Region "假若需要将连接参数的配置数据保存至文件之中的话，则可以使用这两个方法来完成"

    ''' <summary>
    ''' 重新生成链接url字符串
    ''' </summary>
    ''' <returns></returns>
    ''' <param name="passwordEncryption">用户自定义的密码加密信息</param>
    ''' <remarks></remarks>
    ''' 
    <Extension>
    Public Function GenerateUri(uri As ConnectionUri, passwordEncryption As Func(Of String, String)) As String
        Dim usr As String = passwordEncryption(uri.User)
        Dim pwd As String = passwordEncryption(uri.Password)
        Dim dbn As String = passwordEncryption(uri.Database)
        Dim url As String = $"https://{IPAddress}:{uri.Port}/client?user={usr}%password={pwd}%database={dbn}"
        Return uri
    End Function

    ''' <summary>
    ''' 从配置数据之中加载数据库的连接信息
    ''' </summary>
    ''' <param name="url"></param>
    ''' <param name="passwordDecryption"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateObject(url As String, passwordDecryption As Func(Of String, String)) As ConnectionUri
        Dim URI As ConnectionUri = ConnectionUri.TryParsing(url)
        URI.Database = passwordDecryption(URI.Database)
        URI.User = passwordDecryption(URI.User)
        URI.Password = passwordDecryption(URI.Password)

        Call Debug.WriteLine(URI.GetConnectionString)

        Return URI
    End Function
#End Region

End Module
