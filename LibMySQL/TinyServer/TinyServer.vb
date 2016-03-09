﻿Imports Microsoft.VisualBasic.CommandLine

''' <summary>
''' Only works on Windows
''' </summary>
Public Class TinyServer

    Dim Resource As Microsoft.VisualBasic.CommandLine.CliResCommon

    Public ReadOnly Property libmySQL As String
        Get
            Return Resource.TryRelease(NameOf(My.Resources.libmySQL), "dll")
        End Get
    End Property

    Public ReadOnly Property mysql As String
        Get
            Return Resource.TryRelease(NameOf(My.Resources.mysql))
        End Get
    End Property

    Public ReadOnly Property mysqladmin As String
        Get
            Return Resource.TryRelease(NameOf(My.Resources.mysqladmin))
        End Get
    End Property

    Public ReadOnly Property mysqld As String
        Get
            Return Resource.TryRelease(NameOf(My.Resources.mysqld))
        End Get
    End Property

    Sub New()
        Resource = New CliResCommon(FileIO.FileSystem.GetParentPath(System.Windows.Forms.Application.ExecutablePath) & "/MySQL.Tiny/", GetType(My.Resources.Resources))
    End Sub

    ''' <summary>
    ''' Run server process, the thread will be stuck at this function until the server is stop.
    ''' </summary>
    Public Sub Start()
        Dim Cli = New Microsoft.VisualBasic.CommandLine.IORedirectFile(mysqld)
        Call Cli.Run()
    End Sub

End Class
