Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.SQL

    Public MustInherit Class SQL

        ''' <summary>
        ''' The table schema of the sql generation target.(用于生成SQL语句的表结构属性)
        ''' </summary>
        ''' <remarks></remarks>
        Protected _schemaInfo As Schema.Table

        Protected Sub New()
        End Sub

        Public Sub New(SchemaInfo As Schema.Table)
            _schemaInfo = SchemaInfo
        End Sub

        Public Overrides Function ToString() As String
            Return _schemaInfo.ToString
        End Function
    End Class
End Namespace
