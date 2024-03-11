#Region "Microsoft.VisualBasic::a6c9eacf3b3c5f2959307b61bc30ed4c, LibMySQL\Reflection\DbReflector\DbReflector.vb"

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

'     Class DbReflector
' 
'         Function: (+2 Overloads) __getObject, __queryInitSchema, ReadFirst
'         Class __readFirst
' 
'             Properties: value
' 
'             Function: __getFirst
' 
'         Delegate Function
' 
'             Constructor: (+3 Overloads) Sub New
' 
'             Function: __linqToMySQL, __queryEngine, __queryInvoke, __queryParallelInvoke, AsQuery
'                       ParallelQuery, Query, ToString
' 
'             Sub: (+2 Overloads) ForEach
'         Class Linq_2MySQL
' 
'             Constructor: (+2 Overloads) Sub New
' 
'             Function: ___invoke, __queryEngine
' 
'             Sub: __forEach
' 
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Data.Common
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace Reflection

    Public NotInheritable Class DbReflector

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 假若目标数据表不存在数据记录，则会返回空值
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="Reader"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ReadFirst(Of T As Class)(reader As DataTableReader) As T
            If Not reader.HasRows Then
                Return Nothing
            Else
                Return Load(Of T)(reader, getErr:=Nothing).First
            End If
        End Function

        Public Shared Iterator Function Load(Of T As Class)(reader As DataTableReader, getErr As Value(Of String)) As IEnumerable(Of T)
            Dim clr_map As Type = GetType(T)
            Dim schema = InitSchema(reader, type:=clr_map).ToArray
            Dim [error] As Exception = Nothing

            Do While reader.Read
                Yield __getObject(Of T)(reader, clr_map, schema, [error])

                If Not [error] Is Nothing Then
                    If getErr Is Nothing Then
                        Throw [error]
                    Else
                        getErr.Value = [error].ToString
                    End If

                    Exit Do
                End If
            Loop
        End Function

        Private Shared Function __getObject(Of T)(reader As DataTableReader, type As Type, fields As SeqValue(Of PropertyInfo)(), ByRef err As Exception) As T
            ' Create a instance of specific type: our record schema. 
            Dim fillObject As Object = Activator.CreateInstance(type)
            Dim i%

            Try
                ' Scan all of the fields in the field list
                ' and get the field value.
                For i = 0 To fields.Length - 1
                    Dim prop As SeqValue(Of PropertyInfo) = fields(i)
                    Dim ordinal As Integer = prop.i
                    Dim value As Object = reader.GetValue(ordinal)

                    If Not IsDBNull(value) Then
                        Call (+prop).SetValue(fillObject, value, Nothing)
                    End If
                Next
            Catch ex As Exception
                Dim errorField = fields(i)
                ex = New Exception($"[{errorField.i}] => {errorField.value.ToString}", ex)
                err = ex
            End Try

            Return DirectCast(fillObject, T)
        End Function

        ''' <summary>
        ''' 获取当前表之中可用的列名称列表
        ''' </summary>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetCurrentOrdinals(reader As DbDataReader) As IEnumerable(Of String)
            Return From r As DataRow
                   In reader.GetSchemaTable
                   Let colname = r.Item("ColumnName").ToString
                   Select colname
        End Function

        Private Shared Iterator Function InitSchema(Reader As DataTableReader, type As Type) As IEnumerable(Of SeqValue(Of PropertyInfo))
            Dim DbFieldAttr As DatabaseField
            Dim ItemTypeProperty = type.GetProperties
            Dim schema As Index(Of String) = GetCurrentOrdinals(Reader).Indexing

            For Each [property] As PropertyInfo In ItemTypeProperty
                ' Using the reflection to get the fields in
                ' the table schema only once.
                DbFieldAttr = [property].GetAttribute(Of DatabaseField)()

                If DbFieldAttr Is Nothing Then Continue For
                If String.IsNullOrEmpty(DbFieldAttr.Name) Then
                    DbFieldAttr.Name = [property].Name
                End If
                ' 反射操作的时候只对包含有的列属性进行赋值
                If Not DbFieldAttr.Name Like schema Then
                    Continue For
                End If

                Dim ordinal As Integer = Reader.GetOrdinal(DbFieldAttr.Name)

                If ordinal >= 0 Then
                    Yield New SeqValue(Of PropertyInfo) With {
                        .i = ordinal,
                        .value = [property]
                    }
                End If
            Next
        End Function
    End Class
End Namespace
