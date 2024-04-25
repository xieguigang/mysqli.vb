Imports Oracle.LinuxCompatibility.MySQL.MySqlBuilder

Public Module sql_test

    Sub Main2()
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.b"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("a.`b.b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a.a`.`b`"))
        Call Console.WriteLine(FieldAssert.ParseFieldName("`a`.`b`"))

        Pause()
    End Sub
End Module
