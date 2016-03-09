Imports System.Text
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
    ''' INSERT INTO `TableName` (`Field1`, `Field2`, `Field3`) VALUES ('1', '1', '1');
    ''' </remarks>
    Public Class Insert(Of Schema) : Inherits SQL

        ''' <summary>
        ''' INSERT INTO `TableName` (`Field1`, `Field2`, `Field3`, ...) VALUES ('{0}', '{1}', '{2}', ...);
        ''' </summary>
        ''' <remarks></remarks>
        Friend InsertSQL As String

        ''' <summary>
        ''' Generate the INSERT sql command of the instance of the specific type of 'Schema'.
        ''' (生成特定的'Schema'数据类型实例的 'INSERT' sql命令)
        ''' </summary>
        ''' <param name="value">The instance to generate this command of type 'Schema'</param>
        ''' <returns>INSERT sql text</returns>
        ''' <remarks></remarks>
        Public Function Generate(value As Schema) As String
            Dim valuesbuffer As List(Of String) = New List(Of String)

            For Each Field In MyBase._schemaInfo.Fields
                Dim s_value As String = Field.PropertyInfo.GetValue(value, Nothing).ToString
                Call valuesbuffer.Add(s_value)
            Next

            Return String.Format(InsertSQL, valuesbuffer.ToArray)
        End Function

        Public Shared Widening Operator CType(schema As Reflection.Schema.Table) As Insert(Of Schema)
            Return New Insert(Of Schema) With {
                ._schemaInfo = schema,
                .InsertSQL = GenerateInsertSql(schema)
            }
        End Operator
    End Class
End Namespace

