#Region "Microsoft.VisualBasic::4cc92d78b311353fe0b0c97e411a0b1e, G:/graphQL/src/mysqli/LibMySQL//Reflection/DbReflector/FieldDescription.vb"

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


    ' Code Statistics:

    '   Total Lines: 32
    '    Code Lines: 24
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 1.27 KB


    '     Class FieldDescription
    ' 
    '         Properties: [Default], Extra, Field, Key, Null
    '                     Type
    ' 
    '         Function: CreateField
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.Helper

    ''' <summary>
    ''' result of mysql ``describ table``
    ''' </summary>
    Public Class FieldDescription

        <DatabaseField("Field")> Public Property Field As String
        <DatabaseField("Type")> Public Property Type As String
        <DatabaseField("Null")> Public Property Null As String
        <DatabaseField("Key")> Public Property Key As String
        <DatabaseField("Default")> Public Property [Default] As String
        <DatabaseField("Extra")> Public Property Extra As String

        Public Function CreateField() As Field
            Return New Field With {
                .FieldName = Field,
                .AutoIncrement = Extra.TextEquals("auto_increment"),
                .DataType = CreateDataType.CreateDataType(type_define:=Type),
                .NotNull = Null.TextEquals("NO"),
                .[Default] = [Default],
                .PrimaryKey = Key.TextEquals("PRI"),
                .Unsigned = Type.IndexOf("unsigned") > -1
            }
        End Function

    End Class
End Namespace
