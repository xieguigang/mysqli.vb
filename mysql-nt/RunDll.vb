﻿#Region "Microsoft.VisualBasic::84fd93e2a852e501b49fcf0458b87f81, mysql-nt\RunDll.vb"

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

    ' Module RunDll
    ' 
    '     Function: StartTinyServices
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' API for rundll32.exe
''' </summary>
''' 
<PackageNamespace("Mysqld", Publisher:="Oracle Corp")>
<RunDllEntryPoint("MySQL")>
Public Module RunDll

    <ExportAPI("--start", Info:="Start the embedded tiny mysqld services.")>
    Public Function StartTinyServices(args As CommandLine) As Integer
        Try
            Call New TinyServer().Start()
            Return 0
        Catch ex As Exception
            Return -1
        End Try
    End Function
End Module
