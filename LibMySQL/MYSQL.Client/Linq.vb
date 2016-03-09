
''' <summary>
''' Linq to MySQL
''' </summary>
''' <typeparam name="TTable"></typeparam>
Public Class Linq(Of TTable As SQLTable) : Inherits Table(Of TTable)

    Dim __reflector As Reflection.DbReflector

    Sub New(uri As ConnectionUri)
        Call MyBase.New(uri)
        Call __init()
    End Sub

    Sub New(Engine As MySQL)
        Call MyBase.New(Engine)
        Call __init()
    End Sub

    Sub New(base As Table(Of TTable))
        Call MyBase.New(base.MySQL)
        Call __init()
    End Sub

    Private Sub __init()
        __reflector = New Reflection.DbReflector(MySQL.UriMySQL)
    End Sub

    Public Overloads Shared Operator <=(DBI As Linq(Of TTable), SQL As String) As Generic.IEnumerable(Of TTable)
        Dim err As String = ""
        Dim query As Generic.IEnumerable(Of TTable) = DBI.__reflector.AsQuery(Of TTable)(SQL, getError:=err)
        Return query
    End Operator

    Public Overloads Shared Operator >=(DBI As Linq(Of TTable), SQL As String) As Generic.IEnumerable(Of TTable)
        Return DBI <= SQL
    End Operator
End Class
