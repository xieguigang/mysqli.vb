#Region "Microsoft.VisualBasic::ee9ebac8b117684f9d2f4f5a3ba10d43, src\mysqli\LibMySQL\Reflection\Schema\FieldAttributes.vb"

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

    '   Total Lines: 170
    '    Code Lines: 117
    ' Comment Lines: 28
    '   Blank Lines: 25
    '     File Size: 6.35 KB


    '     Class Field
    ' 
    '         Properties: [Default], [PropertyInfo], AutoIncrement, Binary, Comment
    '                     DataType, FieldName, NotNull, PrimaryKey, Unique
    '                     Unsigned, ZeroFill
    ' 
    '         Function: hasDefault, ParseAttributes, PropertyParser, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace Reflection.Schema

    ''' <summary>
    ''' Mysql database field
    ''' </summary>
    Public Class Field : Implements INamedValue

        Public Property FieldName As String Implements INamedValue.Key
        Public Property Unique As Boolean
        Public Property PrimaryKey As Boolean
        Public Property DataType As DataType

        ''' <summary>
        ''' UNSIGNED
        ''' </summary>
        ''' <returns></returns>
        Public Property Unsigned As Boolean
        ''' <summary>
        ''' NOT NULL
        ''' </summary>
        ''' <returns></returns>
        Public Property NotNull As Boolean
        ''' <summary>
        ''' AUTO_INCREMENT
        ''' (值是自动增长的)
        ''' </summary>
        ''' <returns></returns>
        Public Property AutoIncrement As Boolean
        ''' <summary>
        ''' ZEROFILL
        ''' </summary>
        ''' <returns></returns>
        Public Property ZeroFill As Boolean
        Public Property Binary As Boolean
        Public Property [Default] As String = String.Empty

        ''' <summary>
        ''' The property information of this custom database field attribute. 
        ''' </summary>
        ''' <remarks>The reflection binding</remarks>
        Public Property [PropertyInfo] As PropertyInfo

        Public Property Comment As String

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder($"`{FieldName}` {DataType.ToString}")

            With New List(Of NamedValue(Of Boolean)) From {
                {"UNSIGNED", Unsigned},
                {"ZEROFILL", ZeroFill},
                {"NOT NULL", NotNull},
                {"BINARY", Binary},
                {"AUTO_INCREMENT", AutoIncrement}
            }.Where(Function(a)
                        Return a.Value = True
                    End Function) _
             .ToArray

                If .Length > 0 Then
                    sb.Append(" ") _
                      .Append(.Keys.JoinBy(" "))
                End If
            End With

            If hasDefault() Then
                Call sb.Append(" ")

                Select Case DataType.MySQLType
                    Case MySqlDbType.LongText,
                         MySqlDbType.MediumText,
                         MySqlDbType.Text,
                         MySqlDbType.TinyText

                        Call sb.AppendFormat("DEFAULT '{0}'", [Default])
                    Case MySqlDbType.Date
                        If [Default].TextEquals("now()") Then
                            Call sb.Append("DEFAULT now()")
                        Else
                            Call sb.AppendFormat("DEFAULT '{0}'", [Default])
                        End If
                    Case Else
                        Call sb.AppendFormat("DEFAULT {0}", [Default])
                End Select
            End If

            Return sb.ToString
        End Function

        Private Function hasDefault() As Boolean
            If [Default] Is Nothing Then
                Return False
            End If

            Select Case DataType.MySQLType
                Case MySqlDbType.LongText,
                     MySqlDbType.MediumText,
                     MySqlDbType.Text,
                     MySqlDbType.TinyText

                    Return True
                Case Else
                    Return Not [Default].StringEmpty
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(field As Field) As String
            Return field.ToString
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType([Property] As PropertyInfo) As Field
            Return PropertyParser([Property])
        End Operator

        Public Shared Function PropertyParser([property] As PropertyInfo) As Field
            Dim attr As DatabaseField = GetAttribute(Of DatabaseField)([property])

            If Not attr Is Nothing AndAlso Len(attr.Name) > 0 Then
                Return ParseAttributes(attr, [property])
            Else
                ' This property is not define as a database field 
                ' as this property has no custom attribute of type 
                ' [DatabaseField].
                Return Nothing
            End If
        End Function

        Public Shared Function ParseAttributes(attr As DatabaseField, [property] As PropertyInfo) As Field
            Dim type As DataType = GetAttribute(Of DataType)([property])
            Dim default$ = Nothing

            ' Not define this custom attribute.
            If type Is Nothing Then
                type = [property].PropertyType.GetDbDataType
            End If

            With GetAttribute(Of [Default])([property])
                If Not .IsNothing Then
                    [default] = .DefaultValue
                End If
            End With

            Dim field As New Field With {
                .FieldName = attr.Name,
                .Unique = Not GetAttribute(Of Unique)([property]) Is Nothing,
                .PrimaryKey = Not GetAttribute(Of PrimaryKey)([property]) Is Nothing,
                .AutoIncrement = Not GetAttribute(Of AutoIncrement)([property]) Is Nothing,
                .Binary = Not GetAttribute(Of Binary)([property]) Is Nothing,
                .NotNull = Not GetAttribute(Of NotNULL)([property]) Is Nothing,
                .Unsigned = Not GetAttribute(Of Unsigned)([property]) Is Nothing,
                .ZeroFill = Not GetAttribute(Of ZeroFill)([property]) Is Nothing,
                .PropertyInfo = [property],
                .DataType = type,
                .Default = [default]
            }

            Return field
        End Function
    End Class
End Namespace
