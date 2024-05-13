#Region "Microsoft.VisualBasic::3c94d6a1b18b8e1bdc237a3437cc1875, src\mysqli\LibMySQL\Workbench\ServerInstances.vb"

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

    '   Total Lines: 36
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.30 KB


    '     Class ServerInstances
    ' 
    '         Properties: grt_format
    ' 
    '     Class value
    ' 
    '         Properties: _ptr_, contentStructName, contenttype, id, key
    '                     structchecksum, structname, type
    ' 
    '     Class link
    ' 
    '         Properties: key, structname, type, value
    ' 
    '     Class DictionaryValue
    ' 
    '         Properties: key, type, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Workbench.Configuration

    <XmlType("data")>
    Public Class ServerInstances

        <XmlAttribute> Public Property grt_format As String

    End Class

    Public Class value
        <XmlAttribute> Public Property _ptr_ As String
        <XmlAttribute> Public Property type As String
        <XmlAttribute("content-type")> Public Property contenttype As String
        <XmlAttribute("content-struct-name")> Public Property contentStructName As String
        <XmlAttribute("struct-name")> Public Property structname As String
        <XmlAttribute> Public Property id As String
        <XmlAttribute("struct-checksum")> Public Property structchecksum As String
        <XmlAttribute> Public Property key As String

    End Class

    Public Class link
        <XmlAttribute> Public Property type As String
        <XmlAttribute("struct-name")> Public Property structname As String
        <XmlAttribute> Public Property key As String
        <XmlText> Public Property value As String
    End Class

    Public Class DictionaryValue
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property key As String
        <XmlText> Public Property value As String
    End Class
End Namespace
