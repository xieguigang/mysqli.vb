Imports System.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports System.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Microsoft.VisualBasic

Namespace Reflection.SQL

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Example SQL:
    ''' 
    ''' UPDATE `TableName` 
    ''' SET `Field1`='value', `Field2`='value' 
    ''' WHERE `IndexField`='index';
    ''' </remarks>
    Public Class Update(Of Schema) : Inherits SQL

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Protected UpdateSQL As String

        Public Function Generate(Record As Schema) As String
            Dim Values As New List(Of String)

            For Each Field In MyBase._schemaInfo.Fields
                Dim value As String = Field.PropertyInfo.GetValue(Record, Nothing).ToString
                Values.Add(value)
            Next
            Values.Add(MyBase._schemaInfo.IndexProperty.GetValue(Record, Nothing).ToString)

            Return String.Format(UpdateSQL, Values.ToArray)
        End Function

        Public Shared Widening Operator CType(schema As Table) As Update(Of Schema)
            Return New Update(Of Schema) With {
                ._schemaInfo = schema,
                .UpdateSQL = GenerateUpdateSql(schema)
            }
        End Operator
    End Class
End Namespace