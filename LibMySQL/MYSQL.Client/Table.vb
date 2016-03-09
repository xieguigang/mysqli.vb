Public Class Table(Of TTable As Oracle.LinuxCompatibility.MySQL.SQLTable)

    Public ReadOnly Property MySQL As MySQL

    Sub New(uri As ConnectionUri)
        MySQL = New MySQL
        Call MySQL.Connect(uri)
    End Sub

    Sub New(Engine As MySQL)
        Me.MySQL = Engine
    End Sub

    Public Overrides Function ToString() As String
        Return MySQL.ToString
    End Function

#Region "Operator"

    Public Shared Narrowing Operator CType(table As Table(Of TTable)) As MySQL
        Return table.MySQL
    End Operator

    Public Shared Widening Operator CType(Engine As MySQL) As Table(Of TTable)
        Return New Table(Of TTable)(Engine)
    End Operator

    Public Shared Operator <=(table As Table(Of TTable), SQL As String) As TTable()
        If String.Equals(SQL.Trim.Split.First, "SELECT", StringComparison.OrdinalIgnoreCase) Then
            Return table.MySQL.Query(Of TTable)(SQL)
        Else
            If table.MySQL.Execute(SQL) > 0 Then
                Return New TTable() {}
            Else
                Return Nothing
            End If
        End If
    End Operator

    Public Shared Operator >=(table As Table(Of TTable), SQL As String) As TTable()
        Return table <= SQL
    End Operator

    ''' <summary>
    ''' 添加新的记录
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="insertRow"></param>
    ''' <returns></returns>
    Public Shared Operator +(table As Table(Of TTable), insertRow As TTable) As Boolean
        Return table.MySQL.ExecInsert(insertRow)
    End Operator

    Public Shared Operator -(table As Table(Of TTable), deleteRow As TTable) As Boolean
        Return table.MySQL.ExecDelete(deleteRow)
    End Operator

    Public Shared Operator ^(table As Table(Of TTable), updateRow As TTable) As Boolean
        Return table.MySQL.ExecUpdate(updateRow)
    End Operator

    ''' <summary>
    ''' 查询单条记录
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="SQL"></param>
    ''' <returns></returns>
    Public Shared Operator <(table As Table(Of TTable), SQL As String) As TTable
        Return table.MySQL.ExecuteScalar(Of TTable)(SQL)
    End Operator

    Public Shared Operator >(table As Table(Of TTable), WHERE As String) As TTable
        Return table.MySQL.ExecuteScalarAuto(Of TTable)(WHERE)
    End Operator
#End Region

End Class
