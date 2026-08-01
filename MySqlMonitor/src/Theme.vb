' Theme.vb
' Defines a full-color palette for the MySqlMonitor dashboard. A Theme carries
' every color used by the renderer: structural colors (background / panel /
' border / accent), semantic colors (muted / ok / warn / danger) and the
' per-SQL-verb colors, plus the three grade colors used for valuegrading.
'
' Themes can be selected at runtime via the -theme / -themefile CLI options and
' cycled live with the "t" hotkey. Custom themes are loaded from an INI file
' (using GCModeller's Microsoft.VisualBasic.Core IniFile) where each [section]
' is one theme.

Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf

Public Class Theme

    ' A simple RGB triple.
    Public Structure RGB
        Public r, g, b As Integer
        Public Sub New(r As Integer, g As Integer, b As Integer)
            Me.r = r : Me.g = g : Me.b = b
        End Sub
        Public Overrides Function ToString() As String
            Return r & "," & g & "," & b
        End Function
    End Structure

    ' ---- Identity ----
    Public Property Name As String = "Ocean"

    ' ---- Structural ----
    Public Property BG As RGB = New RGB(11, 14, 20)
    Public Property Panel As RGB = New RGB(18, 23, 34)
    Public Property Border As RGB = New RGB(63, 182, 201)
    Public Property Accent As RGB = New RGB(63, 182, 201)

    ' ---- Semantic ----
    Public Property Muted As RGB = New RGB(130, 140, 155)
    Public Property Ok As RGB = New RGB(61, 214, 140)
    Public Property Warn As RGB = New RGB(242, 193, 78)
    Public Property Danger As RGB = New RGB(242, 92, 84)

    ' ---- Per SQL verb ----
    Public Property SelectC As RGB = New RGB(61, 214, 140)
    Public Property InsertC As RGB = New RGB(63, 182, 201)
    Public Property UpdateC As RGB = New RGB(63, 182, 201)
    Public Property DeleteC As RGB = New RGB(242, 92, 84)
    Public Property CreateC As RGB = New RGB(242, 193, 78)
    Public Property AlterC As RGB = New RGB(242, 193, 78)
    Public Property DropC As RGB = New RGB(242, 92, 84)
    Public Property UserC As RGB = New RGB(63, 182, 201)

    ' ---- Grade colors (low value = ok, mid = warn, high = danger) ----
    Public Property GradeOk As RGB = New RGB(61, 214, 140)
    Public Property GradeWarn As RGB = New RGB(242, 193, 78)
    Public Property GradeDanger As RGB = New RGB(242, 92, 84)

    ' ---- ANSI prefix builders ----
    Public Function FgBG() As String
        Return Ansi.Bg(BG.r, BG.g, BG.b)
    End Function
    Public Function FgPanel() As String
        Return Ansi.Bg(Panel.r, Panel.g, Panel.b)
    End Function
    Public Function FgBorder() As String
        Return Ansi.Fg(Border.r, Border.g, Border.b)
    End Function
    Public Function FgAccent() As String
        Return Ansi.Fg(Accent.r, Accent.g, Accent.b)
    End Function
    Public Function FgMuted() As String
        Return Ansi.Fg(Muted.r, Muted.g, Muted.b)
    End Function
    Public Function FgOk() As String
        Return Ansi.Fg(Ok.r, Ok.g, Ok.b)
    End Function
    Public Function FgWarn() As String
        Return Ansi.Fg(Warn.r, Warn.g, Warn.b)
    End Function
    Public Function FgDanger() As String
        Return Ansi.Fg(Danger.r, Danger.g, Danger.b)
    End Function
    Public Function FgSelect() As String
        Return Ansi.Fg(SelectC.r, SelectC.g, SelectC.b)
    End Function
    Public Function FgInsert() As String
        Return Ansi.Fg(InsertC.r, InsertC.g, InsertC.b)
    End Function
    Public Function FgUpdate() As String
        Return Ansi.Fg(UpdateC.r, UpdateC.g, UpdateC.b)
    End Function
    Public Function FgDelete() As String
        Return Ansi.Fg(DeleteC.r, DeleteC.g, DeleteC.b)
    End Function
    Public Function FgCreate() As String
        Return Ansi.Fg(CreateC.r, CreateC.g, CreateC.b)
    End Function
    Public Function FgAlter() As String
        Return Ansi.Fg(AlterC.r, AlterC.g, AlterC.b)
    End Function
    Public Function FgDrop() As String
        Return Ansi.Fg(DropC.r, DropC.g, DropC.b)
    End Function
    Public Function FgUser() As String
        Return Ansi.Fg(UserC.r, UserC.g, UserC.b)
    End Function

    ' Grade a value into an ANSI prefix. Same thresholds as the renderer's
    ' GradeValue usage: below <ok> -> ok color, below <warn> -> warn color,
    ' otherwise danger color.
    Public Function Grade(v As Double, ok As Double, warn As Double) As String
        If v <= ok Then Return Ansi.Fg(GradeOk.r, GradeOk.g, GradeOk.b)
        If v <= warn Then Return Ansi.Fg(GradeWarn.r, GradeWarn.g, GradeWarn.b)
        Return Ansi.Fg(GradeDanger.r, GradeDanger.g, GradeDanger.b)
    End Function

    ' -----------------------------------------------------------------
    ' Built-in themes
    ' -----------------------------------------------------------------
    Public Shared Function BuiltInThemes() As List(Of Theme)
        Dim list As New List(Of Theme) From {
            Ocean(),
            Light(),
            Matrix(),
            Amber()
        }
        Return list
    End Function

    ' Dark, cyan-accented (the original default look).
    Public Shared Function Ocean() As Theme
        Return New Theme With {
            .Name = "Ocean",
            .BG = New RGB(11, 14, 20),
            .Panel = New RGB(18, 23, 34),
            .Border = New RGB(63, 182, 201),
            .Accent = New RGB(63, 182, 201),
            .Muted = New RGB(130, 140, 155),
            .Ok = New RGB(61, 214, 140),
            .Warn = New RGB(242, 193, 78),
            .Danger = New RGB(242, 92, 84),
            .SelectC = New RGB(61, 214, 140),
            .InsertC = New RGB(63, 182, 201),
            .UpdateC = New RGB(63, 182, 201),
            .DeleteC = New RGB(242, 92, 84),
            .CreateC = New RGB(242, 193, 78),
            .AlterC = New RGB(242, 193, 78),
            .DropC = New RGB(242, 92, 84),
            .UserC = New RGB(63, 182, 201),
            .GradeOk = New RGB(61, 214, 140),
            .GradeWarn = New RGB(242, 193, 78),
            .GradeDanger = New RGB(242, 92, 84)
        }
    End Function

    ' Light background variant.
    Public Shared Function Light() As Theme
        Return New Theme With {
            .Name = "Light",
            .BG = New RGB(245, 247, 250),
            .Panel = New RGB(232, 236, 243),
            .Border = New RGB(90, 110, 140),
            .Accent = New RGB(20, 110, 160),
            .Muted = New RGB(110, 120, 135),
            .Ok = New RGB(20, 130, 80),
            .Warn = New RGB(170, 120, 10),
            .Danger = New RGB(190, 50, 45),
            .SelectC = New RGB(20, 130, 80),
            .InsertC = New RGB(20, 110, 160),
            .UpdateC = New RGB(20, 110, 160),
            .DeleteC = New RGB(190, 50, 45),
            .CreateC = New RGB(170, 120, 10),
            .AlterC = New RGB(170, 120, 10),
            .DropC = New RGB(190, 50, 45),
            .UserC = New RGB(20, 110, 160),
            .GradeOk = New RGB(20, 130, 80),
            .GradeWarn = New RGB(170, 120, 10),
            .GradeDanger = New RGB(190, 50, 45)
        }
    End Function

    ' Dark green "matrix" terminal.
    Public Shared Function Matrix() As Theme
        Return New Theme With {
            .Name = "Matrix",
            .BG = New RGB(0, 8, 0),
            .Panel = New RGB(0, 20, 0),
            .Border = New RGB(0, 200, 70),
            .Accent = New RGB(0, 230, 90),
            .Muted = New RGB(70, 130, 80),
            .Ok = New RGB(0, 230, 90),
            .Warn = New RGB(180, 220, 60),
            .Danger = New RGB(230, 70, 60),
            .SelectC = New RGB(0, 230, 90),
            .InsertC = New RGB(0, 200, 70),
            .UpdateC = New RGB(0, 200, 70),
            .DeleteC = New RGB(230, 70, 60),
            .CreateC = New RGB(180, 220, 60),
            .AlterC = New RGB(180, 220, 60),
            .DropC = New RGB(230, 70, 60),
            .UserC = New RGB(0, 200, 70),
            .GradeOk = New RGB(0, 230, 90),
            .GradeWarn = New RGB(180, 220, 60),
            .GradeDanger = New RGB(230, 70, 60)
        }
    End Function

    ' Amber retro CRT terminal.
    Public Shared Function Amber() As Theme
        Return New Theme With {
            .Name = "Amber",
            .BG = New RGB(20, 12, 0),
            .Panel = New RGB(33, 20, 2),
            .Border = New RGB(255, 176, 0),
            .Accent = New RGB(255, 200, 40),
            .Muted = New RGB(170, 120, 40),
            .Ok = New RGB(120, 255, 120),
            .Warn = New RGB(255, 210, 60),
            .Danger = New RGB(255, 90, 50),
            .SelectC = New RGB(120, 255, 120),
            .InsertC = New RGB(255, 200, 40),
            .UpdateC = New RGB(255, 200, 40),
            .DeleteC = New RGB(255, 90, 50),
            .CreateC = New RGB(255, 210, 60),
            .AlterC = New RGB(255, 210, 60),
            .DropC = New RGB(255, 90, 50),
            .UserC = New RGB(255, 200, 40),
            .GradeOk = New RGB(120, 255, 120),
            .GradeWarn = New RGB(255, 210, 60),
            .GradeDanger = New RGB(255, 90, 50)
        }
    End Function

    ' -----------------------------------------------------------------
    ' Lookup helpers
    ' -----------------------------------------------------------------
    Public Shared Function Find(list As List(Of Theme), name As String) As Theme
        If list Is Nothing OrElse list.Count = 0 Then Return Ocean()
        If String.IsNullOrWhiteSpace(name) Then Return list(0)
        Dim n = name.Trim().ToLower()
        For Each t In list
            If t.Name.ToLower() = n Then Return t
        Next
        Return list(0)
    End Function

    ' -----------------------------------------------------------------
    ' INI loading - each [section] is one theme.
    '
    ' Recognized keys (values are "r,g,b"):
    '   bg panel border accent muted ok warn danger
    '   select insert update delete create alter drop user
    '   grade_ok grade_warn grade_danger
    ' Any missing key falls back to the Ocean defaults.
    ' -----------------------------------------------------------------
    Public Shared Function FromIni(path As String) As List(Of Theme)
        Dim result As New List(Of Theme)
        If String.IsNullOrWhiteSpace(path) OrElse Not System.IO.File.Exists(path) Then
            Return result
        End If

        Dim ini As New IniFile(path)
        Dim base = Ocean()

        For Each sectionName In ini.SectionNames
            Dim t As New Theme With {.Name = sectionName}
            Dim read As Func(Of String, RGB, RGB) =
                Function(key As String, fallback As RGB) As RGB
                    Dim raw = ini.ReadValue(sectionName, key, "")
                    If String.IsNullOrWhiteSpace(raw) Then Return fallback
                    Dim parts = raw.Split(","c)
                    If parts.Length < 3 Then Return fallback
                    Dim r As Integer = 0, g As Integer = 0, b As Integer = 0
                    If Integer.TryParse(parts(0).Trim(), r) AndAlso
                       Integer.TryParse(parts(1).Trim(), g) AndAlso
                       Integer.TryParse(parts(2).Trim(), b) Then
                        Return New RGB(r, g, b)
                    End If
                    Return fallback
                End Function

            ' Structural
            t.BG = read("bg", base.BG)
            t.Panel = read("panel", base.Panel)
            t.Border = read("border", base.Border)
            t.Accent = read("accent", base.Accent)
            ' Semantic
            t.Muted = read("muted", base.Muted)
            t.Ok = read("ok", base.Ok)
            t.Warn = read("warn", base.Warn)
            t.Danger = read("danger", base.Danger)
            ' SQL verbs
            t.SelectC = read("select", base.SelectC)
            t.InsertC = read("insert", base.InsertC)
            t.UpdateC = read("update", base.UpdateC)
            t.DeleteC = read("delete", base.DeleteC)
            t.CreateC = read("create", base.CreateC)
            t.AlterC = read("alter", base.AlterC)
            t.DropC = read("drop", base.DropC)
            t.UserC = read("user", base.UserC)
            ' Grades
            t.GradeOk = read("grade_ok", base.GradeOk)
            t.GradeWarn = read("grade_warn", base.GradeWarn)
            t.GradeDanger = read("grade_danger", base.GradeDanger)

            result.Add(t)
        Next

        Return result
    End Function

End Class
