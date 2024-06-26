﻿#Region "Microsoft.VisualBasic::aaf17249fc0f8a16ef3d977984103101, src\mysqli\LibMySQL\Extensions\DumpTaskRunner.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 146
    '    Code Lines: 106
    ' Comment Lines: 14
    '   Blank Lines: 26
    '     File Size: 4.88 KB


    ' Class DumpTaskRunner
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DumpRows
    ' 
    '     Sub: Close, (+2 Overloads) Dispose, OpenFile
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Public Class DumpTaskRunner : Implements IDisposable

    ReadOnly EXPORT As String
    ReadOnly bufferSize As Integer
    ReadOnly echo As Boolean
    ReadOnly AI As Boolean
    ReadOnly truncate As Boolean
    ReadOnly singleTransaction As Boolean

    Private disposedValue As Boolean

    Sub New(EXPORT$,
            bufferSize%,
            singleTransaction As Boolean,
            echo As Boolean,
            AI As Boolean,
            truncate As Boolean)

        Me.EXPORT = EXPORT
        Me.bufferSize = bufferSize
        Me.echo = echo
        Me.AI = AI
        Me.truncate = truncate
        Me.singleTransaction = singleTransaction
    End Sub

    Dim writer As New Dictionary(Of String, StreamWriter)
    Dim buffer As New Dictionary(Of String, (schema As Table, bufferData As List(Of MySQLTable)))
    Dim tableNames As New Dictionary(Of Type, String)
    Dim DBName$ = ""

    Private Sub OpenFile(tblName As String, type As Type)
        buffer(tblName) = (New Table(type), New List(Of MySQLTable))
        DBName = buffer(tblName).schema.Database

        With $"{EXPORT}/{DBName}_{tblName}.sql".OpenWriter
            If Not singleTransaction Then
                Call .WriteLine(OptionsTempChange.Replace("%s", DBName))

                If echo Then
                    Call ("  --> " & DirectCast(.BaseStream, FileStream).Name).__INFO_ECHO
                End If
            End If

            If truncate Then
                Call .WriteLine($"TRUNCATE `{DBName}`.`{tblName}`;")
            End If

            Call .LockTable(tblName)
            Call .WriteLine()
            Call writer.Add(tblName, .ByRef)
        End With
    End Sub

    Public Function DumpRows(source As IEnumerable(Of MySQLTable)) As String
        For Each obj As MySQLTable In source.SafeQuery
            Dim type As Type = obj.GetType
            Dim tblName$

            If Not tableNames.ContainsKey(type) Then
                tableNames(type) = TableName.GetTableName(type)
            End If

            tblName = tableNames(type)

            If Not writer.ContainsKey(tblName) Then
                Call OpenFile(tblName, type)
            End If

            With buffer(tblName)
                If .bufferData = bufferSize Then
                    Call .bufferData.DumpBlock(.schema, writer(tblName), AI:=AI)
                    Call .bufferData.Clear()

                    If echo Then
                        Call $"write_buffer({tblName})".__DEBUG_ECHO
                    End If
                Else
                    Call .bufferData.Add(obj)
                End If
            End With
        Next

        Return DBName
    End Function

    Private Sub Close()
        For Each buf In buffer.EnumerateTuples
            With buf.obj
                Call .bufferData.DumpBlock(.schema, writer(buf.name), AI:=AI)
            End With

            With writer(buf.name)
                Call .WriteLine()
                Call .UnlockTable(buf.name)

                If Not singleTransaction Then
                    Call .WriteLine(OptionsRestore, Now.ToString)
                End If
            End With
        Next

        For Each handle As StreamWriter In writer.Values
            Call handle.Flush()
            Call handle.Close()
            Call handle.Dispose()
        Next
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                Call Close()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    ''' <summary>
    ''' Call this method for commit the remaining data to 
    ''' the local transaction dump sql file
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
