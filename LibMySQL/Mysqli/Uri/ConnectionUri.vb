#Region "Microsoft.VisualBasic::693d17cbf1fb7a2c7872d13a6800e679, ..\mysqli\LibMySQL\MYSQL.Client\ConnectionUri.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

''' <summary>
''' The connection parameter for the MYSQL database server.(MySQL服务器的远程连接参数)
''' </summary>
''' <remarks>
''' #### Using MySqlCommand
'''
''' A MySqlCommand has the CommandText And CommandType properties associated With it. The CommandText will be handled differently depending On the setting Of CommandType. 
''' CommandType can be one Of: 
'''
''' + Text -An SQL text command (default)
''' + StoredProcedure -The name of a Stored Procedure
''' + TableDirect -The name of a table (New in Connector/Net 6.2)
'''
''' The Default CommandType, Text, is used For executing queries And other SQL commands. Some example Of this can be found In the following section Section 4.1.2, 
''' **“The MySqlCommand Object”**.
'''
''' + If CommandType Is set to StoredProcedure, set CommandText to the name of the Stored Procedure to access.
''' + If CommandType Is set to TableDirect, all rows And columns of the named table will be returned when you call one of the Execute methods. In effect, 
'''   this command performs a SELECT * on the table specified. The CommandText property Is set to the name of the table to query. This Is illustrated by the 
'''   following code snippet
'''
''' ```vbnet
''' ' ...
''' Dim cmd As New MySqlCommand()
''' cmd.CommandText = "mytable"
''' cmd.Connection = someConnection
''' cmd.CommandType = CommandType.TableDirect
''' Dim reader As MySqlDataReader = cmd.ExecuteReader()
'''
''' Do While (reader.Read())
'''     Call Console.WriteLine(reader(0), reader(1)...)
''' Loop
''' ' ...
''' ```
''' 
''' Examples of using the CommandType of StoredProcedure can be found in the section Section 5.10, “Accessing Stored Procedures with Connector/Net”.
''' Commands can have a timeout associated With them. This Is useful As you may Not want a situation were a command takes up an excessive amount Of time. 
''' A timeout can be Set Using the CommandTimeout Property. The following code snippet sets a timeout Of one minute:
'''
''' ```vbnet
''' Dim cmd As New MySqlCommand()
''' cmd.CommandTimeout = 60
''' ```
''' 
''' The Default value Is 30 seconds. **Avoid a value Of 0, which indicates an indefinite wait.** To change the Default command timeout, use the connection String 
''' Option Default Command Timeout.
''' Prior to MySQL Connector/Net 6.2, MySqlCommand.CommandTimeout included user processing time, that Is processing time Not related To direct use Of the connector. 
''' Timeout was implemented through a .NET Timer, that triggered after CommandTimeout seconds. This timer consumed a thread.
''' MySQL Connector/Net 6.2 introduced timeouts that are aligned With how Microsoft Handles SqlCommand.CommandTimeout. This Property Is the cumulative timeout 
''' For all network reads And writes during command execution Or processing Of the results. A timeout can still occur In the MySqlReader.Read method after the first 
''' row Is returned, And does Not include user processing time, only IO operations. The 6.2 implementation uses the underlying stream timeout facility, so Is more 
''' efficient In that it does Not require the additional timer thread As was the Case With the previous implementation.
''' </remarks>
Public Class ConnectionUri

    ''' <summary>
    ''' The server IP address, you can using ``localhost`` to specific the local machine.(服务器的IP地址，可以使用localhost来指代本机)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <XmlAttribute> Public Property IPAddress As String
    ''' <summary>
    ''' The port number of the remote database server.(数据库服务器的端口号)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <XmlAttribute> Public Property Port As UInteger

    ''' <summary>
    ''' ``Using &lt;database_name>``.(数据库的名称)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <XmlAttribute> Public Property Database As String
    <XmlAttribute> Public Property User As String
    <XmlAttribute> Public Property Password As String

    ''' <summary>
    ''' 这个属性会在链接字符串之中设置查询超时的选项，单位为秒：
    ''' 
    ''' ```
    ''' default command timeout={TimeOut};
    ''' ```
    ''' 
    ''' + 假若这个参数值为负数，则不会进行设置，默认值为-1，为负值，则默认不会设置超时选项，即使用默认的超时设置30秒
    ''' + 假若这个参数值为0，则会被设置为无限等待
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property TimeOut As Integer = -1

    Sub New()
    End Sub

    ''' <summary>
    ''' 复制值
    ''' </summary>
    ''' <param name="o"></param>
    Sub New(o As ConnectionUri, Optional DbName$ = Nothing)
        Me.Database = DbName Or o.Database.AsDefault
        Me.IPAddress = o.IPAddress
        Me.Password = o.Password
        Me.Port = o.Port
        Me.TimeOut = o.TimeOut
        Me.User = o.User
    End Sub

    ''' <summary>
    ''' Get a connection string for the connection establish of a client to a mysql database 
    ''' server using the specific paramenter that was assigned by the user.
    ''' (获取一个由用户指定连接参数的用于建立客户端和MySql数据库服务器之间的连接的连接字符串)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetConnectionString() As String
        Return Me.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDisplayUri() As String
        Return $"https://{Me.IPAddress}:{Me.Port}/mysql/db_{Me.Database}/user_{Me.User}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return BuildConnectionString
    End Function

    ''' <summary>
    ''' Create a mysql connection using the connection uri
    ''' </summary>
    ''' <param name="url"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CreateObject(url As String) As MySQL
        Return CType(CType(url, ConnectionUri), MySQL)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CreateObject(url As String, passwordDecryption As Func(Of String, String)) As ConnectionUri
        Return UriEncryption.CreateObject(url, passwordDecryption)
    End Function

    ''' <summary>
    ''' Conver the ConnectionHelper object to a mysql connection string using 
    ''' the specific parameter which assigned by the user.
    ''' (将使用由用户指定连接参数的连接生成器转换为Mysql数据库的连接字符串)
    ''' </summary>
    ''' <param name="uri"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(uri As ConnectionUri) As String
        Return uri.GetConnectionString
    End Operator

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="url">MySql connection string.(MySql连接字符串)</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Example: 
    ''' http://localhost:8080/client?user=username%password=password%database=database
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Widening Operator CType(url As String) As ConnectionUri
        Return TryParsing(url)
    End Operator

    ''' <summary>
    ''' 函数会自动解析MySQL格式或者uri拓展格式
    ''' </summary>
    ''' <param name="uri">MySQL连接字符串或者uri拓展格式</param>
    ''' <returns></returns>
    Public Shared Function TryParsing(uri As String) As ConnectionUri
        If Not InStr(uri, "https://", CompareMethod.Text) > 0 Then
            Return UriBuilder.MySQLParser(uri)
        Else
            Return UriBuilder.UriParser(uri)
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Widening Operator CType(X As XElement) As ConnectionUri
        Return CType(X.Value, ConnectionUri)
    End Operator
End Class
