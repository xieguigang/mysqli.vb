Imports System.Data.Common
Imports System.Text
Imports System.Data.Linq.Mapping
Imports System.Data.Entity.Core

<Table(Name:="table_schema")>
Public Class TableDump
    <Column(Name:="guid", DbType:="int", IsPrimaryKey:=True)> Public Property Guid As Integer
    <Column(Name:="table_name", DbType:="varchar(2048)")> Public Property TableName As String
    <Column(Name:="field", DbType:="varchar(2048)")> Public Property FieldName As String
    <Column(Name:="dbtype", DbType:="varchar(512)")> Public Property DbType As String
    ''' <summary>
    ''' 1或者0
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Column(Name:="is_primary_key", DbType:="int")> Public Property IsPrimaryKey As Integer
End Class