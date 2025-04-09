Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace MySqlBuilder

    Public Class CommitInsert

        ReadOnly model As Model
        ReadOnly cache As New List(Of (name$, val As FieldAssert)())
        ReadOnly delayed As Boolean = False

        Sub New(model As Model, delayed As Boolean)
            Me.model = model
            Me.delayed = delayed
        End Sub

        Public Function commit() As Boolean
            Dim names As String() = fieldKeys()
            Dim vals As String() = fieldValues(names).ToArray
            Dim schema As Table = model.schema
            Dim sql As String = $"INSERT {If(delayed, "DELAYED", "")} INTO `{schema.Database}`.`{schema.TableName}` ({names.JoinBy(", ")}) VALUES {vals.JoinBy(", ")};"
            model.chain.m_getLastMySql = sql
            Dim result = model.mysql.Execute(sql)
            Return result > 0
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

        Private Function fieldKeys() As String()
            Return cache.IteratesALL.Select(Function(a) a.name).Distinct.ToArray
        End Function

        ''' <summary>
        ''' INSERT INTO
        ''' </summary>
        ''' <param name="fields"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub add(ParamArray fields As FieldAssert())
            Call cache.Add(fields.Select(Function(a) (a.GetSafeName, a)).ToArray)
        End Sub

    End Class
End Namespace