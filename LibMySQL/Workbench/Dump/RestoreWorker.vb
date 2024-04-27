﻿#Region "Microsoft.VisualBasic::e9f5632ac5dfdf87bb5f373346f66710, G:/graphQL/src/mysqli/LibMySQL//Workbench/Dump/RestoreWorker.vb"

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

Imports Microsoft.VisualBasic.Linq.Extensions
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace Workbench.Dump

    Public Class RestoreWorker

        Public ReadOnly Property MySQL As MySqli

        Sub New(uri As ConnectionUri)
            MySQL = New MySqli(uri)
        End Sub

        ''' <summary>
        ''' 会需要动态编译
        ''' </summary>
        ''' <param name="dumpDir"></param>
        ''' <returns></returns>
        Public Function ImportsData(dumpDir As String, Optional dbName As String = "") As Boolean
            Dim SQLs As String() = FileIO.FileSystem.GetFiles(
                dumpDir,
                FileIO.SearchOption.SearchTopLevelOnly,
                "*.sql").Select(Function(file) FileIO.FileSystem.ReadAllText(file)).ToArray

            If String.IsNullOrEmpty(dbName) Then
                dbName = FileIO.FileSystem.GetDirectoryInfo(dumpDir).Name
                dbName = dbName.NormalizePathString
            End If

            Call MySQL.Execute($"CREATE SCHEMA `{dbName}` ;")

            '   Dim Tables = SQLs.Select(Of KeyValuePair)(Function(sql) CodeGenerator.GenerateClass(sql, ""))

            Throw New NotImplementedException
        End Function
    End Class
End Namespace
