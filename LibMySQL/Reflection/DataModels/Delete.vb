#Region "Microsoft.VisualBasic::9f9e2e7d65afd77de8487c88bc48b323, ..\mysqli\LibMySQL\Reflection\DataModels\Delete.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices

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
            Dim s$ = MyBase._schemaInfo.IndexProperty.GetValue(Record, Nothing).ToString
            Return String.Format(GenerateDeleteSql(_schemaInfo), s)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tbl As Reflection.Schema.Table) As Delete(Of Schema)
            Return New Delete(Of Schema) With {
                ._schemaInfo = tbl
            }
        End Operator
    End Class
End Namespace
