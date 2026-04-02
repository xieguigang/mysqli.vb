Namespace MySqlBuilder

    ''' <summary>
    ''' AsyncModelTable is a wrapper for Model class to provide asynchronous query and data manipulation functions,
    ''' it is designed for use in async/await programming pattern, and all of its methods will return Task(Of T) type, 
    ''' so you can use await keyword to get the result of the query or data manipulation operation.
    ''' </summary>
    Public Class AsyncModelTable

        ReadOnly table As Model

        Sub New(table As Model)
            Me.table = table
        End Sub

        ''' <summary>
        ''' WHERE
        ''' </summary>
        ''' <param name="q"></param>
        ''' <returns></returns>
        Public Function where(q As String) As AsyncModelTable
            Return New AsyncModelTable(table.where(q))
        End Function

        ''' <summary>
        ''' WHERE
        ''' </summary>
        ''' <returns></returns>
        Public Function where(list As IEnumerable(Of FieldAssert)) As AsyncModelTable
            Return New AsyncModelTable(table.where(list))
        End Function

        ''' <summary>
        ''' WHERE
        ''' </summary>
        ''' <param name="asserts"></param>
        ''' <returns></returns>
        Public Function where(ParamArray asserts As FieldAssert()) As AsyncModelTable
            Return New AsyncModelTable(table.where(asserts))
        End Function

        Public Function group_by(ParamArray fields As String()) As AsyncModelTable
            Return New AsyncModelTable(table.group_by(fields))
        End Function

        Public Function having(ParamArray asserts As FieldAssert()) As AsyncModelTable
            Return New AsyncModelTable(table.having(asserts))
        End Function

        Public Function [and](q As String) As AsyncModelTable
            Return New AsyncModelTable(table.and(q))
        End Function

        Public Function [and](ParamArray asserts As FieldAssert()) As AsyncModelTable
            Return New AsyncModelTable(table.and(asserts))
        End Function

        Public Function [or](q As String) As AsyncModelTable
            Return New AsyncModelTable(table.or(q))
        End Function

        Public Function [or](ParamArray asserts As FieldAssert()) As AsyncModelTable
            Return New AsyncModelTable(table.or(asserts))
        End Function

        Public Function find(Of T As {New, Class})(ParamArray fields As String()) As System.Threading.Tasks.Task(Of T)
            Return Task.Run(Function() table.find(Of T)(fields))
        End Function

        Public Function find(ParamArray fields As String()) As System.Threading.Tasks.Task(Of Dictionary(Of String, Object))
            Return Task.Run(Function() table.find(fields))
        End Function

        Public Function aggregate(Of T)(exp As String) As System.Threading.Tasks.Task(Of T)
            Return Task.Run(Function() table.aggregate(Of T)(exp))
        End Function

        Public Function count() As System.Threading.Tasks.Task(Of Long)
            Return Task.Run(Function() table.count())
        End Function

        Public Function [select](Of T As {New, Class})(ParamArray fields As String()) As System.Threading.Tasks.Task(Of T())
            Return Task.Run(Function() table.select(Of T)(fields))
        End Function

        Public Function [select](ParamArray fields As String()) As System.Threading.Tasks.Task(Of System.Data.DataTableReader)
            Return Task.Run(Function() table.select(fields))
        End Function

        ''' <summary>
        ''' project a field column as vector from the database
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="field">the field name to project as vector</param>
        ''' <returns></returns>
        Public Function project(Of T As IComparable)(field As String) As System.Threading.Tasks.Task(Of T())
            Return Task.Run(Function() table.project(Of T)(field))
        End Function

        ''' <summary>
        ''' UPDATE
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function save(ParamArray fields As FieldAssert()) As System.Threading.Tasks.Task(Of Boolean)
            Return Task.Run(Function() table.save(fields))
        End Function

        ''' <summary>
        ''' INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function add(ParamArray fields As FieldAssert()) As System.Threading.Tasks.Task(Of Boolean)
            Return Task.Run(Function() table.add(fields))
        End Function

        ''' <summary>
        ''' DELETE FROM
        ''' </summary>
        ''' <returns></returns>
        Public Function delete() As System.Threading.Tasks.Task(Of Boolean)
            Return Task.Run(Function() table.delete())
        End Function

        ''' <summary>
        ''' LIMIT m,n
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function limit(m As Integer, Optional n As Integer? = Nothing) As AsyncModelTable
            Return New AsyncModelTable(table.limit(m, n))
        End Function

        ''' <summary>
        ''' LEFT JOIN
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Function left_join(table As String) As AsyncModelTable
            Return New AsyncModelTable(Me.table.left_join(table))
        End Function

        ''' <summary>
        ''' LEFT JOIN ON
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        Public Function [on](ParamArray fields As FieldAssert()) As AsyncModelTable
            Return New AsyncModelTable(table.on(fields))
        End Function

        ''' <summary>
        ''' ORDER BY
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' fields parameter data could be field name or an expression
        ''' </remarks>
        Public Function order_by(ParamArray fields As String()) As AsyncModelTable
            Return New AsyncModelTable(table.order_by(fields))
        End Function

        ''' <summary>
        ''' ORDER BY DESC
        ''' </summary>
        ''' <param name="fields"></param>
        ''' <param name="desc"></param>
        ''' <returns></returns>
        Public Function order_by(fields As String(), desc As Boolean) As AsyncModelTable
            Return New AsyncModelTable(table.order_by(fields, desc))
        End Function

        ''' <summary>
        ''' ORDER BY DESC
        ''' </summary>
        ''' <param name="field"></param>
        ''' <param name="desc"></param>
        ''' <returns></returns>
        Public Function order_by(field As String, desc As Boolean) As AsyncModelTable
            Return New AsyncModelTable(table.order_by(field, desc))
        End Function

        ''' <summary>
        ''' DISTINCT
        ''' </summary>
        ''' <returns></returns>
        Public Function distinct() As AsyncModelTable
            Return New AsyncModelTable(table.distinct)
        End Function
    End Class
End Namespace