Imports System.Text
Imports Oracle.LinuxCompatibility.MySQL.Reflection.DbAttributes
Imports System.Reflection
Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Microsoft.VisualBasic

Namespace Reflection.SQL

    ''' <summary>
    ''' Generate the CREATE TABLE sql of the target table schema class object.
    ''' (生成目标数据表模式的"CREATE TABLE" sql语句)
    ''' </summary>
    ''' <remarks>
    ''' Example SQL:
    ''' 
    ''' CREATE  TABLE `Table_Name` (
    '''   `Field1` INT UNSIGNED ZEROFILL NOT NULL DEFAULT 4444 ,
    '''   `Field2` VARCHAR(45) BINARY NOT NULL DEFAULT '534534' ,
    '''   `Field3` INT UNSIGNED ZEROFILL NOT NULL AUTO_INCREMENT ,
    '''  PRIMARY KEY (`Field1`, `Field2`, `Field3`) ,
    '''  UNIQUE INDEX `Field1_UNIQUE` (`Field1` ASC) ,
    '''  UNIQUE INDEX `Field2_UNIQUE` (`Field2` ASC) );
    ''' </remarks>
    Public Class CreateTableSQL

        Friend Const CREATE_TABLE As String = "CREATE  TABLE `{0}` ("
        Friend Const PRIMARY_KEY As String = "PRIMARY KEY ({0})"
        Friend Const UNIQUE_INDEX As String = "UNIQUE INDEX `%s_UNIQUE` (`%s` ASC)"

        ''' <summary>
        ''' Generate the 'CREATE TABLE' sql command.
        ''' (生成'CREATE TABLE' sql命令)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FromSchema(Schema As Schema.Table) As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Dim sBuilder2 As StringBuilder = New StringBuilder(128)

            sBuilder.AppendFormat(CREATE_TABLE & vbCrLf, Schema.TableName)

            Dim Fields = Schema.Fields
            For i As Integer = 0 To Fields.Length - 1
                sBuilder.AppendLine("  " & Fields(i).ToString & " ,")
            Next

            Dim PrimaryField = Schema.PrimaryFields
            For Each PK As String In PrimaryField
                sBuilder2.AppendFormat("`{0}`, ", PK)
            Next
            sBuilder2.Remove(sBuilder2.Length - 2, 2)
            sBuilder.AppendFormat(PRIMARY_KEY & vbCrLf, sBuilder2.ToString)

            Dim UniqueFields = Schema.UniqueFields
            If UniqueFields.Count > 0 Then
                sBuilder.Append(" ,")
            End If
            For Each UniqueField As String In UniqueFields
                sBuilder.AppendLine(UNIQUE_INDEX.Replace("%s", UniqueField) & " ,")
            Next
            sBuilder.Remove(sBuilder.Length - 3, 3)
            sBuilder.Append(");") 'End of the sql statement

            Return sBuilder.ToString
        End Function
    End Class
End Namespace
