Namespace Scripting

    Public Module LinqToMySQL

        Public Function Query(Of T As MySQLTable)(linq As IEnumerable(Of T)) As IEnumerable(Of T)

        End Function
    End Module
End Namespace