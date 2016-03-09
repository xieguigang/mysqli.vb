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
        [Byte]
        [Date]
        [DateTime]
        [Decimal]
        [Double]
        [Enum]
        Float
        Geometry
        Guid
        Int16
        Int24
        Int32
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
        TinyInt
        UByte
        UInt16
        UInt24
        UInt32
        UInt64
        VarBinary
        VarChar
        VarString
        Year
    End Enum
End Namespace

