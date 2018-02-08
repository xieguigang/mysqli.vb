Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace Reflection.Helper

    Public NotInheritable Class SchemaCache(Of T As Class)

        Public Shared ReadOnly Cache As NamedValue(Of BindProperty(Of DatabaseField))()

        Private Sub New()
        End Sub

        Shared Sub New()
            Dim type As Type = GetType(T)
            Dim schema As PropertyInfo() = type.GetProperties
            Dim list As New List(Of NamedValue(Of BindProperty(Of DatabaseField)))

            ' Using the reflection to get the fields in the table schema only once.
            For Each [property] As PropertyInfo In schema
                Dim attr = [property].GetAttribute(Of DatabaseField)()

                If attr Is Nothing Then Continue For
                If Len(attr.Name) = 0 Then
                    attr.Name = [property].Name
                End If

                list += New NamedValue(Of BindProperty(Of DatabaseField)) With {
                    .Name = attr.Name,
                    .Description = $"Dim {[property].Name} As {[property].PropertyType.ToString}",
                    .Value = New BindProperty(Of DatabaseField)(attr, [property])
                }
            Next

            Cache = list
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetOrdinals(reader As DataTableReader) As Dictionary(Of String, Integer)
            Return Cache _
                .Keys _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return reader.GetOrdinal(name)
                              End Function)
        End Function
    End Class
End Namespace