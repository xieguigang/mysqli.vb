Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder

    Public Class CommitInsert : Implements IModel

        ReadOnly model As Model
        ReadOnly blocks As New List(Of Block)
        ReadOnly delayed As Boolean = False

        Dim maxBlockSize As Integer = 1024

        Public ReadOnly Property lastBlockSize As Integer
            Get
                If blocks.Count = 0 Then
                    Return 0
                End If
                Return blocks.Last.cache.Count
            End Get
        End Property

        Private Class Block

            Public ReadOnly cache As New List(Of (name$, val As FieldAssert)())

            ReadOnly commit As CommitInsert

            Sub New(commit As CommitInsert)
                Me.commit = commit
            End Sub

            Public Function commit_sql() As String
                Dim names As String() = fieldKeys()
                Dim vals As String() = fieldValues(names).ToArray
                Dim schema As Table = commit.model.schema
                Dim sql As String = $"INSERT {If(commit.delayed, "DELAYED", "")} INTO `{schema.Database}`.`{schema.TableName}` ({names.JoinBy(", ")}) VALUES {vals.JoinBy(", ")};"

                Return sql
            End Function

            Private Iterator Function fieldValues(names As String()) As IEnumerable(Of String)
                For Each row As (String, FieldAssert)() In cache
                    Dim hash = row.ToDictionary(Function(a) a.Item1, Function(a) a.Item2)
                    Dim vals As New List(Of String)

                    For Each name As String In names
                        If hash.ContainsKey(name) Then
                            Call vals.Add(hash(name).val)
                        Else
                            Call vals.Add("NULL")
                        End If
                    Next

                    Yield $"({vals.JoinBy(", ")})"
                Next
            End Function

            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Private Function fieldKeys() As String()
                Return cache.IteratesALL.Select(Function(a) a.name).Distinct.ToArray
            End Function
        End Class

        Sub New(model As Model, delayed As Boolean, Optional maxBlockSize As Integer = 1024)
            Me.model = model
            Me.delayed = delayed
            Me.maxBlockSize = maxBlockSize
        End Sub

        ''' <summary>
        ''' commit the batch insert transaction
        ''' </summary>
        ''' <returns></returns>
        Public Function commit() As Boolean
            Dim sql As New StringBuilder
            Dim sql_str As String
            Dim result As Integer
            Dim ex As Exception = Nothing

            For Each block As Block In blocks
                Call sql.AppendLine(block.commit_sql)
            Next

            sql_str = sql.ToString
            model.chain.m_getLastMySql = sql_str
            result = model.mysql.CommitTransaction(sql_str, ex)

            Return result > 0
        End Function

        Public Sub commit(transaction As CommitTransaction)
            For Each block As Block In blocks
                transaction.add(block.commit_sql)
            Next
        End Sub

        ''' <summary>
        ''' INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub add(ParamArray fields As FieldAssert())
            If lastBlockSize > maxBlockSize Then
                blocks.Add(New Block(Me))
            End If

            Dim data = fields.Select(Function(a) (a.GetSafeName, a)).ToArray
            Dim block = blocks.Last

            Call block.cache.Add(data)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function field(name As String) As FieldAssert Implements IModel.field
            Return model.field(name)
        End Function
    End Class
End Namespace