#Region "Microsoft.VisualBasic::fa1a9ecdc1b60055001775f033fcff7c, src\mysqli\LibMySQL\Reflection\SQL\SQL.vb"

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

'   Total Lines: 24
'    Code Lines: 14
' Comment Lines: 4
'   Blank Lines: 6
'     File Size: 645 B


'     Class SQL
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.SQL

    Public MustInherit Class SQL

        ''' <summary>
        ''' The table schema of the sql generation target.
        ''' </summary>
        ''' <remarks>(用于生成SQL语句的表结构属性)</remarks>
        Protected _schemaInfo As Table

        Protected Sub New()
        End Sub

        Public Sub New(SchemaInfo As Table)
            _schemaInfo = SchemaInfo
        End Sub

        Public Overrides Function ToString() As String
            Return _schemaInfo.ToString
        End Function

        Const ActionNotAllows$ = "Only allowes ""insert/update/delete/replace"" actions."

        ''' <summary>
        ''' 根据动作类型获取对应的SQL生成委托，消除重复的Select Case代码块
        ''' </summary>
        Public Shared Function GetSqlGenerator(sqlAction As SqlActions, Optional custom As Func(Of MySQLTable, String) = Nothing) As ISqlGenerateMethod
            If custom IsNot Nothing Then
                Return New ISqlGenerateMethod(AddressOf custom.Invoke)
            End If

            Select Case sqlAction
                Case SqlActions.Insert : Return New ISqlGenerateMethod(Function(o) o.GetInsertSQL)
                Case SqlActions.Update : Return New ISqlGenerateMethod(Function(o) o.GetUpdateSQL)
                Case SqlActions.Delete : Return New ISqlGenerateMethod(Function(o) o.GetDeleteSQL)
                Case SqlActions.Replace : Return New ISqlGenerateMethod(Function(o) o.GetReplaceSQL)
                Case Else
                    Throw New ArgumentException(ActionNotAllows, paramName:=NameOf(sqlAction))
            End Select
        End Function
    End Class
End Namespace
