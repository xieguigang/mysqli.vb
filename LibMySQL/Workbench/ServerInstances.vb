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