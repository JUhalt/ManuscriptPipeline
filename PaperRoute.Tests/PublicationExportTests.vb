Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

<TestClass>
Public Class PublicationExportTests

    <TestMethod>
    Public Sub SelectByScope_PublishedOnly_ExcludesEarlierStages()

        Dim items As New List(Of Manuscript) From {
            New Manuscript With {
                .Title = "Idea",
                .CurrentStage = PaperStage.Idea
            },
            New Manuscript With {
                .Title = "Published",
                .CurrentStage = PaperStage.Published,
                .Location = ManuscriptLocation.Published
            }
        }

        Dim selected As List(Of Manuscript) =
            PublicationExportService.SelectByScope(
                items,
                PublicationExportScope.PublishedOnly
            )

        Assert.AreEqual(
            1,
            selected.Count
        )

        Assert.AreEqual(
            "Published",
            selected(0).Title
        )

    End Sub


    <TestMethod>
    Public Sub SelectByScope_AcceptedAndPublished_IncludesAcceptedInPressAndPublished()

        Dim items As New List(Of Manuscript) From {
            New Manuscript With {
                .Title = "Review",
                .CurrentStage = PaperStage.UnderReview
            },
            New Manuscript With {
                .Title = "Accepted",
                .CurrentStage = PaperStage.Accepted
            },
            New Manuscript With {
                .Title = "In Press",
                .CurrentStage = PaperStage.InPress
            },
            New Manuscript With {
                .Title = "Published",
                .CurrentStage = PaperStage.Published,
                .Location = ManuscriptLocation.Published
            }
        }

        Dim selected As List(Of Manuscript) =
            PublicationExportService.SelectByScope(
                items,
                PublicationExportScope.AcceptedAndPublished
            )

        Assert.AreEqual(
            3,
            selected.Count
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_UsesStructuredAuthorsInManuscriptOrder()

        Dim first As New AuthorRecord With {
            .GivenName = "Jane",
            .FamilyName = "Smith"
        }

        Dim second As New AuthorRecord With {
            .GivenName = "John",
            .MiddleName = "Paul",
            .FamilyName = "Doe"
        }

        Dim library As New AuthorLibraryData()

        library.Authors.Add(first)
        library.Authors.Add(second)

        Dim manuscript As New Manuscript With {
            .Title = "A Paper"
        }

        manuscript.Authors.Add(
            New ManuscriptAuthor With {
                .AuthorId = first.Id
            }
        )

        manuscript.Authors.Add(
            New ManuscriptAuthor With {
                .AuthorId = second.Id
            }
        )

        manuscript.Metadata.PublishedDate =
            New DateTime(
                2026,
                1,
                1
            )

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                library
            )

        Assert.IsTrue(
            citation.StartsWith(
                "Smith, J., & Doe, J. P. (2026).",
                StringComparison.Ordinal
            ),
            citation
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_IncludesJournalVolumeIssuePagesAndDoi()

        Dim manuscript As New Manuscript With {
            .Title = "Metadata Rich Paper"
        }

        manuscript.Metadata.PublishedDate =
            New DateTime(
                2025,
                5,
                4
            )

        manuscript.Metadata.PublicationJournal =
            "Journal of Examples"

        manuscript.Metadata.Volume =
            "12"

        manuscript.Metadata.Issue =
            "3"

        manuscript.Metadata.Pages =
            "44-58"

        manuscript.Metadata.Doi =
            "https://doi.org/10.1234/example"

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                New AuthorLibraryData()
            )

        StringAssert.Contains(
            citation,
            "Journal of Examples, 12(3), 44-58."
        )

        StringAssert.Contains(
            citation,
            "https://doi.org/10.1234/example"
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_WithoutDoi_RemainsUseful()

        Dim manuscript As New Manuscript With {
            .Title = "No DOI Paper"
        }

        manuscript.Metadata.PublishedDate =
            New DateTime(
                2024,
                1,
                1
            )

        manuscript.Metadata.PublicationJournal =
            "Journal"

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                New AuthorLibraryData()
            )

        StringAssert.Contains(
            citation,
            "(2024)."
        )

        StringAssert.Contains(
            citation,
            "No DOI Paper."
        )

        StringAssert.Contains(
            citation,
            "Journal."
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_PreprintFallback_IsIncluded()

        Dim manuscript As New Manuscript With {
            .Title = "Preprint Paper"
        }

        manuscript.Metadata.PreprintDoi =
            "10.5555/preprint"

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                New AuthorLibraryData()
            )

        StringAssert.Contains(
            citation,
            "[Preprint]."
        )

        StringAssert.Contains(
            citation,
            "Preprint: https://doi.org/10.5555/preprint"
        )

    End Sub


    <TestMethod>
    Public Sub Export_Markdown_CreatesEditableList()

        Dim manuscript As New Manuscript With {
            .Title = "Markdown Paper"
        }

        Dim output As String =
            PublicationExportService.Export(
                New List(Of Manuscript) From {
                    manuscript
                },
                New AuthorLibraryData(),
                PublicationExportFormat.Markdown,
                PublicationExportStyle.CvSection
            )

        StringAssert.Contains(
            output,
            "## Publications"
        )

        StringAssert.Contains(
            output,
            "- "
        )

        StringAssert.Contains(
            output,
            "Markdown Paper"
        )

    End Sub


    <TestMethod>
    Public Sub Export_Html_EscapesUntrustedText()

        Dim manuscript As New Manuscript With {
            .Title = "A <B> & C"
        }

        Dim output As String =
            PublicationExportService.Export(
                New List(Of Manuscript) From {
                    manuscript
                },
                New AuthorLibraryData(),
                PublicationExportFormat.Html,
                PublicationExportStyle.PublicationList
            )

        StringAssert.Contains(
            output,
            "A &lt;B&gt; &amp; C"
        )

        Assert.IsFalse(
            output.Contains(
                "A <B> & C",
                StringComparison.Ordinal
            )
        )

    End Sub


    <TestMethod>
    Public Sub Export_DoesNotMutateManuscriptData()

        Dim manuscript As New Manuscript With {
            .Title = "Original Title",
            .CurrentStage = PaperStage.Accepted
        }

        Dim originalStage As PaperStage =
            manuscript.CurrentStage

        Dim originalTitle As String =
            manuscript.Title

        PublicationExportService.Export(
            New List(Of Manuscript) From {
                manuscript
            },
            New AuthorLibraryData(),
            PublicationExportFormat.PlainText,
            PublicationExportStyle.CvSection
        )

        Assert.AreEqual(
            originalTitle,
            manuscript.Title
        )

        Assert.AreEqual(
            originalStage,
            manuscript.CurrentStage
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_PublisherOnlyRecord_IncludesPublisher()

        Dim manuscript As New Manuscript With {
            .Title = "A Book"
        }

        manuscript.Metadata.PublishedDate =
            New DateTime(
                2020,
                1,
                1
            )

        manuscript.Metadata.Publisher =
            "Example University Press"

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                New AuthorLibraryData()
            )

        StringAssert.Contains(
            citation,
            "Example University Press."
        )

    End Sub


    <TestMethod>
    Public Sub FormatCitation_AcceptedRecord_CanUseTargetJournalAsOutlet()

        Dim manuscript As New Manuscript With {
            .Title = "Accepted Paper",
            .CurrentStage = PaperStage.Accepted,
            .TargetJournal = "Journal of Future Work"
        }

        Dim citation As String =
            PublicationExportService.FormatCitation(
                manuscript,
                New AuthorLibraryData()
            )

        StringAssert.Contains(
            citation,
            "Journal of Future Work."
        )

    End Sub

End Class
