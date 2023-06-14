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
            Dim fieldName As String = tokens(0)
            Dim dataType As String = tokens(1)
            Dim commentText As String = r.Match(fieldDef, FIELD_COMMENTS).Value
            Dim i As Integer = InStr(fieldDef, fieldName)

            fieldDef = Mid(fieldDef, i + Len(fieldName))
            i = InStr(fieldDef, dataType)
            fieldDef = Mid(fieldDef, i + Len(dataType)).Replace(",", "").Trim
            fieldName = Mid(fieldName, 2, Len(fieldName) - 2)

            If Not String.IsNullOrEmpty(commentText) Then
                commentText = Mid(commentText, 10)
                commentText = Mid(commentText, 1, Len(commentText) - 2)
            End If

            Dim pos% = InStr(fieldDef, "COMMENT '", CompareMethod.Text)
            Dim p As i32 = 0

            If pos = 0 Then
                ' 没有注释，则百分之百就是列属性了
                pos = Integer.MaxValue
            End If

            Dim autoIncrement As Boolean = (p = InStr(fieldDef, "AUTO_INCREMENT", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim IsNotNull As Boolean = (p = InStr(fieldDef, "NOT NULL", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim unsigned As Boolean = (p = InStr(fieldDef, "UNSIGNED", CompareMethod.Text)) > 0 AndAlso p < pos
            Dim zeroFill As Boolean = (p = InStr(fieldDef, "ZEROFILL", CompareMethod.Text)) > 0 AndAlso p < pos
            ' Some data type can be merged into a same type
            ' when we mapping a database table
            Dim field As New Field With {
                .FieldName = fieldName,
                .DataType = CreateDataType.CreateDataType(dataType.Replace(",", "").Trim),
                .Comment = commentText,
                .AutoIncrement = autoIncrement,
                .NotNull = IsNotNull,
                .Unsigned = unsigned,
                .ZeroFill = zeroFill
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