' Ansi.vb
' ANSI escape sequence helpers for building a terminal dashboard on Ubuntu.
' Pure ANSI - no third-party TUI library.

Public Module Ansi

    ' ---- Cursor control ----
    Public Function Home() As String
        Return Esc() & "[H"
    End Function

    Public Function HideCursor() As String
        Return Esc() & "[?25l"
    End Function

    Public Function ShowCursor() As String
        Return Esc() & "[?25h"
    End Function

    ' Clear from cursor to end of screen (used for full redraw).
    Public Function ClearDown() As String
        Return Esc() & "[J"
    End Function

    ' Enter the alternate screen buffer (so the dashboard does not scroll the
    ' user's existing terminal contents). Pairs with AltScreenOff.
    Public Function AltScreenOn() As String
        Return Esc() & "[?1049h"
    End Function

    ' Leave the alternate screen buffer, restoring the previous view.
    Public Function AltScreenOff() As String
        Return Esc() & "[?1049l"
    End Function

    ' Move cursor to a 1-based row/col.
    Public Function MoveTo(row As Integer, col As Integer) As String
        Return Esc() & "[" & row.ToString() & ";" & col.ToString() & "H"
    End Function

    ' ---- Color (truecolor 24-bit) ----
    Public Function Fg(r As Integer, g As Integer, b As Integer) As String
        Return Esc() & "[38;2;" & r & ";" & g & ";" & b & "m"
    End Function

    Public Function Bg(r As Integer, g As Integer, b As Integer) As String
        Return Esc() & "[48;2;" & r & ";" & g & ";" & b & "m"
    End Function

    Public Function Reset() As String
        Return Esc() & "[0m"
    End Function

    Public Function Bold(s As String) As String
        Return Esc() & "[1m" & s & Esc() & "[22m"
    End Function

    Public Function [Dim](s As String) As String
        Return Esc() & "[2m" & s & Esc() & "[22m"
    End Function

    Public Function Inverse(s As String) As String
        Return Esc() & "[7m" & s & Esc() & "[27m"
    End Function

    ' ---- Semantic palette (matches dark dashboard theme) ----
    Public Function FgReset() As String
        Return Fg(215, 224, 238) ' #D7E0EE default text
    End Function

    Public Function FgMuted() As String
        Return Fg(138, 151, 168) ' #8A97A8
    End Function

    Public Function FgAccent() As String
        Return Fg(63, 182, 201) ' #3FB6C9
    End Function

    Public Function FgOk() As String
        Return Fg(61, 214, 140) ' #3DD68C
    End Function

    Public Function FgWarn() As String
        Return Fg(242, 193, 78) ' #F2C14E
    End Function

    Public Function FgDanger() As String
        Return Fg(242, 92, 84) ' #F25C54
    End Function

    ' Choose a graded color based on a 0..1 ratio vs warn/danger thresholds.
    Public Function Grade(ratio As Double, warn As Double, danger As Double) As String
        If ratio >= danger Then Return FgDanger()
        If ratio >= warn Then Return FgWarn()
        Return FgOk()
    End Function

    ' Choose color by value with explicit thresholds (higher = worse).
    Public Function GradeValue(v As Double, warn As Double, danger As Double) As String
        If v >= danger Then Return FgDanger()
        If v >= warn Then Return FgWarn()
        Return FgOk()
    End Function

    ' ---- Progress / bar ----
    ' Draw a horizontal bar of `width` cells. `ratio` in 0..1.
    ' Uses block characters: full █, partial ▓, empty ░.
    Public Function Bar(ratio As Double, width As Integer, Optional color As String = "") As String
        If width <= 0 Then Return ""
        If ratio < 0 Then ratio = 0
        If ratio > 1 Then ratio = 1
        Dim filled As Integer = CInt(Math.Round(ratio * width))
        If filled > width Then filled = width
        Dim sb As New Text.StringBuilder()
        If color <> "" Then sb.Append(color)
        sb.Append(New String("█"c, filled))
        sb.Append(FgMuted())
        sb.Append(New String("░"c, width - filled))
        sb.Append(Reset())
        Return sb.ToString()
    End Function

    ' ---- Box drawing helpers ----
    Public Function HLine(width As Integer) As String
        Return New String("─"c, width)
    End Function

    Public Function VLine() As String
        Return "│"
    End Function

    Private Function Esc() As String
        Return ChrW(&H1B)
    End Function

End Module
