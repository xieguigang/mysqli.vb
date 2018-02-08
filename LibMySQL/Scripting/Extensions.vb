Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text
Imports r = System.Text.RegularExpressions.Regex
Imports Table = Oracle.LinuxCompatibility.MySQL.Reflection.Schema.Table

Namespace Scripting

    Public Module Extensions

        Const LIMIT1$ = "\sLIMIT\s+1\s*;?"

        ''' <summary>
        ''' 确保``SELECT`` SQL语句是以``LIMIT 1``截尾的
        ''' </summary>
        ''' <param name="SQL">已经被Trim过了的</param>
        ''' <returns></returns>
        <Extension> Public Function EnsureLimit1(SQL As String) As String
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

        ' {"\", "\\"}

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

        Private Function replaceInternal(value As String) As String
            Dim sb As New StringBuilder(value.Replace("\", "\\"))

            For Each x In escapingCodes
                Call sb.Replace(x.Key, x.Value)
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
