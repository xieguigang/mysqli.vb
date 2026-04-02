Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.SQL
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

Public Module CsvHelper

    ''' <summary>
    ''' 将csv导入数据库之中的帮助工具，利用这个工具解析csv文件的头部标题行，生成``Create Table``脚本
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function CsvImportsHelper(path As String) As String
        Dim header$() = path _
            .ReadFirstLine _
            .Split(","c) _
            .Select(Function(s) s.Trim(ASCII.Quot)) _
            .ToArray
        Dim fields As Dictionary(Of String, Field) = header _
            .ToDictionary(Function(name) name,
                          Function(name)
                              Return simpleField(name)
                          End Function)
        Dim schema As New Table(fields) With {
            .TableName = path.BaseName,
            .Comment = $"Auto-generated table schema from csv file: {path}",
            .PrimaryFields = New List(Of String) From {header.First},
            .Database = path.ParentDirName
        }
        Dim SQL$ = CreateTableSQL.FromSchema(schema)
        Return SQL
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function simpleField(name As String) As Field
        Return New Field() With {
            .FieldName = name,
            .DataType = New DataType(MySqlDbType.Text)
        }
    End Function
End Module
