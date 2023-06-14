Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateField

        ''' <summary>
        ''' Regex expression for parsing the comments of the field in a table definition.
        ''' </summary>
        Const FIELD_COMMENTS As String = "COMMENT '.+?',"

        Private Function CreateField(fieldDef$, tokens$()) As Field
            Dim FieldName As String = tokens(0)
            Dim DataType As String = tokens(1)
            Dim Comment As String = Regex.Match(fieldDef, FIELD_COMMENTS).Value
            Dim i As Integer = InStr(fieldDef, FieldName)

            fieldDef = Mid(fieldDef, i + Len(FieldName))
            i = InStr(fieldDef, DataType)
            fieldDef = Mid(fieldDef, i + Len(DataType)).Replace(",", "").Trim
            FieldName = Mid(FieldName, 2, Len(FieldName) - 2)

            If Not String.IsNullOrEmpty(Comment) Then
                Comment = Mid(Comment, 10)
                Comment = Mid(Comment, 1, Len(Comment) - 2)
            End If

            Dim pos% = InStr(fieldDef, "COMMENT '", CompareMethod.Text)
            Dim p As i32 = 0

            If pos = 0 Then  '没有注释，则百分之百就是列属性了
                pos = Integer.MaxValue
            End If

            Dim IsAutoIncrement As Boolean = (p = InStr(fieldDef, "AUTO_INCREMENT", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim IsNotNull As Boolean = (p = InStr(fieldDef, "NOT NULL", CompareMethod.Text)) > 0 AndAlso p < pos

            ' Some data type can be merged into a same type
            ' when we mapping a database table
            Dim field As New Field With {
                .FieldName = FieldName,
                .DataType = CreateDataType.CreateDataType(DataType.Replace(",", "").Trim),
                .Comment = Comment,
                .AutoIncrement = IsAutoIncrement,
                .NotNull = IsNotNull
            }

            Return field
        End Function

        <Extension>
        Public Function CreateField(fieldDef As String) As Reflection.Schema.Field
            Dim name$ = r.Match(fieldDef, "`.+?`", RegexICSng).Value
            Dim tokens$() = {name}.Join(fieldDef.Replace(name, "").Trim.Split)

            Try
                Return CreateField(fieldDef, tokens)
            Catch ex As Exception
                Throw New Exception($"{NameOf(CreateField)} ===>  {fieldDef}{vbCrLf & vbCrLf & vbCrLf}", ex)
            End Try
        End Function
    End Module
End Namespace