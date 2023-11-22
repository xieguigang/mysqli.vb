﻿Imports Oracle.LinuxCompatibility.MySQL.Reflection.Schema
Imports Oracle.LinuxCompatibility.MySQL.Uri

Namespace MySqlBuilder

    Public Class Model

        ReadOnly mysql As MySqli
        ReadOnly schema As Table

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="table">the table name</param>
        ''' <param name="conn"></param>
        Sub New(table As String, conn As ConnectionUri)
            Me.mysql = conn
            Me.schema = inspectSchema(conn.Database, table)
        End Sub

        Private Function inspectSchema(database As String, table As String) As Table
            Dim sql As String = $"describe `{database}`.`{table}`;"
            Dim schema = mysql.ExecuteDataset(sql)

        End Function
    End Class


End Namespace