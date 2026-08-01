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

Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.Settings.Inf

Public Class Theme

    ' ---- Identity ----
    Public Property Name As String = "Ocean"

    ' ---- Structural ----
    Public Property BG As AnsiColor = AnsiColor.Rgb(11, 14, 20)
    Public Property Panel As AnsiColor = AnsiColor.Rgb(18, 23, 34)
    Public Property Border As AnsiColor = AnsiColor.Rgb(63, 182, 201)
    Public Property Accent As AnsiColor = AnsiColor.Rgb(63, 182, 201)

    ' ---- Semantic ----
    Public Property Muted As AnsiColor = AnsiColor.Rgb(130, 140, 155)
    Public Property Ok As AnsiColor = AnsiColor.Rgb(61, 214, 140)
    Public Property Warn As AnsiColor = AnsiColor.Rgb(242, 193, 78)
    Public Property Danger As AnsiColor = AnsiColor.Rgb(242, 92, 84)

    ' ---- Per SQL verb ----
    Public Property SelectC As AnsiColor = AnsiColor.Rgb(61, 214, 140)
    Public Property InsertC As AnsiColor = AnsiColor.Rgb(63, 182, 201)
    Public Property UpdateC As AnsiColor = AnsiColor.Rgb(63, 182, 201)
    Public Property DeleteC As AnsiColor = AnsiColor.Rgb(242, 92, 84)
    Public Property CreateC As AnsiColor = AnsiColor.Rgb(242, 193, 78)
    Public Property AlterC As AnsiColor = AnsiColor.Rgb(242, 193, 78)
    Public Property DropC As AnsiColor = AnsiColor.Rgb(242, 92, 84)
    Public Property UserC As AnsiColor = AnsiColor.Rgb(63, 182, 201)

    ' ---- Grade colors (low value = ok, mid = warn, high = danger) ----
    Public Property GradeOk As AnsiColor = AnsiColor.Rgb(61, 214, 140)
    Public Property GradeWarn As AnsiColor = AnsiColor.Rgb(242, 193, 78)
    Public Property GradeDanger As AnsiColor = AnsiColor.Rgb(242, 92, 84)

    ' ---- ANSI prefix builders ----
    Public Function FgBG() As String
        Return Ansi.Bg(BG.R, BG.G, BG.B)
    End Function
    Public Function FgPanel() As String
        Return Ansi.Bg(Panel.R, Panel.G, Panel.B)
    End Function
    Public Function FgBorder() As String
        Return Ansi.Fg(Border.R, Border.G, Border.B)
    End Function
    Public Function FgAccent() As String
        Return Ansi.Fg(Accent.R, Accent.G, Accent.B)
    End Function
    Public Function FgMuted() As String
        Return Ansi.Fg(Muted.R, Muted.G, Muted.B)
    End Function
    Public Function FgOk() As String
        Return Ansi.Fg(Ok.R, Ok.G, Ok.B)
    End Function
    Public Function FgWarn() As String
        Return Ansi.Fg(Warn.R, Warn.G, Warn.B)
    End Function
    Public Function FgDanger() As String
        Return Ansi.Fg(Danger.R, Danger.G, Danger.B)
    End Function
    Public Function FgSelect() As String
        Return Ansi.Fg(SelectC.R, SelectC.G, SelectC.B)
    End Function
    Public Function FgInsert() As String
        Return Ansi.Fg(InsertC.R, InsertC.G, InsertC.B)
    End Function
    Public Function FgUpdate() As String
        Return Ansi.Fg(UpdateC.R, UpdateC.G, UpdateC.B)
    End Function
    Public Function FgDelete() As String
        Return Ansi.Fg(DeleteC.R, DeleteC.G, DeleteC.B)
    End Function
    Public Function FgCreate() As String
        Return Ansi.Fg(CreateC.R, CreateC.G, CreateC.B)
    End Function
    Public Function FgAlter() As String
        Return Ansi.Fg(AlterC.R, AlterC.G, AlterC.B)
    End Function
    Public Function FgDrop() As String
        Return Ansi.Fg(DropC.R, DropC.G, DropC.B)
    End Function
    Public Function FgUser() As String
        Return Ansi.Fg(UserC.R, UserC.G, UserC.B)
    End Function

    ' Grade a value into an ANSI prefix. Same thresholds as the renderer's
    ' GradeValue usage: below <ok> -> ok color, below <warn> -> warn color,
    ' otherwise danger color.
    Public Function Grade(v As Double, ok As Double, warn As Double) As String
        If v <= ok Then Return Ansi.Fg(GradeOk.R, GradeOk.G, GradeOk.B)
        If v <= warn Then Return Ansi.Fg(GradeWarn.R, GradeWarn.G, GradeWarn.B)
        Return Ansi.Fg(GradeDanger.R, GradeDanger.G, GradeDanger.B)
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
            .BG = AnsiColor.Rgb(11, 14, 20),
            .Panel = AnsiColor.Rgb(18, 23, 34),
            .Border = AnsiColor.Rgb(63, 182, 201),
            .Accent = AnsiColor.Rgb(63, 182, 201),
            .Muted = AnsiColor.Rgb(130, 140, 155),
            .Ok = AnsiColor.Rgb(61, 214, 140),
            .Warn = AnsiColor.Rgb(242, 193, 78),
            .Danger = AnsiColor.Rgb(242, 92, 84),
            .SelectC = AnsiColor.Rgb(61, 214, 140),
            .InsertC = AnsiColor.Rgb(63, 182, 201),
            .UpdateC = AnsiColor.Rgb(63, 182, 201),
            .DeleteC = AnsiColor.Rgb(242, 92, 84),
            .CreateC = AnsiColor.Rgb(242, 193, 78),
            .AlterC = AnsiColor.Rgb(242, 193, 78),
            .DropC = AnsiColor.Rgb(242, 92, 84),
            .UserC = AnsiColor.Rgb(63, 182, 201),
            .GradeOk = AnsiColor.Rgb(61, 214, 140),
            .GradeWarn = AnsiColor.Rgb(242, 193, 78),
            .GradeDanger = AnsiColor.Rgb(242, 92, 84)
        }
    End Function

    ' Light background variant.
    Public Shared Function Light() As Theme
        Return New Theme With {
            .Name = "Light",
            .BG = AnsiColor.Rgb(245, 247, 250),
            .Panel = AnsiColor.Rgb(232, 236, 243),
            .Border = AnsiColor.Rgb(90, 110, 140),
            .Accent = AnsiColor.Rgb(20, 110, 160),
            .Muted = AnsiColor.Rgb(110, 120, 135),
            .Ok = AnsiColor.Rgb(20, 130, 80),
            .Warn = AnsiColor.Rgb(170, 120, 10),
            .Danger = AnsiColor.Rgb(190, 50, 45),
            .SelectC = AnsiColor.Rgb(20, 130, 80),
            .InsertC = AnsiColor.Rgb(20, 110, 160),
            .UpdateC = AnsiColor.Rgb(20, 110, 160),
            .DeleteC = AnsiColor.Rgb(190, 50, 45),
            .CreateC = AnsiColor.Rgb(170, 120, 10),
            .AlterC = AnsiColor.Rgb(170, 120, 10),
            .DropC = AnsiColor.Rgb(190, 50, 45),
            .UserC = AnsiColor.Rgb(20, 110, 160),
            .GradeOk = AnsiColor.Rgb(20, 130, 80),
            .GradeWarn = AnsiColor.Rgb(170, 120, 10),
            .GradeDanger = AnsiColor.Rgb(190, 50, 45)
        }
    End Function

    ' Dark green "matrix" terminal.
    Public Shared Function Matrix() As Theme
        Return New Theme With {
            .Name = "Matrix",
            .BG = AnsiColor.Rgb(0, 8, 0),
            .Panel = AnsiColor.Rgb(0, 20, 0),
            .Border = AnsiColor.Rgb(0, 200, 70),
            .Accent = AnsiColor.Rgb(0, 230, 90),
            .Muted = AnsiColor.Rgb(70, 130, 80),
            .Ok = AnsiColor.Rgb(0, 230, 90),
            .Warn = AnsiColor.Rgb(180, 220, 60),
            .Danger = AnsiColor.Rgb(230, 70, 60),
            .SelectC = AnsiColor.Rgb(0, 230, 90),
            .InsertC = AnsiColor.Rgb(0, 200, 70),
            .UpdateC = AnsiColor.Rgb(0, 200, 70),
            .DeleteC = AnsiColor.Rgb(230, 70, 60),
            .CreateC = AnsiColor.Rgb(180, 220, 60),
            .AlterC = AnsiColor.Rgb(180, 220, 60),
            .DropC = AnsiColor.Rgb(230, 70, 60),
            .UserC = AnsiColor.Rgb(0, 200, 70),
            .GradeOk = AnsiColor.Rgb(0, 230, 90),
            .GradeWarn = AnsiColor.Rgb(180, 220, 60),
            .GradeDanger = AnsiColor.Rgb(230, 70, 60)
        }
    End Function

    ' Amber retro CRT terminal.
    Public Shared Function Amber() As Theme
        Return New Theme With {
            .Name = "Amber",
            .BG = AnsiColor.Rgb(20, 12, 0),
            .Panel = AnsiColor.Rgb(33, 20, 2),
            .Border = AnsiColor.Rgb(255, 176, 0),
            .Accent = AnsiColor.Rgb(255, 200, 40),
            .Muted = AnsiColor.Rgb(170, 120, 40),
            .Ok = AnsiColor.Rgb(120, 255, 120),
            .Warn = AnsiColor.Rgb(255, 210, 60),
            .Danger = AnsiColor.Rgb(255, 90, 50),
            .SelectC = AnsiColor.Rgb(120, 255, 120),
            .InsertC = AnsiColor.Rgb(255, 200, 40),
            .UpdateC = AnsiColor.Rgb(255, 200, 40),
            .DeleteC = AnsiColor.Rgb(255, 90, 50),
            .CreateC = AnsiColor.Rgb(255, 210, 60),
            .AlterC = AnsiColor.Rgb(255, 210, 60),
            .DropC = AnsiColor.Rgb(255, 90, 50),
            .UserC = AnsiColor.Rgb(255, 200, 40),
            .GradeOk = AnsiColor.Rgb(120, 255, 120),
            .GradeWarn = AnsiColor.Rgb(255, 210, 60),
            .GradeDanger = AnsiColor.Rgb(255, 90, 50)
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
            Dim read As Func(Of String, AnsiColor, AnsiColor) =
                Function(key As String, fallback As AnsiColor) As AnsiColor
                    Dim raw = ini.ReadValue(sectionName, key, "")
                    If String.IsNullOrWhiteSpace(raw) Then Return fallback
                    Dim parts = raw.Split(","c)
                    If parts.Length < 3 Then Return fallback
                    Dim r As Integer = 0, g As Integer = 0, b As Integer = 0
                    If Integer.TryParse(parts(0).Trim(), r) AndAlso
                       Integer.TryParse(parts(1).Trim(), g) AndAlso
                       Integer.TryParse(parts(2).Trim(), b) Then
                        Return AnsiColor.Rgb(r, g, b)
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
