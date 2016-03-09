﻿Imports System.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace Reflection.Schema

    ''' <summary>
    ''' The table schema that we define on the custom attributes of a Class.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Table

        Protected Friend _databaseFields As Dictionary(Of String, Field)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TableName As String
        Public Property UniqueFields As New List(Of String)
        ''' <summary>
        ''' 主键，主要根据这个属性来生成WHERE条件
        ''' </summary>
        ''' <returns></returns>
        Public Property PrimaryFields As New List(Of String)
        Public ReadOnly Property Fields As Field()
            Get
                Return _databaseFields.Values.ToArray
            End Get
        End Property

        Public ReadOnly Property lstFieldName As String()
            Get
                Return _databaseFields.ToArray(Function(x) x.Value.FieldName)
            End Get
        End Property

        ''' <summary>
        ''' 这张数据表所在数据库的名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Database As String

        ''' <summary>
        ''' 查找不到则返回空值
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        Public ReadOnly Property DatabaseField(FieldName As String) As Field
            Get
                If _databaseFields.ContainsKey(FieldName) Then
                    Return _databaseFields(FieldName)
                Else
                    Call $"{TableName} => {FieldName} is not exists....".__DEBUG_ECHO
                    Return Nothing
                End If
            End Get
        End Property

        Public Property Comment As String

        ''' <summary>
        ''' The index field when execute the update/delete sql.
        ''' </summary>
        ''' <remarks>Long/Integer first, then the Text is second, the primary key is the last consideration.</remarks>
        Public Property Index As String
        Public Property IndexProperty As PropertyInfo
        Public Property SchemaType As System.Type

        Protected Friend Sub New()
        End Sub

        Sub New(Schema As Type)
            Call Me.__getSchema(Schema)
            SchemaType = Schema
        End Sub

        Public Function GetPrimaryKeyFields() As Field()
            Return PrimaryFields.ToArray(Function(name) DatabaseField(name))
        End Function

        Public Overrides Function ToString() As String
            Return TableName
        End Function

        Private Sub __getSchema(Schema As Type)
            Dim ItemProperty = Schema.GetProperties
            Dim Field As Field
            Dim Index2 As String = String.Empty
            Dim IndexProperty2 As PropertyInfo = Nothing

            TableName = GetTableName(Schema)

            For i As Integer = 0 To ItemProperty.Length - 1
                Field = ItemProperty(i) 'Parse the field attribute from the ctype operator, this property must have a DatabaseField custom attribute to indicate that it is a database field.

                If Field Is Nothing Then Continue For

                Call _databaseFields.Add(Field.FieldName, Field)

                If Field.PrimaryKey Then
                    PrimaryFields.Add(Field.FieldName)
                End If

                If Field.Unique Then
                    UniqueFields.Add(Field.FieldName)

                    If CommonExtension.Numerics.IndexOf(Field.DataType) > -1 AndAlso Field.PrimaryKey Then
                        Index = Field.FieldName
                        IndexProperty = ItemProperty(i)
                    End If

                    Index2 = Field.FieldName
                    IndexProperty2 = ItemProperty(i)
                End If
            Next

            Call __indexing(Index2, IndexProperty2, ItemProperty) 'If we can not found a index from its unique field, then we indexing from its primary key.
        End Sub

        ''' <summary>
        ''' Indexing from the primary key attributed field.
        ''' </summary>
        ''' <param name="Index2"></param>
        ''' <param name="Indexproperty2"></param>
        ''' <param name="ItemProperty"></param>
        ''' <remarks></remarks>
        Private Sub __indexing(Index2 As String, Indexproperty2 As PropertyInfo, ItemProperty As PropertyInfo())
            If Not String.IsNullOrEmpty(Index) Then Return 'If we have a unique index from previous work, then this operation is no more needed. 

            If String.IsNullOrEmpty(Index2) Then
                If PrimaryFields.Count > 0 Then      'Indexing from the primary key, the primary key may be more than one
                    Dim LQuery As String() = (From Field In Fields
                                              Where (Field.PrimaryKey AndAlso CommonExtension.Numerics.IndexOf(Field.DataType) > -1)
                                              Select Field.FieldName).ToArray
                    If LQuery.Length > 0 Then
                        Index = LQuery(Scan0)
                    Else
                        Index = PrimaryFields.First
                    End If

                    Dim PQuery As Generic.IEnumerable(Of PropertyInfo) = From iPinfo As PropertyInfo In ItemProperty
                                                                         Let p As DatabaseField = GetAttribute(Of DatabaseField)(iPinfo)
                                                                         Where Not p Is Nothing AndAlso String.Equals(p.Name, Index)
                                                                         Select iPinfo '
                    IndexProperty = PQuery.First
                End If
            Else
                Index = Index2   'The data type of this index field is a text type. 
                IndexProperty = Indexproperty2
            End If
        End Sub

        Private Function GetTableName(Type As Type) As String
            Dim Attributes = Type.CustomAttributes

            For Each attr As CustomAttributeData In Attributes
                If String.Equals(attr.AttributeType.Name, "TableName") Then
                    Return attr.ConstructorArguments.First.Value
                End If
            Next

            Return String.Empty
        End Function

        Public Shared Widening Operator CType(Schema As Type) As Table
            Return New Table(Schema)
        End Operator
    End Class
End Namespace
