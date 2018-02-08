Imports System.Runtime.CompilerServices
Imports r = System.Text.RegularExpressions.Regex

Namespace Scripting

    Module Extensions

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
    End Module
End Namespace
