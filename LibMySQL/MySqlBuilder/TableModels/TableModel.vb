
Imports System.Runtime.CompilerServices
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes

Namespace MySqlBuilder

    Public Class TableModel(Of T As {New, MySQLTable}) : Inherits Model

        Sub New(mysqli As MySqli)
            Call MyBase.New(TableName.GetTableName(Of T), mysqli)
        End Sub

        Public Function find_object(ParamArray where As FieldAssert()) As T
            Return Me.where(where).find(Of T)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function async() As AsyncModelTable
            Return New AsyncModelTable(Me)
        End Function

    End Class
End Namespace