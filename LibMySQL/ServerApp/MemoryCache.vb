#Region "Microsoft.VisualBasic::96f314da0261d7dae22a12b5eaf6d8ed, ..\mysqli\LibMySQL\ServerApp\MemoryCache.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri
Imports p = Microsoft.VisualBasic.Parallel.ParallelExtension

Namespace ServerApp

    ''' <summary>
    ''' For the biological database, due to the reason of very few UPDATE/INSERT query but with 
    ''' more active SELECT query. So that a memory cache is important for the improvements 
    ''' on the database query performance.
    ''' (这个缓存对象对于不经常更新数据，即只执行SELECT查询操作的数据表非常有效
    ''' 至少对于生物信息学的数据库而言，由于更新数据很缓慢，大部分时候都只是执行SELECT查询操作，所以非常好用)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class MemoryCache(Of T As MySQLTable)

#Region "Internal Cache"
        ReadOnly mysqli As New MySqli
        ReadOnly __cache As Dictionary(Of String, T())

        ''' <summary>
        ''' 这个索引是由用户手动指定的
        ''' </summary>
        ReadOnly __index As PropertyInfo()
        ReadOnly __schema As New Table(GetType(T))
        ''' <summary>
        ''' ``propertyName -> FieldName``
        ''' </summary>
        ReadOnly __fields As Dictionary(Of String, String)
#End Region

#Region "Auto Memory Cleanup"

        ''' <summary>
        ''' 用于计算内存的使用量，进行自动缓存清除的算法
        ''' </summary>
        ReadOnly sizeOf As New MemorySize(Of T)

        ''' <summary>
        ''' 会对查询进行统计，定时进行排序，这个哈希表的键名和<see cref="__cache"/>哈希表的键名是一样的
        ''' </summary>
        ReadOnly hits As New Dictionary(Of String, ULong)

        ''' <summary>
        ''' 估算出来的当前这个缓存对象的内存占用量
        ''' </summary>
        Dim currentSize&
        ''' <summary>
        ''' 当这个缓存对象的内存占用超过这个阈值之后就会开始进行自动内存清理
        ''' 将<see cref="hits"/>哈希表之中的较少hits的对象删除
        ''' </summary>
        ''' <returns></returns>
        Public Property MemorySizeThreshold As Long

        Dim current_n%
        ''' <summary>
        ''' 默认是执行了2048次查询之后进行一次缓存内存计算，判断是否需要进行内存清理
        ''' </summary>
        Public Property MeasureCounts As Integer = 2048
#End Region

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cnn"></param>
        ''' <param name="index$">属性名列表，请尽量使用``NameOf``操作符来获取</param>
        Sub New(cnn As ConnectionUri, ParamArray index$())
            If (mysqli <= cnn) = -1.0R Then
                Throw New Exception("No avalaible mysql connection!")
            End If

            __index = GetType(T) _
                .GetProperties(PublicProperty) _
                .Where(Function([property]) index.IndexOf([property].Name) > -1) _
                .ToArray
        End Sub

        ''' <summary>
        ''' 进行hit次数的统计以及累加查询次数
        ''' </summary>
        ''' <param name="key$"></param>
        Private Sub push(key$)
            SyncLock hits
                If hits.ContainsKey(key) Then
                    hits(key) += 1
                Else
                    hits.Add(key, 1)
                End If

                current_n += 1

                If current_n = MeasureCounts Then
                    current_n = 0
                    p.RunTask(AddressOf cleanup).Start()
                End If
            End SyncLock
        End Sub

        ''' <summary>
        ''' Auto cleanup cache memory
        ''' </summary>
        Private Sub cleanup()
            Static running As Boolean = False

            If running Then
                Return
            Else
                running = True
            End If

            ' 先计算出缓存的内存占用大小
            currentSize = __cache _
                .Values _
                .IteratesALL _
                .Select(AddressOf sizeOf.MeasureSize) _
                .Sum

            If currentSize >= MemorySizeThreshold Then

                ' 按照次数升序排序取1/3
                SyncLock hits
                    With hits _
                            .OrderBy(Function(l) l.Value) _
                            .Take(hits.Count / 3) _
                            .ToArray

                        .DoEach(Sub(key) Call __cache.Remove(key.Key))
                        ' 必须要将hits之中的键也移除，否则后面会出现很多的key存在
                        ' 于hits之中但是cache表中不存在的尴尬情况
                        .DoEach(Sub(key) Call hits.Remove(key.Key))
                    End With
                End SyncLock

                ' 重新计算内存占用大小
                currentSize = __cache _
                    .Values _
                    .IteratesALL _
                    .Select(AddressOf sizeOf.MeasureSize) _
                    .Sum
            End If
        End Sub

        ''' <summary>
        ''' 对从数据库之中读取回来的对象建立缓存之中的索引
        ''' </summary>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Private Function __indexKey(o As T) As String
            Dim keys$() = __index _
                .Select(Function(prop) prop.GetValue(o)) _
                .Select(Function(s) If(s Is Nothing, "null", s.ToString)) _
                .ToArray
            Return String.Join("--", keys)
        End Function

        Private Function __indexKey(o As NamedValue(Of String)()) As String
            Dim keys$() = __index _
                .Select(Function(prop) o.Take(prop.Name).Value) _
                .Select(Function(s) If(s Is Nothing, "null", s)) _
                .ToArray
            Return String.Join("--", keys)
        End Function

        Public Function Query(args As NamedValue(Of String)(), Optional forceUpdate As Boolean = False) As T()
            Dim key$ = __indexKey(args)

            Call push(key)

            If Not forceUpdate AndAlso __cache.ContainsKey(key) Then
                Return Clone(__cache(key))
            Else
                Dim SQL$ = $"SELECT * FROM `{__schema.TableName}` WHERE {__where(args)};"
                Dim data As T() = Query(SQL, forceUpdate)  ' SQL会被用作为key缓存一次，这样子下次即使直接用SQL查询的话，只要有hit就可以直接从缓存之中读取了
                __cache(key) = Clone(data)    ' 当前的key也会被缓存一次
                Return data
            End If
        End Function

        ''' <summary>
        ''' SQL语句直接用作为key
        ''' </summary>
        ''' <param name="SQL$"></param>
        ''' <returns></returns>
        Public Function Query(SQL$, Optional forceUpdate As Boolean = False) As T()
            Call push(SQL)

            If Not forceUpdate AndAlso __cache.ContainsKey(SQL) Then
                Return Clone(__cache(SQL))
            Else
                Dim data As T() = mysqli.Query(Of T)(SQL)
                __cache(SQL) = Clone(data)
                Return data
            End If
        End Function

        Private Function __where(args As NamedValue(Of String)()) As String
            Dim out$() = args.Select(Function(x) $"`{__fields(x.Name)}` = '{x.Value}'")
            Return String.Join(" AND ", out)
        End Function

        ''' <summary>
        ''' 当不进行Clone操作的话，会由于Class引用的缘故导致Cache的数据也被修改，所以在这里使用这个函数来避免这种情况的发生
        ''' </summary>
        ''' <param name="array"></param>
        ''' <returns></returns>
        Private Shared Function Clone(array As T()) As T()
            Return array _
                .Select(Function(o) DirectCast(o.Copy, T)) _
                .ToArray
        End Function

        Public Function ExecuteScalar(args As NamedValue(Of String)(), Optional forceUpdate As Boolean = False) As T
            Dim key$ = __indexKey(args)
            Dim o As T

            If Not forceUpdate AndAlso __cache.ContainsKey(key) Then
                ' 当数据库不存在在这条记录的时候会是空集合
                o = __cache(key).FirstOrDefault
            Else
                o = ExecuteScalar(
                    $"SELECT * FROM `{__schema.TableName}` WHERE {__where(args)} LIMIT 1;",
                    forceUpdate)
            End If

            Call push(key)

            If o Is Nothing Then
                Return Nothing
            Else
                Return o.Copy
            End If
        End Function

        Public Function ExecuteScalar(SQL$, Optional forceUpdate As Boolean = False) As T
            Call push(SQL)

            If Not forceUpdate AndAlso __cache.ContainsKey(SQL) Then
                Return Clone(__cache(SQL)).FirstOrDefault
            Else
                Dim data As T = mysqli.ExecuteScalar(Of T)(SQL)
                If data Is Nothing Then
                    __cache(SQL) = {}
                Else
                    __cache(SQL) = {
                        DirectCast(data.Copy, T)
                    }
                End If

                Return data
            End If
        End Function
    End Class
End Namespace
