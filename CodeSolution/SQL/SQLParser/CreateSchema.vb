Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateSchema

        ''' <summary>
        ''' Create a MySQL table schema object.
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreateSchemaInner(tokens As TableTokens) As Table
            Dim primaryKeys$()
            Dim tableName = r.Matches(tokens.name, "`.+?`").ToArray.Last
            tableName = Mid(tableName, 2, Len(tableName) - 2)
            Dim primaryKey = r.Match(tokens.primaryKey, "\(`.+?`\)").Value

            If Not String.IsNullOrEmpty(primaryKey) Then
                primaryKey = r.Replace(primaryKey, "\(\d+\)", "")
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKey = Mid(primaryKey, 2, Len(primaryKey) - 2)
                primaryKeys = Strings.Split(primaryKey, "`,`")
            Else
                primaryKeys = New String() {}
            End If

            Dim comment As String = r.Match(tokens.original, "COMMENT='.+';", RegexOptions.Singleline).Value
            Dim fieldList = tokens.fields _
            .Select(AddressOf __createField) _
            .ToDictionary(Function(field) field.FieldName)

            If Not String.IsNullOrEmpty(comment) Then
                comment = Mid(comment, 10)
                comment = Mid(comment, 1, Len(comment) - 2)
            End If

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