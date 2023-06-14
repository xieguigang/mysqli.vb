Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module RegexpParser

        ''' <summary>
        ''' Parsing the create table statement in the SQL document.
        ''' </summary>
        Const SQL_CREATE_TABLE As String = "CREATE TABLE .+?;(\r|\n)"
        Const DB_NAME As String = "CREATE\s+((DATABASE)|(SCHEMA))\s+IF\s+NOT\s+EXISTS\s+`.+?`"

        Public Function GetCreateTableSQLText(sql As String) As String
            Dim createTable As Match = r.Match(sql, SQL_CREATE_TABLE, RegexOptions.Singleline)
            Dim str As String = createTable.Value

            Return str
        End Function

        <Extension>
        Public Function SplitTableCreateInternal(sql As String) As String()
            Dim mc As MatchCollection = r.Matches(sql, SQL_CREATE_TABLE, RegexOptions.Singleline)
            Dim out As String() = mc.ToArray

            Return out
        End Function

        ''' <summary>
        ''' 获取数据库的名称
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Public Function GetDBName(SQL As String) As String
            Dim name$ = r.Match(SQL, DB_NAME, RegexOptions.IgnoreCase).Value

            If String.IsNullOrEmpty(name) Then
                Return ""
            Else
                name = r.Match(name, "`.+?`").Value
                name = Mid(name, 2, Len(name) - 2)

                Return name
            End If
        End Function
    End Module
End Namespace