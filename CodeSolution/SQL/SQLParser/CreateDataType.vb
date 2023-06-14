Imports System.Text.RegularExpressions
Imports r = System.Text.RegularExpressions.Regex

Namespace SQLParser

    Module CreateDataType

        ''' <summary>
        ''' Mapping the MySQL database type and visual basic data type 
        ''' </summary>
        ''' <param name="type_define"></param>
        ''' <returns></returns>
        Public Function __createDataType(type_define$) As Reflection.DbAttributes.DataType
            Dim type As Reflection.DbAttributes.MySqlDbType
            Dim parameter As String = ""

            If type_define.TextEquals("tinyint") OrElse r.Match(type_define, "tinyint\(\d+\)", RegexOptions.IgnoreCase).Success Then
                parameter = __getNumberValue(type_define, 1)

                If parameter = "1" Then
                    ' boolean 
                    type = Reflection.DbAttributes.MySqlDbType.Boolean
                Else
                    type = Reflection.DbAttributes.MySqlDbType.Int32
                End If

            ElseIf "int".TextEquals(type_define) OrElse r.Match(type_define, "int\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Int64
                parameter = __getNumberValue(type_define, 11)

            ElseIf Regex.Match(type_define, "varchar\(\d+\)", RegexOptions.IgnoreCase).Success OrElse Regex.Match(type_define, "char\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.VarChar
                parameter = __getNumberValue(type_define, 45)

            ElseIf Regex.Match(type_define, "double", RegexOptions.IgnoreCase).Success OrElse InStr(type_define, "float", CompareMethod.Text) > 0 Then
                type = Reflection.DbAttributes.MySqlDbType.Double

            ElseIf Regex.Match(type_define, "datetime", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "date", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "timestamp", RegexOptions.IgnoreCase).Success Then

                type = Reflection.DbAttributes.MySqlDbType.DateTime

            ElseIf Regex.Match(type_define, "text", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Text

            ElseIf InStr(type_define, "enum(", CompareMethod.Text) > 0 Then   ' enum类型转换为String类型？？？？
                type = Reflection.DbAttributes.MySqlDbType.String

            ElseIf InStr(type_define, "Blob", CompareMethod.Text) > 0 OrElse
            Regex.Match(type_define, "varbinary\(\d+\)", RegexOptions.IgnoreCase).Success OrElse
            Regex.Match(type_define, "binary\(\d+\)", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Blob

            ElseIf Regex.Match(type_define, "decimal\(", RegexOptions.IgnoreCase).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Decimal

            ElseIf Regex.Match(type_define, "bit\(", RegexICSng).Success Then
                type = Reflection.DbAttributes.MySqlDbType.Bit
                parameter = __getNumberValue(type_define, 1)

            Else

                'More complex type is not support yet, but you can easily extending the mapping code at here
                Throw New NotImplementedException($"Type define is not support yet for    {NameOf(type_define)}   >>> ""{type_define}""")

            End If

            Return New Reflection.DbAttributes.DataType(type, parameter)
        End Function

        Private Function __getNumberValue(typeDef$, default$) As String
            Dim parameter$ = r.Match(typeDef, "\(.+?\)").Value

            If parameter.StringEmpty Then
                Return [default]
            Else
                parameter = Mid(parameter, 2, Len(parameter) - 2)
                Return parameter
            End If
        End Function
    End Module
End Namespace