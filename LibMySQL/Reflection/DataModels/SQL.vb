#Region "Microsoft.VisualBasic::a91e528414352a9151449dc8b10bcc87, ..\mysqli\LibMySQL\Reflection\DataModels\SQL.vb"

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

Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema

Namespace Reflection.SQL

    Public MustInherit Class SQL

        ''' <summary>
        ''' The table schema of the sql generation target.(用于生成SQL语句的表结构属性)
        ''' </summary>
        ''' <remarks></remarks>
        Protected _schemaInfo As Table

        Protected Sub New()
        End Sub

        Public Sub New(SchemaInfo As Table)
            _schemaInfo = SchemaInfo
        End Sub

        Public Overrides Function ToString() As String
            Return _schemaInfo.ToString
        End Function
    End Class
End Namespace
