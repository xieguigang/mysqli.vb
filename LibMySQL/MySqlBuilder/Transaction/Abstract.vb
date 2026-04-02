Namespace MySqlBuilder

    ''' <summary>
    ''' commit operation for the insert operation:
    ''' 
    ''' 1. <see cref="CommitInsert"/>
    ''' 2. <see cref="CommitTransaction"/>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' this class is used to commit the insert operation
    ''' </remarks>
    Public Interface IDataCommitOperation

        Sub Commit()

    End Interface

    Public Interface IInsert

        ''' <summary>
        ''' implements of the insert into
        ''' </summary>
        ''' <param name="fields"></param>
        Function add(ParamArray fields As FieldAssert()) As Boolean

    End Interface

    ''' <summary>
    ''' the abstract model for insert into sql
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IInsertModel(Of T As IInsertModel(Of T)) : Inherits IInsert

        ''' <summary>
        ''' insert delayed into
        ''' </summary>
        ''' <returns></returns>
        Function delayed() As T
        ''' <summary>
        ''' insert ignore into
        ''' </summary>
        ''' <returns></returns>
        Function ignore() As T
        ''' <summary>
        ''' insert into
        ''' </summary>
        ''' <returns></returns>
        Function clearOption() As T

    End Interface
End Namespace