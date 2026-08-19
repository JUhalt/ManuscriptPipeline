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

        ' Alpha builds default to Preview so testers continue receiving
        ' alpha/RC releases. This default can move to Stable for v0.1.0.
        Public Property UpdateChannel As AppUpdateChannel =
            AppUpdateChannel.Preview

        Public Property CheckForUpdatesAutomatically As Boolean =
            True

    End Class

End Namespace
