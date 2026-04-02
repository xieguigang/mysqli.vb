#Region "Microsoft.VisualBasic::e9f5632ac5dfdf87bb5f373346f66710, src\mysqli\LibMySQL\Workbench\Dump\RestoreWorker.vb"

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

'   Total Lines: 37
'    Code Lines: 22
' Comment Lines: 6
'   Blank Lines: 9
'     File Size: 1.28 KB


'     Class RestoreWorker
' 
'         Properties: MySQL
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ImportsData
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Text
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace Workbench

    Public Class RestoreWorker : Implements IDisposable

        Public ReadOnly Property uri As ConnectionUri

        Public Structure MySqlError

            Dim fileName As String
            Dim exitcode As Integer
            Dim message As String

            Public Overrides Function ToString() As String
                Return $"mysql_import({fileName.BaseName}), error={exitcode}:{message}"
            End Function

        End Structure

        ''' <summary>
        ''' the mysql.exe path, it is required for executing the sql dump file, 
        ''' and it is also required for dynamic compiling the sql dump file
        ''' to generate the class code.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property mysql As String

        ReadOnly passfile As String
        ReadOnly bufferSize As Integer = 4 * ByteSize.MB
        ReadOnly errors As New List(Of MySqlError)

        Private disposedValue As Boolean

        Private ReadOnly Property mysql_cmdl() As String
            Get
                Return $"--defaults-file=""{passfile}"" --protocol=tcp --host={uri.IPAddress} --user={uri.User} --port={uri.Port} --default-character-set=utf8 --comments --database={uri.Database}"
            End Get
        End Property

        Sub New(uri As ConnectionUri, mysql As String)
            Me.passfile = TempFileSystem.GetAppSysTempFile(".cnf", sessionID:=App.PID, prefix:="mysql_pass")
            Me.uri = uri
            Me.mysql = mysql.GetFullPath

            Call WritePassFile(uri, passfile)
        End Sub

        Private Shared Sub WritePassFile(uri As ConnectionUri, passfile As String)
            Call $"[client]{ASCII.LF}password=""{uri.Password}""".SaveTo(passfile)
        End Sub

        Private ReadOnly Property LastMysqlError As MySqlError
            Get
                Return errors.LastOrDefault
            End Get
        End Property

        ''' <summary>
        ''' 会需要动态编译
        ''' </summary>
        ''' <param name="dumpDir">
        ''' a data directory that contains multiple sql files, each file is a table dump, and the file name is the table name.
        ''' </param>
        Public Async Sub ImportsData(dumpDir As String)
            Dim procBar As ProgressBar = Nothing

            For Each sqlfile As String In TqdmWrapper.WrapIterator(ls - l - r - "*.sql" <= dumpDir, bar:=procBar)
                Call procBar.SetLabel($"imports '{sqlfile}'...")

                If Not Await ImportSqlFileAsync(sqlfile) Then
                    Call LastMysqlError.ToString.warning
                End If
            Next
        End Sub

        ''' <summary>
        ''' 跨平台导入大型 SQL 文件
        ''' </summary>
        Public Async Function ImportSqlFileAsync(sqlfile As String) As Task(Of Boolean)
            Dim procInfo As New ProcessStartInfo() With {
                .FileName = mysql,
                .Arguments = mysql_cmdl,
                .CreateNoWindow = True,
                .UseShellExecute = False
            }

            ' 3. 核心：重定向输入流
            procInfo.RedirectStandardInput = True
            procInfo.RedirectStandardError = True

            Return Await ShellMySqlImports(procInfo, sqlfile)
        End Function

        Private Async Function ShellMySqlImports(mysql As ProcessStartInfo, sqlfile As String) As Task(Of Boolean)
            Using proc As Process = Process.Start(mysql)
                ' 异步读取错误流，防止大文件写入时管道缓冲区满了导致死锁
                Dim errorTask = Task.Run(Function() proc.StandardError.ReadToEnd())
                Dim errorOutput As Value(Of String) = ""

                ' 4. 核心流式读取：使用 BaseStream 进行原始字节传输
                ' 这里的优势巨大：直接传输 Byte()，彻底避免了操作系统默认编码不同导致的乱码问题
                Using fs As New FileStream(sqlfile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize:=bufferSize)
                    Dim buffer(bufferSize) As Byte ' 4MB 缓冲区
                    Dim bytesRead As Integer
                    Dim stdin As Stream = proc.StandardInput.BaseStream

                    Do
                        bytesRead = fs.Read(buffer, 0, buffer.Length)

                        If bytesRead > 0 Then
                            ' 写入 MySQL 的标准输入流
                            Await stdin.WriteAsync(buffer, 0, bytesRead)
                        End If
                    Loop While bytesRead > 0
                End Using

                ' 告诉 MySQL 数据传输结束
                Call proc.StandardInput.Close()

                ' 等待进程退出并获取错误
                Await Task.Run(Sub() proc.WaitForExit())

                If proc.ExitCode <> 0 OrElse Not String.IsNullOrEmpty(errorOutput = Await errorTask) Then
                    Call errors.Add(New MySqlError With {
                        .fileName = sqlfile,
                        .exitcode = proc.ExitCode,
                        .message = errorOutput
                    })

                    Return False
                Else
                    Return True
                End If
            End Using
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call passfile.DeleteFile
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

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
