Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateSchema

        Private Function ParseTableName(tableName As String) As String
            Dim tokens As String() = r.Matches(tableName, "`.+?`").ToArray
            tableName = tokens.Last
            tableName = Mid(tableName, 2, Len(tableName) - 2)
            Return tableName
        End Function

        ''' <summary>
        ''' Create a MySQL table schema object.
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSchemaInner(tokens As TableTokens) As Table
            Dim primaryKeys$()
            Dim tableName = ParseTableName(tableName:=tokens.name)
            Dim primaryKey = r.Match(tokens.primaryKey, "\(`.+?`\)").Value

            If Not String.IsNullOrEmpty(primaryKey) Then
                primaryKey = r.Replace(primaryKey, "\(\d+\)", "")
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKeys = Strings.Split(primaryKey, "`,`")
            Else
                primaryKeys = New String() {}
            End If

            Dim comment As String = tokens.comment
            Dim fieldList = tokens.fields _
                .Select(Function(si) si.CreateField) _
                .ToDictionary(Function(field)
                                  Return field.FieldName
                              End Function)

            tokens.original = ASCII.ReplaceQuot(tokens.original, "\'")

            ' The database fields reflection result {Name, Attribute}
            ' Assuming at least only one primary key in a table
            Dim tableSchema As New Table(fieldList) With {
                .TableName = tableName,
                .PrimaryFields = primaryKeys.AsList,
                .Index = primaryKey,
                .Comment = comment,
                .SQL = tokens.original
            }

            Return tableSchema
        End Function
    End Module
End Namespace