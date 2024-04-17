Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Uri

''' <summary>
''' an abstract database wrapper for multiple database table models in clr types
''' </summary>
Public MustInherit Class IDatabase

    ''' <summary>
    ''' the wrapper for the mysql query functions
    ''' </summary>
    Protected mysqli As MySqli

    Public Sub New(mysqli As ConnectionUri)
        Me.mysqli = mysqli
    End Sub

    ''' <summary>
    ''' create a new data table model for create mysql query
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <returns></returns>
    Protected Function model(Of T As MySQLTable)() As Model
        Return New Model(TableName.GetTableName(Of T), mysqli)
    End Function

    ''' <summary>
    ''' create a model reference to a specific table
    ''' </summary>
    ''' <param name="name">the table name</param>
    ''' <returns></returns>
    Public Function CreateModel(name As String) As Model
        Return New Model(name, mysqli)
    End Function

End Class
