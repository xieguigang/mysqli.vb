Imports System.Text
Imports System.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace Reflection.SQL

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="Schema"></typeparam>
    ''' <remarks>
    ''' Example SQL:
    ''' 
    ''' DELETE FROM `TableName` WHERE `IndexFieldName`='value';
    ''' </remarks>
    Public Class Delete(Of Schema) : Inherits SQL

        Public Function Generate(Record As Schema) As String
            Dim [String] As String = MyBase._schemaInfo.IndexProperty.GetValue(Record, Nothing).ToString
            Return System.String.Format(GenerateDeleteSql(_schemaInfo), [String])
        End Function

        Public Shared Widening Operator CType(tbl As Reflection.Schema.Table) As Delete(Of Schema)
            Return New Delete(Of Schema) With {._schemaInfo = tbl}
        End Operator
    End Class
End Namespace

