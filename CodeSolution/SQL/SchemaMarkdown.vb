Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Public Module SchemaMarkdown

    <Extension>
    Private Function __attrs(field As Field) As String
        Dim a As New List(Of String)

        With field

            If .AutoIncrement Then
                a += "AI"
            End If
            If .Binary Then
                a += "B"
            End If
            If .NotNull Then
                a += "NN"
            End If
            If .PrimaryKey Then
                a += "PK"
            End If
            If .Unique Then
                a += "UQ"
            End If
            If .Unsigned Then
                a += "UN"
            End If
            If .ZeroFill Then
                a += "ZF"
            End If

        End With

        Return a _
            .Select(Function(s) $"``{s}``") _
            .JoinBy(", ")
    End Function

    <Extension>
    Public Function MakeMarkdown(table As Table) As String
        Dim md As New StringBuilder

        Call md.AppendLine("## " & table.TableName)
        Call md.AppendLine(table.Comment)
        Call md.AppendLine()
        Call md.AppendLine("|field|type|attributes|description|")
        Call md.AppendLine("|-----|----|----------|-----------|")

        For Each field As Field In table.Fields
            Dim columns = {
                field.FieldName,
                field.DataType.ToString,
                field.__attrs,
                field.Comment
            }
            Dim row$ = columns _
                .Select(Function(s) s.Replace("|", "\|")) _
                .JoinBy("|")
            Call md.AppendLine("|" & row & "|")
        Next

        Call md.AppendLine()
        Call md.AppendLine("```SQL")
        Call md.AppendLine(table.SQL)
        Call md.AppendLine("```")

        Call md.AppendLine()
        Call md.AppendLine()

        Return md.ToString
    End Function

    <Extension>
    Public Function Documentation(schema As IEnumerable(Of Table)) As String
        Dim md As New StringBuilder

        Call md.AppendLine("# MySQL development docs")

        Call md.AppendLine("Mysql database field attributes notes:")
        Call md.AppendLine()
        Call md.AppendLine("> AI: Auto Increment; B: Binary; NN: Not Null; PK: Primary Key; UQ: Unique; UN: Unsigned; ZF: Zero Fill")
        Call md.AppendLine()

        For Each t As Table In schema
            Call md.AppendLine(t.MakeMarkdown)
        Next

        Return md.ToString
    End Function
End Module
