Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

<TestClass>
Public Class AttentionServiceTests

    Private ReadOnly _today As New DateTime(
        2026,
        8,
        19
    )


    ' =====================================================
    ' Overdue revisions
    ' =====================================================

    <TestMethod>
    Public Sub OverdueRevision_DeadlineBeforeToday_ReturnsTrue()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today.AddDays(-1)
            )

        Assert.IsTrue(
            ManuscriptAttentionService.
                HasOverdueRevision(
                    manuscript,
                    _today
                )
        )

    End Sub


    <TestMethod>
    Public Sub OverdueRevision_DeadlineToday_ReturnsFalse()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today
            )

        Assert.IsFalse(
            ManuscriptAttentionService.
                HasOverdueRevision(
                    manuscript,
                    _today
                )
        )

    End Sub


    ' =====================================================
    ' Revision due soon
    ' =====================================================

    <TestMethod>
    Public Sub RevisionDueSoon_AtWarningBoundary_ReturnsTrue()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today.AddDays(14)
            )

        Assert.IsTrue(
            ManuscriptAttentionService.
                IsRevisionDueSoon(
                    manuscript,
                    _today,
                    14
                )
        )

    End Sub


    <TestMethod>
    Public Sub RevisionDueSoon_BeyondWarningBoundary_ReturnsFalse()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today.AddDays(15)
            )

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsRevisionDueSoon(
                    manuscript,
                    _today,
                    14
                )
        )

    End Sub


    <TestMethod>
    Public Sub RevisionDueSoon_WrongStageOrLocation_ReturnsFalse()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today.AddDays(5)
            )

        manuscript.CurrentStage =
            PaperStage.Draft

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsRevisionDueSoon(
                    manuscript,
                    _today,
                    14
                )
        )

        manuscript.CurrentStage =
            PaperStage.Revision

        manuscript.Location =
            ManuscriptLocation.FileDrawer

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsRevisionDueSoon(
                    manuscript,
                    _today,
                    14
                )
        )

    End Sub


    <TestMethod>
    Public Sub RevisionDueSoon_UsesLatestDecision()

        Dim manuscript As Manuscript =
            CreateRevisionManuscript(
                _today.AddDays(5)
            )

        Dim submission As JournalSubmission =
            manuscript.Submissions(0)

        submission.Decisions.Add(
            New EditorialDecisionEvent With {
                .DecisionDate =
                    _today.AddDays(-1),
                .Decision =
                    EditorialDecision.Accepted,
                .RevisionDeadline =
                    Nothing
            }
        )

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsRevisionDueSoon(
                    manuscript,
                    _today,
                    14
                )
        )

    End Sub


    ' =====================================================
    ' Long review
    ' =====================================================

    <TestMethod>
    Public Sub LongReview_AtThreshold_ReturnsTrue()

        Dim manuscript As Manuscript =
            CreateWaitingManuscript(
                _today.AddDays(-90)
            )

        Assert.IsTrue(
            ManuscriptAttentionService.
                IsLongWaitingManuscript(
                    manuscript,
                    _today,
                    90
                )
        )

    End Sub


    <TestMethod>
    Public Sub LongReview_BelowThreshold_ReturnsFalse()

        Dim manuscript As Manuscript =
            CreateWaitingManuscript(
                _today.AddDays(-89)
            )

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsLongWaitingManuscript(
                    manuscript,
                    _today,
                    90
                )
        )

    End Sub


    <TestMethod>
    Public Sub LongReview_UsesLatestSubmission()

        Dim manuscript As Manuscript =
            CreateWaitingManuscript(
                _today.AddDays(-120)
            )

        manuscript.Submissions.Add(
            New JournalSubmission With {
                .JournalName =
                    "Newer Journal",
                .SubmittedDate =
                    _today.AddDays(-10)
            }
        )

        Assert.IsFalse(
            ManuscriptAttentionService.
                IsLongWaitingManuscript(
                    manuscript,
                    _today,
                    90
                )
        )

    End Sub


    ' =====================================================
    ' Missing target journal
    ' =====================================================

    <TestMethod>
    Public Sub MissingTargetJournal_DraftPipelineWithoutTarget_ReturnsTrue()

        Dim manuscript As New Manuscript With {
            .Title =
                "Untargeted Draft",
            .CurrentStage =
                PaperStage.Draft,
            .Location =
                ManuscriptLocation.Pipeline,
            .TargetJournal =
                String.Empty
        }

        Assert.IsTrue(
            ManuscriptAttentionService.
                HasMissingTargetJournal(
                    manuscript
                )
        )

    End Sub


    <TestMethod>
    Public Sub MissingTargetJournal_UnderReviewWithoutTarget_ReturnsFalse()

        Dim manuscript As New Manuscript With {
            .Title =
                "Already Submitted",
            .CurrentStage =
                PaperStage.UnderReview,
            .Location =
                ManuscriptLocation.Pipeline,
            .TargetJournal =
                String.Empty
        }

        Assert.IsFalse(
            ManuscriptAttentionService.
                HasMissingTargetJournal(
                    manuscript
                )
        )

    End Sub


    ' =====================================================
    ' Recent rejection
    ' =====================================================

    <TestMethod>
    Public Sub RecentRejection_AtThreshold_ReturnsTrue()

        Dim manuscript As Manuscript =
            CreateRejectedManuscript(
                _today.AddDays(-30)
            )

        Assert.IsTrue(
            ManuscriptAttentionService.
                WasRecentlyRejected(
                    manuscript,
                    _today,
                    30
                )
        )

    End Sub


    <TestMethod>
    Public Sub RecentRejection_OutsideThreshold_ReturnsFalse()

        Dim manuscript As Manuscript =
            CreateRejectedManuscript(
                _today.AddDays(-31)
            )

        Assert.IsFalse(
            ManuscriptAttentionService.
                WasRecentlyRejected(
                    manuscript,
                    _today,
                    30
                )
        )

    End Sub


    <TestMethod>
    Public Sub RecentRejection_LatestDecisionControlsResult()

        Dim manuscript As Manuscript =
            CreateRejectedManuscript(
                _today.AddDays(-5)
            )

        Dim submission As JournalSubmission =
            manuscript.Submissions(0)

        submission.Decisions.Add(
            New EditorialDecisionEvent With {
                .DecisionDate =
                    _today.AddDays(-1),
                .Decision =
                    EditorialDecision.MajorRevision,
                .RevisionDeadline =
                    _today.AddDays(20)
            }
        )

        Assert.IsFalse(
            ManuscriptAttentionService.
                WasRecentlyRejected(
                    manuscript,
                    _today,
                    30
                )
        )

    End Sub


    <TestMethod>
    Public Sub RecentRejection_LatestSubmissionControlsResult()

        Dim manuscript As Manuscript =
            CreateRejectedManuscript(
                _today.AddDays(-5)
            )

        manuscript.Submissions.Add(
            New JournalSubmission With {
                .JournalName =
                    "New Route Journal",
                .SubmittedDate =
                    _today.AddDays(-1)
            }
        )

        Assert.IsFalse(
            ManuscriptAttentionService.
                WasRecentlyRejected(
                    manuscript,
                    _today,
                    30
                )
        )

    End Sub


    ' =====================================================
    ' Helpers
    ' =====================================================

    Private Function CreateRevisionManuscript(
        deadline As DateTime
    ) As Manuscript

        Dim manuscript As New Manuscript With {
            .Title =
                "Revision Study",
            .CurrentStage =
                PaperStage.Revision,
            .Location =
                ManuscriptLocation.Pipeline
        }

        Dim submission As New JournalSubmission With {
            .JournalName =
                "Journal of Revision Tests",
            .SubmittedDate =
                _today.AddDays(-60)
        }

        submission.Decisions.Add(
            New EditorialDecisionEvent With {
                .DecisionDate =
                    _today.AddDays(-20),
                .Decision =
                    EditorialDecision.MajorRevision,
                .RevisionDeadline =
                    deadline
            }
        )

        manuscript.Submissions.Add(
            submission
        )

        Return manuscript

    End Function


    Private Function CreateWaitingManuscript(
        submittedDate As DateTime
    ) As Manuscript

        Dim manuscript As New Manuscript With {
            .Title =
                "Waiting Study",
            .CurrentStage =
                PaperStage.UnderReview,
            .Location =
                ManuscriptLocation.Pipeline
        }

        manuscript.Submissions.Add(
            New JournalSubmission With {
                .JournalName =
                    "Journal of Waiting",
                .SubmittedDate =
                    submittedDate
            }
        )

        Return manuscript

    End Function


    Private Function CreateRejectedManuscript(
        decisionDate As DateTime
    ) As Manuscript

        Dim manuscript As New Manuscript With {
            .Title =
                "Rejected Study",
            .CurrentStage =
                PaperStage.Draft,
            .Location =
                ManuscriptLocation.Pipeline
        }

        Dim submission As New JournalSubmission With {
            .JournalName =
                "Journal of Rejections",
            .SubmittedDate =
                decisionDate.AddDays(-30)
        }

        submission.Decisions.Add(
            New EditorialDecisionEvent With {
                .DecisionDate =
                    decisionDate,
                .Decision =
                    EditorialDecision.Rejected
            }
        )

        manuscript.Submissions.Add(
            submission
        )

        Return manuscript

    End Function

End Class