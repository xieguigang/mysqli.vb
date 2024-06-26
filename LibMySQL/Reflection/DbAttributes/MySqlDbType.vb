﻿#Region "Microsoft.VisualBasic::40d3b8ef5ea75f3ac9392a0f86949219, src\mysqli\LibMySQL\Reflection\DbAttributes\MySqlDbType.vb"

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

    '   Total Lines: 64
    '    Code Lines: 47
    ' Comment Lines: 15
    '   Blank Lines: 2
    '     File Size: 1.49 KB


    '     Enum MySqlDbType
    ' 
    '         [Byte], [Date], [DateTime], [Decimal], [Double]
    '         [Enum], [Set], [String], BigInt, Bit
    '         Blob, Float, Geometry, Guid, Int64
    '         LongBlob, LongText, MediumBlob, MediumInt, MediumText
    '         NewDate, NewDecimal, Text, Time, Timestamp
    '         TinyBlob, TinyInt, TinyText, UByte, VarBinary
    '         VarChar, VarString, Year
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Reflection.DbAttributes

    ''' <summary>
    ''' Enum all of the data type in the mysql database.
    ''' (枚举MYSQL数据库中所有的数据类型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MySqlDbType As Integer
        Binary = 0
        Bit
        Blob
        ''' <summary>
        ''' Long
        ''' </summary>
        ''' <remarks></remarks>
        BigInt
        ''' <summary>
        ''' ``TINYINT(1)``
        ''' </summary>
        <Description("TINYINT")> [Boolean]
        [Byte]
        [Date]
        [DateTime]
        [Decimal]
        [Double]
        [Enum]
        Float
        Geometry
        Guid
        <Description("int")> Int16
        <Description("int")> Int24
        <Description("int")> Int32
        Int64
        LongBlob
        LongText
        MediumBlob
        MediumText
        MediumInt
        NewDate
        NewDecimal
        [Set]
        [String]
        Text
        Time
        Timestamp
        TinyBlob
        TinyText
        ''' <summary>
        ''' + ``TINYINT(1)`` 在VB.NET之中是<see cref="Boolean"/>
        ''' </summary>
        TinyInt
        UByte
        <Description("int")> UInt16
        <Description("int")> UInt24
        <Description("int")> UInt32
        <Description("int")> UInt64
        VarBinary
        VarChar
        VarString
        Year
    End Enum
End Namespace
