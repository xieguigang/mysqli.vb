#Region "Microsoft.VisualBasic::5a9f7806e0fc71a538087cd587ea21ea, G:/graphQL/src/mysqli/LibMySQL//Reflection/SQL/Insert.vb"

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

    '   Total Lines: 50
    '    Code Lines: 24
    ' Comment Lines: 19
    '   Blank Lines: 7
    '     File Size: 1.78 KB


    '     Class Insert
    ' 
    '         Function: Generate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.SQL

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Example SQL:
    ''' 
    ''' INSERT INTO `TableName` (`Field1`, `Field2`, `Field3`) VALUES ('1', '1', '1');
    ''' </remarks>
    Public Class Insert(Of Schema) : Inherits SQL

        ''' <summary>
        ''' INSERT INTO `TableName` (`Field1`, `Field2`, `Field3`, ...) VALUES ('{0}', '{1}', '{2}', ...);
        ''' </summary>
        ''' <remarks></remarks>
        Friend InsertSQL As String

        ''' <summary>
        ''' Generate the INSERT sql command of the instance of the specific type of 'Schema'.
        ''' (生成特定的'Schema'数据类型实例的 'INSERT' sql命令)
        ''' </summary>
        ''' <param name="value">The instance to generate this command of type 'Schema'</param>
        ''' <returns>INSERT sql text</returns>
        ''' <remarks></remarks>
        Public Function Generate(value As Schema) As String
            Dim valuesbuffer As List(Of String) = New List(Of String)
            Dim str$

            For Each field In MyBase._schemaInfo.Fields
                str = field _
                    .PropertyInfo _
                    .GetValue(value, Nothing) _
                    .ToString
                Call valuesbuffer.Add(str)
            Next

            Return String.Format(InsertSQL, valuesbuffer.ToArray)
        End Function

        Public Shared Widening Operator CType(schema As Table) As Insert(Of Schema)
            Return New Insert(Of Schema) With {
                ._schemaInfo = schema,
                .InsertSQL = GenerateInsertSql(schema)
            }
        End Operator
    End Class
End Namespace
