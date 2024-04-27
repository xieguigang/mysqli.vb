#Region "Microsoft.VisualBasic::d5a769edfdc2b5ec260577b48ecbbf14, G:/graphQL/src/mysqli/LibMySQL//Mysqli/Uri/UriEncryption.vb"

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


    ' Code Statistics:

    '   Total Lines: 50
    '    Code Lines: 27
    ' Comment Lines: 14
    '   Blank Lines: 9
    '     File Size: 1.84 KB


    '     Module UriEncryption
    ' 
    '         Function: CreateObject, GenerateUri
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Uri

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
            Dim url As String = New ConnectionUri(uri) With {
                .User = usr,
                .Password = pwd,
                .Database = dbn
            }.GetDisplayUri

            Return url
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
End Namespace
