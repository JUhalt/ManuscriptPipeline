Namespace Models

    Public Class AppSettings

        Public Property Appearance As AppAppearance =
            AppAppearance.System

        Public Property FileDrawerSuggestionThreshold As Integer =
            3

        Public Property LongReviewThresholdDays As Integer =
            90

        Public Property RevisionWarningDays As Integer =
            14

        Public Property RecentRejectionThresholdDays As Integer =
            30

    End Class

End Namespace