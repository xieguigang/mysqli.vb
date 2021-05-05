#Region "Microsoft.VisualBasic::06cb0a3663aa7a240d0212d136fc4199, LibMySQL\Scripting\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: DropTableSQL, EnsureLimit1, MySqlEscaping, replaceInternal, ToMySqlDateTimeString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex
Imports Table = Oracle.LinuxCompatibility.MySQL.Reflection.Schema.Table

Namespace Scripting

    <HideModuleName> Public Module Extensions

        ''' <summary>
        ''' 2018-04-24 03:15:21
        ''' </summary>
        Public Const DateTimePattern$ = "\d{4}[-]\d{2}[-]\d{2}\s\d{2}([:]\d{2}){2}"

        ''' <summary>
        ''' The input string is <see cref="DateTimePattern"/>?
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsMySQLDateTimePattern(str As String) As Boolean
            Return str.IsPattern(DateTimePattern)
        End Function

        ''' <summary>
        ''' 可能由于操作系统的语言或者文化的差异，直接使用<see cref="DateTime"></see>的ToString方法所得到的字符串可能会在一些环境配置之下
        ''' 无法正确的插入MySQL数据库之中，所以需要使用本方法在将对象实例进行转换为SQL语句的时候进行转换为字符串
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToMySqlDateTimeString(value As DateTime) As String
            With value
                Return String.Format("{0}-{1}-{2} {3}:{4}:{5}", .Year, .Month, .Day, .Hour, .Minute, .Second)
            End With
        End Function

        Const LIMIT1$ = "\sLIMIT\s+1\s*;?"

        ''' <summary>
        ''' 确保``SELECT`` SQL语句是以``LIMIT 1``截尾的
        ''' </summary>
        ''' <param name="SQL">已经被Trim过了的</param>
        ''' <returns></returns>
        <Extension>
        Public Function EnsureLimit1(SQL As String) As String
            Dim limitFlag$ = r.Match(SQL, LIMIT1, RegexICSng).Value

            If String.IsNullOrEmpty(limitFlag) OrElse InStrRev(SQL, limitFlag) < SQL.Length - limitFlag.Length Then
                If SQL.Last = ";"c Then
                    SQL = Mid(SQL, 1, SQL.Length - 1)
                End If
                ' 需要进行添加
                SQL = SQL & " LIMIT 1;"
            Else
                ' 已经存在了，则不需要额外的处理
            End If

            Return SQL
        End Function

        ''' <summary>
        ''' ``DROP TABLE IF EXISTS `{<see cref="Table.GetTableName"/>(GetType(<typeparamref name="T"/>))}`;``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DropTableSQL(Of T As MySQLTable)() As String
            Return $"DROP TABLE IF EXISTS `{Table.GetTableName(GetType(T))}`;"
        End Function

        ReadOnly escapingCodes As New Dictionary(Of String, String) From {
            {ASCII.NUL, "\0"},
            {"'", "\'"},
            {"""", "\"""},
            {ASCII.BS, "\b"},
            {ASCII.LF, "\n"},
            {ASCII.CR, "\r"},
            {ASCII.TAB, "\t"},
            {ASCII.SUB, "\Z"},
            {"%", "\%"}', {"_", "\_"}
        }

        Const escapingSplash$ = "{$" & NameOf(escapingSplash) & "}"

        ''' <summary>
        ''' 空值会返回空字符串
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MySqlUnescaping(value As String) As String
            If value.StringEmpty Then
                Return ""
            Else
                Dim sb As New StringBuilder(value.Replace("\\", escapingSplash))

                For Each code In escapingCodes
                    Call sb.Replace(code.Value, code.Key)
                Next

                If escapingSplash.Length < sb.Length Then
                    Call sb.Replace(escapingSplash, "\")
                End If

                Return sb.ToString
            End If
        End Function

        ''' <summary>
        ''' 处理字符串之中的特殊字符的转义。(这是一个安全的函数，如果输入的字符串为空，则这个函数会输出空字符串)
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function MySqlEscaping(value$) As String
            If value.StringEmpty Then
                Return ""
            Else
                Return replaceInternal(value)
            End If
        End Function

        ''' <summary>
        ''' do escape for mysql query
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Private Function replaceInternal(value As String) As String
            Dim sb As New StringBuilder(value.Replace("\", "\\"))

            For Each x In escapingCodes
                Call sb.Replace(x.Key, x.Value)
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
