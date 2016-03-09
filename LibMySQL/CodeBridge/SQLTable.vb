''' <summary>
''' MYSQL之中的一个数据表的抽象描述接口
''' </summary>
Public MustInherit Class SQLTable

    Public MustOverride Function GetInsertSQL() As String
    Public MustOverride Function GetUpdateSQL() As String
    Public MustOverride Function GetDeleteSQL() As String
    ''' <summary>
    ''' 如果已经存在了一条相同主键值的记录，则删除它然后在插入更新值；
    ''' 假若不存在，则直接插入新数据，这条命令几乎等价于<see cref="GetInsertSQL"/>命令，所不同的是这个会自动处理旧记录，可能会不安全，
    ''' 因为旧记录可能会在你不知情的情况下被意外的更新了；
    ''' 并且由于需要先判断记录是否存在，执行的速度也比直接的Insert操作要慢一些，大批量数据插入不建议这个操作
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function GetReplaceSQL() As String

    Public Overrides Function ToString() As String
        Return GetInsertSQL()
    End Function
End Class
