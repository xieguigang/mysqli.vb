#Region "Microsoft.VisualBasic::68556c58993c3e7cafd96e2d91c19a54, LibMySQL\Reflection\DataModels\Update.vb"

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

    '     Class Update
    ' 
    '         Function: Generate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

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
            Dim value As String
            Dim indexValue = MyBase _
                ._schemaInfo _
                .IndexProperty _
                .GetValue(Record, Nothing) _
                .ToString

            For Each field In MyBase._schemaInfo.Fields
                value = field.PropertyInfo.GetValue(Record, Nothing).ToString
                Values.Add(value)
            Next

            ' 最后这个值是和前面的值有顺序之分的
            ' 必须要在最后进行添加
            Values.Add(indexValue)

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
