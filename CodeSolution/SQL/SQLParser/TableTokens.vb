Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace SQLParser

    ''' <summary>
    ''' the create table sql text parts
    ''' </summary>
    Public Class TableTokens

        ''' <summary>
        ''' table name
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        Public Property primaryKey As String
        Public Property fields As String()
        Public Property comment As String
        Public Property original As String

        Public Shared Function ParseTokens(sql As String) As TableTokens
            Dim tokens = sqlParser(sql)
            Dim tableName As String = tokens.Value(Scan0)
            Dim primaryKey As String = tokens.Name
            Dim fieldsTokens = tokens.Value _
                .Skip(1) _
                .ToArray

            Return New TableTokens With {
                .comment = tokens.Description,
                .fields = fieldsTokens,
                .name = tableName,
                .primaryKey = primaryKey,
                .original = sql
            }
        End Function

        ''' <summary>
        ''' Just parse the primary key and the data field list at here
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        Private Shared Function sqlParser(SQL As String) As NamedValue(Of String())
            Dim tokens$() = SQL.LineTokens
            Dim p% = tokens.Lookup("PRIMARY KEY")
            Dim primaryKey As String
            Dim table_comment As String = Strings.Trim(tokens.Last)

            If Not table_comment.StartsWith("COMMENT = ") Then
                table_comment = Nothing
            Else
                table_comment = table_comment.GetStackValue("'", "'").Trim
            End If

            If p = -1 Then ' 没有设置主键
                p = tokens.Lookup("UNIQUE KEY")
            End If

            If p = -1 Then
                p = tokens.Lookup("KEY")
            End If

            If p = -1 Then
                primaryKey = ""
            Else
_SET_PRIMARYKEY:
                primaryKey = tokens(p)
                tokens = tokens.Take(p).ToArray
            End If

            p = tokens.Lookup(") ENGINE=")

            If Not p = -1 Then
                tokens = tokens.Take(p).ToArray
            End If

            Return New NamedValue(Of String())(primaryKey, tokens, table_comment)
        End Function
    End Class
End Namespace