#Region "Microsoft.VisualBasic::a28dd65a216af568a3d0252e9a7b8768, LibMySQL\Reflection\DbReflector\ExecuteScalar.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Class SchemaCache
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: GetOrdinals
' 
' 
' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace Reflection.Helper

    ''' <summary>
    ''' A cache holder for the .net clr object schema inside the database
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public NotInheritable Class SchemaCache(Of T As Class)

        ''' <summary>
        ''' the field data of current data type mapping result
        ''' </summary>
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
