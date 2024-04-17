#Region "Microsoft.VisualBasic::6b26174755be397704a28fd40fe9b47e, LibMySQL\Mysqli\Uri\UriBuilder.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
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



' /********************************************************************************/

' Summaries:

'     Module UriBuilder
' 
'         Function: BuildConnectionString, getField, MySQLParser, TrimSeperator, UriParser
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.Bencoding
Imports r = System.Text.RegularExpressions.Regex

Namespace Uri

    ''' <summary>
    ''' 解析url格式的链接字符串
    ''' </summary>
    Public Module UriBuilder

#Region "Parser"

        ''' <summary>
        ''' IP地址或者localhost
        ''' </summary>
        Public Const SERVERSITE As String = ".+[:]\d+"

        ''' <summary>
        ''' 解析url格式的链接字符串
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <returns></returns>
        <Extension>
        Public Function UriParser(uri As String) As ConnectionUri
            Dim code = BencodeDecoder.Decode(uri.GetTagValue("://").Value)
            Dim obj As BDictionary = code(0)
            Dim conn_uri As New ConnectionUri With {
                .Database = TryCast(obj!Database, BString),
                .IPAddress = TryCast(obj!IPAddress, BString),
                .Password = TryCast(obj!Password, BString),
                .Port = TryCast(obj!Port, BInteger),
                .TimeOut = TryCast(obj!TimeOut, BInteger),
                .User = TryCast(obj!User, BString),
                .error_log = TryCast(obj!error_log, BString)
            }

            Return conn_uri
        End Function

        ReadOnly defaultPort As [Default](Of String) = "3306"

        ''' <summary>
        ''' ``Database={0}; Data Source={1}; User Id={2}; Password={3}; Port={4}``
        ''' </summary>
        ''' <param name="cnn"></param>
        ''' <returns></returns>
        Public Function MySQLParser(cnn As String) As ConnectionUri
            Dim Database$ = cnn.getField("Database")
            Dim User$ = cnn.getField("User\s+Id")
            Dim Password$ = cnn.getField("Password")
            Dim IPAddress$ = cnn.getField("Data\s+Source")
            Dim ServicesPort$ = cnn.getField("Port")
            Dim timeout$ = cnn.getField("default\s+command\s+timeout")
            Dim Uri As New ConnectionUri With {
                .User = User,
                .Database = Database,
                .Password = Password,
                .IPAddress = IPAddress,
                .Port = ServicesPort Or defaultPort,
                .TimeOut = Val(timeout)
            }

            Return Uri
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function getField(cnn$, fieldToken$) As String
            Dim matches = r.Match(cnn, $"{fieldToken}\s*[=]\s*\S+")

            If matches.Success Then
                Return matches.GetTagValue("=", trim:=True).Value.TrimEnd(";"c).Trim
            Else
                Return Nothing
            End If
        End Function
#End Region

        ReadOnly defaultPort3306 As [Default](Of UInteger) = CType(3306, UInteger).AsDefault(Function(port) CType(port, Integer) <= 0)

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
                    tokens += $"Port={ .Port Or defaultPort3306 }"
                End If
            End With

            Dim cnn$ = $"{tokens.JoinBy("; ")}; charset=utf8;"

            If uri.TimeOut >= 0 Then
                ' 假若在这里timeout大于零的话，会在链接字符串中设置超时选项
                ' 等于零是无限等待
                cnn &= $" default command timeout={uri.TimeOut};"
            End If

            ' 2018-5-21 https://bugs.mysql.com/bug.php?id=26054
            Return cnn & " Convert Zero Datetime=True;"
        End Function
    End Module
End Namespace
