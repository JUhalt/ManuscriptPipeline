Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Services

<TestClass>
Public Class UserGuideServiceTests

    <TestMethod>
    Public Sub ToPlainText_ConvertsHeadingsAndBullets()

        Dim markdown As String =
            "# Guide" & vbLf &
            "## Quick Start" & vbLf &
            "- Add a manuscript" & vbLf &
            "- Save it"

        Dim plain As String =
            UserGuideService.ToPlainText(
                markdown
            )

        StringAssert.Contains(
            plain,
            "GUIDE"
        )

        StringAssert.Contains(
            plain,
            "Quick Start"
        )

        StringAssert.Contains(
            plain,
            "• Add a manuscript"
        )

    End Sub


    <TestMethod>
    Public Sub ToPlainText_ExpandsMarkdownLinks()

        Dim plain As String =
            UserGuideService.ToPlainText(
                "[PaperRoute](https://github.com/JUhalt/PaperRoute-Tracker)"
            )

        StringAssert.Contains(
            plain,
            "PaperRoute — https://github.com/JUhalt/PaperRoute-Tracker"
        )

    End Sub


    <TestMethod>
    Public Sub ToPlainText_EmptyInput_ReturnsBlank()

        Assert.AreEqual(
            String.Empty,
            UserGuideService.ToPlainText(
                String.Empty
            )
        )

    End Sub

End Class
