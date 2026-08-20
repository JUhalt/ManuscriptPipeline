Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Threading.Tasks
Imports ManuscriptPipeline.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services
Imports ManuscriptPipeline.Controls

Public Class Form1

    Private activeAttentionFilter As AttentionFilter =
    AttentionFilter.None

    Private ReadOnly lblRevisionDueSoon As New Label()

    Private ReadOnly settingsService As New AppSettingsService()

    Private appSettings As AppSettings =
    New AppSettings()

    Private manuscripts As New List(Of Manuscript)()

    Private ReadOnly repository As New ManuscriptRepository()

    Private ReadOnly pipelinePanel As New FlowLayoutPanel()
    Private ReadOnly publishedPanel As New FlowLayoutPanel()
    Private ReadOnly fileDrawerPanel As New FlowLayoutPanel()

    Private ReadOnly lblPipelineHeader As New Label()
    Private ReadOnly lblPublishedHeader As New Label()
    Private ReadOnly lblFileDrawerHeader As New Label()

    Private ReadOnly lblStatus As New Label()

    Private ReadOnly txtBoardSearch As New TextBox()
    Private ReadOnly cboStageFilter As New ComboBox()
    Private ReadOnly cboBoardSort As New ComboBox()
    Private ReadOnly btnClearBoardFilters As New Button()

    Private ReadOnly lblAttentionTitle As New Label()
    Private ReadOnly lblOverdueRevisions As New Label()
    Private ReadOnly lblLongReviews As New Label()
    Private ReadOnly lblMissingJournal As New Label()
    Private ReadOnly lblRecentRejections As New Label()

    Private uiInitialized As Boolean = False


    ' =====================================================
    ' Startup
    ' =====================================================

    Private Sub OpenAbout(
    sender As Object,
    e As EventArgs
)

        Using dialog As New AboutForm()

            dialog.ShowDialog(Me)

        End Using

    End Sub
    Private Sub Form1_Load(
    sender As Object,
    e As EventArgs
) Handles MyBase.Load

        If uiInitialized Then
            Return
        End If

        uiInitialized = True

        appSettings =
        settingsService.Load()

        UiPolish.InstallGlobalDialogStyling()

        BuildInterface()

        If Not LoadManuscripts() Then

            ' Do not allow PaperRoute to continue running with an
            ' artificial empty library after a data-load failure.
            BeginInvoke(
            New Action(
                Sub()
                    Close()
                End Sub
            )
        )

            Return

        End If

        RenderManuscripts()

        If appSettings.CheckForUpdatesAutomatically Then
            BeginAutomaticUpdateCheck()
        End If

    End Sub

    ' =====================================================
    ' Interface
    ' =====================================================

    Private Sub BuildInterface()

        Me.Controls.Clear()

        Me.Text = "PaperRoute Tracker"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1180, 820)
        Me.MinimumSize = New Size(900, 680)
        Me.Font = New Font("Segoe UI", 10.0F)
        Me.AutoScaleMode = AutoScaleMode.Dpi
        Me.BackColor = UiTheme.BoardBackground()
        Me.DoubleBuffered = True

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = UiTheme.BoardBackground()
        }

        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))

        ' =================================================
        ' Header
        ' =================================================

        Dim header As New TableLayoutPanel With {
    .Dock = DockStyle.Top,
    .AutoSize = True,
    .AutoSizeMode = AutoSizeMode.GrowAndShrink,
    .ColumnCount = 2,
    .RowCount = 1,
    .Padding = New Padding(18, 10, 18, 10),
    .BackColor = UiTheme.HeaderBackground()
}

        header.ColumnStyles.Add(
    New ColumnStyle(SizeType.Percent, 100)
)

        header.ColumnStyles.Add(
    New ColumnStyle(SizeType.AutoSize)
)

        ' =================================================
        ' Branding
        ' =================================================

        Dim branding As New TableLayoutPanel With {
    .Dock = DockStyle.Fill,
    .AutoSize = True,
    .AutoSizeMode = AutoSizeMode.GrowAndShrink,
    .ColumnCount = 1,
    .RowCount = 2,
    .Margin = New Padding(0)
}

        branding.RowStyles.Add(
    New RowStyle(SizeType.AutoSize)
)

        branding.RowStyles.Add(
    New RowStyle(SizeType.AutoSize)
)


        Dim lblTitle As New Label With {
    .Text = "PaperRoute",
    .AutoSize = True,
    .Anchor = AnchorStyles.Left,
    .Font = New Font(
        Me.Font.FontFamily,
        16.0F,
        FontStyle.Bold
    ),
    .ForeColor = UiTheme.AccentColor()
}

        Dim lblSubtitle As New Label With {
    .Text = "Academic manuscript tracking, locally",
    .AutoSize = True,
    .Anchor = AnchorStyles.Left,
    .ForeColor = UiTheme.SecondaryText(),
    .Margin = New Padding(0, 2, 0, 0)
}

        branding.Controls.Add(
    lblTitle,
    0,
    0
)

        branding.Controls.Add(
    lblSubtitle,
    0,
    1
)


        ' =================================================
        ' Header actions
        ' =================================================

        Dim headerActions As New FlowLayoutPanel With {
    .AutoSize = True,
    .AutoSizeMode = AutoSizeMode.GrowAndShrink,
    .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
    .FlowDirection = FlowDirection.RightToLeft,
    .WrapContents = False,
    .Margin = New Padding(12, 2, 0, 0)
}


        Dim btnAdd As New Button With {
    .Text = "+ Add Manuscript",
    .Width = 165,
    .Height = 44
}

        Dim btnData As New Button With {
    .Text = "Data ▼",
    .Width = 110,
    .Height = 44
}

        Dim btnSettings As New Button With {
    .Text = "Settings ▼",
    .Width = 118,
    .Height = 44
}


        ' =================================================
        ' Data menu
        ' =================================================

        Dim dataMenu As New ContextMenuStrip()

        dataMenu.Items.Add(
    "Get Import Template...",
    Nothing,
    AddressOf ExportBlankTemplate
)

        dataMenu.Items.Add(
    "Import Spreadsheet...",
    Nothing,
    AddressOf ImportExcelHistory
)

        dataMenu.Items.Add(
    "Export Library to Excel...",
    Nothing,
    AddressOf ExportLibraryExcel
)

        dataMenu.Items.Add(
    New ToolStripSeparator()
)

        dataMenu.Items.Add(
    "Backup Library...",
    Nothing,
    AddressOf BackupLibrary
)

        dataMenu.Items.Add(
    "Restore Backup...",
    Nothing,
    AddressOf RestoreLibraryBackup
)

        ' =================================================
        ' Settings menu
        ' =================================================

        Dim settingsMenu As New ContextMenuStrip()

        settingsMenu.Items.Add(
    "Preferences...",
    Nothing,
    AddressOf OpenSettings
)

        settingsMenu.Items.Add(
    New ToolStripSeparator()
)

        settingsMenu.Items.Add(
    "Check for Updates...",
    Nothing,
    AddressOf CheckForUpdatesNow
)

        settingsMenu.Items.Add(
    "Diagnostics...",
    Nothing,
    AddressOf OpenDiagnostics
)

        settingsMenu.Items.Add(
    New ToolStripSeparator()
)

        settingsMenu.Items.Add(
    "About PaperRoute",
    Nothing,
    AddressOf OpenAbout
)

        ' =================================================
        ' Header handlers
        ' =================================================

        AddHandler btnAdd.Click,
    AddressOf AddManuscript

        AddHandler btnSettings.Click,
    Sub(sender, e)

        settingsMenu.Show(
            btnSettings,
            New Point(
                0,
                btnSettings.Height
            )
        )

    End Sub

        AddHandler btnData.Click,
    Sub(sender, e)

        dataMenu.Show(
            btnData,
            New Point(
                0,
                btnData.Height
            )
        )

    End Sub

        StyleCardButton(
            btnAdd,
            UiTheme.AccentColor()
        )

        StyleCardButton(
            btnData,
            UiTheme.AccentSecondaryColor()
        )

        StyleCardButton(
            btnSettings,
            UiTheme.SecondaryText()
        )


        headerActions.Controls.Add(
    btnAdd
)

        headerActions.Controls.Add(
    btnData
)

        headerActions.Controls.Add(
    btnSettings
)


        ' =================================================
        ' Assemble header
        ' =================================================

        header.Controls.Add(
    branding,
    0,
    0
)

        header.Controls.Add(
    headerActions,
    1,
    0
)

        ' =================================================
        ' Main body
        ' =================================================

        Dim body As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 8,
            .Padding = New Padding(18, 8, 18, 12),
            .BackColor = UiTheme.BoardBackground()
        }

        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 52))

        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 48))

        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 25))

        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 27))


        ' =================================================
        ' Search / filter / sort bar
        ' =================================================

        Dim attentionBar As New FlowLayoutPanel With {
    .Dock = DockStyle.Fill,
    .FlowDirection = FlowDirection.LeftToRight,
    .WrapContents = False,
    .Padding = New Padding(0, 7, 0, 3),
    .BackColor = UiTheme.BoardBackground()
}

        lblAttentionTitle.Text =
    "NEEDS ATTENTION"

        lblAttentionTitle.AutoSize =
    True

        lblAttentionTitle.Font =
    New Font(
        Me.Font.FontFamily,
        9.0F,
        FontStyle.Bold
    )

        lblAttentionTitle.ForeColor =
    UiTheme.PrimaryText()

        lblAttentionTitle.Margin =
    New Padding(0, 3, 16, 0)

        ConfigureAttentionLabel(
    lblOverdueRevisions
)

        ConfigureAttentionLabel(
    lblRevisionDueSoon
)

        AddHandler lblOverdueRevisions.Click,
    AddressOf FilterOverdueRevisions

        AddHandler lblRevisionDueSoon.Click,
    AddressOf FilterRevisionDueSoon

        AddHandler lblLongReviews.Click,
    AddressOf FilterLongReviews

        AddHandler lblMissingJournal.Click,
    AddressOf FilterMissingJournal

        AddHandler lblRecentRejections.Click,
    AddressOf FilterRecentRejections

        ConfigureAttentionLabel(
    lblLongReviews
)

        ConfigureAttentionLabel(
    lblMissingJournal
)

        ConfigureAttentionLabel(
    lblRecentRejections
)


        attentionBar.Controls.Add(
    lblAttentionTitle
)

        attentionBar.Controls.Add(
    lblOverdueRevisions
)

        attentionBar.Controls.Add(
    lblRevisionDueSoon
)

        attentionBar.Controls.Add(
    lblLongReviews
)

        attentionBar.Controls.Add(
    lblMissingJournal
)

        attentionBar.Controls.Add(
    lblRecentRejections
)

        Dim boardToolbar As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(0, 6, 0, 4),
            .BackColor = UiTheme.BoardBackground()
        }


        txtBoardSearch.Width = 310
        txtBoardSearch.PlaceholderText =
            "Search title, journal, or co-authors..."

        txtBoardSearch.Margin =
            New Padding(0, 2, 10, 0)
        txtBoardSearch.BackColor = UiTheme.CardBackground()
        txtBoardSearch.ForeColor = UiTheme.PrimaryText()
        txtBoardSearch.BorderStyle = BorderStyle.FixedSingle


        cboStageFilter.Width = 150
        cboStageFilter.DropDownStyle =
            ComboBoxStyle.DropDownList

        cboStageFilter.Items.Clear()

        cboStageFilter.Items.Add("All stages")
        cboStageFilter.Items.Add("Idea")
        cboStageFilter.Items.Add("Draft")
        cboStageFilter.Items.Add("Submitted")
        cboStageFilter.Items.Add("Under Review")
        cboStageFilter.Items.Add("Revision")
        cboStageFilter.Items.Add("Accepted")
        cboStageFilter.Items.Add("In Press")
        cboStageFilter.Items.Add("Published")

        cboStageFilter.SelectedIndex = 0

        cboStageFilter.Margin =
            New Padding(0, 2, 10, 0)
        cboStageFilter.BackColor = UiTheme.CardBackground()
        cboStageFilter.ForeColor = UiTheme.PrimaryText()
        cboStageFilter.FlatStyle = FlatStyle.Flat


        cboBoardSort.Width = 235
        cboBoardSort.DropDownWidth = 235
        cboBoardSort.DropDownStyle =
            ComboBoxStyle.DropDownList

        cboBoardSort.Items.Clear()

        cboBoardSort.Items.Add("Sort: Current order")
        cboBoardSort.Items.Add("Sort: Title A-Z")
        cboBoardSort.Items.Add("Sort: Title Z-A")
        cboBoardSort.Items.Add("Sort: Most rejections")
        cboBoardSort.Items.Add("Sort: Fewest rejections")
        cboBoardSort.Items.Add("Sort: Newest stage change")
        cboBoardSort.Items.Add("Sort: Oldest stage change")

        cboBoardSort.SelectedIndex = 0

        cboBoardSort.Margin =
            New Padding(0, 2, 10, 0)
        cboBoardSort.BackColor = UiTheme.CardBackground()
        cboBoardSort.ForeColor = UiTheme.PrimaryText()
        cboBoardSort.FlatStyle = FlatStyle.Flat


        btnClearBoardFilters.Text = "Clear"
        btnClearBoardFilters.Width = 80
        btnClearBoardFilters.Height = 30
        btnClearBoardFilters.Enabled = False
        btnClearBoardFilters.Margin =
            New Padding(0, 1, 0, 0)

        StyleCardButton(
            btnClearBoardFilters,
            UiTheme.SecondaryText()
        )


        AddHandler txtBoardSearch.TextChanged,
            AddressOf BoardFilterChanged

        AddHandler cboStageFilter.SelectedIndexChanged,
            AddressOf BoardFilterChanged

        AddHandler cboBoardSort.SelectedIndexChanged,
            AddressOf BoardFilterChanged

        AddHandler btnClearBoardFilters.Click,
            AddressOf ClearBoardFilters


        boardToolbar.Controls.Add(
            txtBoardSearch
        )

        boardToolbar.Controls.Add(
            cboStageFilter
        )

        boardToolbar.Controls.Add(
            cboBoardSort
        )

        boardToolbar.Controls.Add(
            btnClearBoardFilters
        )

        ' =================================================
        ' Shelves
        ' =================================================

        ConfigureSectionHeader(
            lblPipelineHeader,
            "PIPELINE"
        )

        ConfigureSectionHeader(
            lblPublishedHeader,
            "PUBLISHED"
        )

        ConfigureSectionHeader(
            lblFileDrawerHeader,
            "FILE DRAWER"
        )

        ConfigureFlowPanel(
            pipelinePanel
        )

        ConfigureFlowPanel(
            publishedPanel
        )

        ConfigureFlowPanel(
            fileDrawerPanel
        )


        body.Controls.Add(
    attentionBar,
    0,
    0
)

        body.Controls.Add(
    boardToolbar,
    0,
    1
)

        body.Controls.Add(
    lblPipelineHeader,
    0,
    2
)

        body.Controls.Add(
    pipelinePanel,
    0,
    3
)

        body.Controls.Add(
    lblPublishedHeader,
    0,
    4
)

        body.Controls.Add(
    publishedPanel,
    0,
    5
)

        body.Controls.Add(
    lblFileDrawerHeader,
    0,
    6
)

        body.Controls.Add(
    fileDrawerPanel,
    0,
    7
)

        ' =================================================
        ' Footer
        ' =================================================

        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        lblStatus.Padding = New Padding(18, 0, 0, 0)
        lblStatus.ForeColor = SystemColors.GrayText
        lblStatus.Text = "Local storage enabled."

        root.Controls.Add(
            header,
            0,
            0
        )

        root.Controls.Add(
            body,
            0,
            1
        )

        root.Controls.Add(
            lblStatus,
            0,
            2
        )

        Me.Controls.Add(
            root
        )

        AddHandler pipelinePanel.Resize,
            Sub(sender, e)
                ResizeCards(pipelinePanel)
            End Sub

        AddHandler publishedPanel.Resize,
            Sub(sender, e)
                ResizeCards(publishedPanel)
            End Sub

        AddHandler fileDrawerPanel.Resize,
            Sub(sender, e)
                ResizeCards(fileDrawerPanel)
            End Sub

    End Sub

    Private Sub FilterOverdueRevisions(
    sender As Object,
    e As EventArgs
)

        ToggleAttentionFilter(
            AttentionFilter.OverdueRevision
        )

    End Sub


    Private Sub FilterRevisionDueSoon(
        sender As Object,
        e As EventArgs
    )

        ToggleAttentionFilter(
            AttentionFilter.RevisionDueSoon
        )

    End Sub


    Private Sub FilterLongReviews(
        sender As Object,
        e As EventArgs
    )

        ToggleAttentionFilter(
            AttentionFilter.LongReview
        )

    End Sub


    Private Sub FilterMissingJournal(
        sender As Object,
        e As EventArgs
    )

        ToggleAttentionFilter(
            AttentionFilter.MissingTargetJournal
        )

    End Sub


    Private Sub FilterRecentRejections(
        sender As Object,
        e As EventArgs
    )

        ToggleAttentionFilter(
            AttentionFilter.RecentRejection
        )

    End Sub


    Private Sub ToggleAttentionFilter(
        filter As AttentionFilter
    )

        If activeAttentionFilter =
            filter Then

            activeAttentionFilter =
                AttentionFilter.None

        Else

            activeAttentionFilter =
                filter

        End If

        RenderManuscripts()

    End Sub

    Private Sub ConfigureAttentionLabel(
    label As Label
)

        label.AutoSize =
        True

        label.ForeColor =
        UiTheme.SecondaryText()

        label.Margin =
        New Padding(
            0,
            3,
            18,
            0
        )

        label.Cursor =
        Cursors.Hand

    End Sub


    Private Sub RefreshAttentionDashboard()

        Dim overdueCount As Integer = 0
        Dim dueSoonCount As Integer = 0
        Dim longReviewCount As Integer = 0
        Dim missingJournalCount As Integer = 0
        Dim recentRejectionCount As Integer = 0


        For Each manuscript As Manuscript In manuscripts

            If HasOverdueRevision(
            manuscript
        ) Then

                overdueCount += 1

            End If


            If IsRevisionDueSoon(
            manuscript
        ) Then

                dueSoonCount += 1

            End If


            If IsLongWaitingManuscript(
            manuscript
        ) Then

                longReviewCount += 1

            End If


            If HasMissingTargetJournal(
            manuscript
        ) Then

                missingJournalCount += 1

            End If


            If WasRecentlyRejected(
            manuscript
        ) Then

                recentRejectionCount += 1

            End If

        Next


        lblOverdueRevisions.Text =
        overdueCount.ToString() &
        " overdue revision" &
        If(
            overdueCount = 1,
            "",
            "s"
        )


        lblRevisionDueSoon.Text =
        dueSoonCount.ToString() &
        " revision" &
        If(
            dueSoonCount = 1,
            "",
            "s"
        ) &
        " due soon"


        lblLongReviews.Text =
        longReviewCount.ToString() &
        " waiting " &
        appSettings.LongReviewThresholdDays.ToString() &
        "+ days"


        lblMissingJournal.Text =
        missingJournalCount.ToString() &
        " no target journal"


        lblRecentRejections.Text =
        recentRejectionCount.ToString() &
        " rejected in last " &
        appSettings.RecentRejectionThresholdDays.ToString() &
        " days"


        StyleAttentionLabel(
        lblOverdueRevisions,
        overdueCount,
        AttentionFilter.OverdueRevision,
        UiTheme.DangerColor()
    )

        StyleAttentionLabel(
        lblRevisionDueSoon,
        dueSoonCount,
        AttentionFilter.RevisionDueSoon,
        UiTheme.WarningColor()
    )

        StyleAttentionLabel(
        lblLongReviews,
        longReviewCount,
        AttentionFilter.LongReview,
        UiTheme.WarningColor()
    )

        StyleAttentionLabel(
        lblMissingJournal,
        missingJournalCount,
        AttentionFilter.MissingTargetJournal,
        UiTheme.WarningColor()
    )

        StyleAttentionLabel(
        lblRecentRejections,
        recentRejectionCount,
        AttentionFilter.RecentRejection,
        UiTheme.DangerColor()
    )

    End Sub


    Private Sub StyleAttentionLabel(
    label As Label,
    count As Integer,
    filter As AttentionFilter,
    attentionColor As Color
)

        label.Enabled =
        count > 0

        If count = 0 Then

            label.ForeColor =
            UiTheme.SecondaryText()

            label.Cursor =
            Cursors.Default

        Else

            label.ForeColor =
            attentionColor

            label.Cursor =
            Cursors.Hand

        End If


        If activeAttentionFilter =
        filter Then

            label.Font =
            New Font(
                label.Font,
                FontStyle.Bold Or
                FontStyle.Underline
            )

        Else

            label.Font =
            New Font(
                label.Font,
                FontStyle.Regular
            )

        End If

    End Sub

    Private Function BuildManuscriptInsight(
    manuscript As Manuscript,
    ByRef insightColor As Color
) As String

        insightColor =
        UiTheme.SecondaryText()


        ' =================================================
        ' Overdue revision
        ' =================================================

        If HasOverdueRevision(
        manuscript
    ) Then

            Dim latestSubmission As JournalSubmission =
            GetLatestSubmission(
                manuscript
            )

            Dim latestDecision As EditorialDecisionEvent =
            GetLatestDecision(
                latestSubmission
            )

            If latestDecision IsNot Nothing AndAlso
           latestDecision.RevisionDeadline.HasValue Then

                Dim overdueDays As Integer =
                CInt(
                    Math.Floor(
                        (
                            DateTime.Today -
                            latestDecision.RevisionDeadline.Value.Date
                        ).TotalDays
                    )
                )

                insightColor =
                UiTheme.DangerColor()

                Return "Revision overdue by " &
                overdueDays.ToString() &
                " day" &
                If(
                    overdueDays = 1,
                    "",
                    "s"
                )

            End If

        End If


        ' =================================================
        ' Revision due soon
        ' =================================================

        If IsRevisionDueSoon(
        manuscript
    ) Then

            Dim latestSubmission As JournalSubmission =
            GetLatestSubmission(
                manuscript
            )

            Dim latestDecision As EditorialDecisionEvent =
            GetLatestDecision(
                latestSubmission
            )

            If latestDecision IsNot Nothing AndAlso
           latestDecision.RevisionDeadline.HasValue Then

                Dim remainingDays As Integer =
                CInt(
                    Math.Floor(
                        (
                            latestDecision.RevisionDeadline.Value.Date -
                            DateTime.Today
                        ).TotalDays
                    )
                )

                insightColor =
                UiTheme.WarningColor()

                If remainingDays = 0 Then
                    Return "Revision due today"
                End If

                Return "Revision due in " &
                remainingDays.ToString() &
                " day" &
                If(
                    remainingDays = 1,
                    "",
                    "s"
                )

            End If

        End If


        ' =================================================
        ' Long review
        ' =================================================

        If IsLongWaitingManuscript(
        manuscript
    ) Then

            Dim submission As JournalSubmission =
            GetLatestSubmission(
                manuscript
            )

            If submission IsNot Nothing Then

                Dim waitingDays As Integer =
                CInt(
                    Math.Floor(
                        (
                            DateTime.Today -
                            submission.SubmittedDate.Date
                        ).TotalDays
                    )
                )

                insightColor =
                UiTheme.WarningColor()

                Return "Waiting " &
                waitingDays.ToString() &
                " days for a decision"

            End If

        End If


        ' =================================================
        ' Recent rejection
        ' =================================================

        If WasRecentlyRejected(
        manuscript
    ) Then

            Dim latestSubmission As JournalSubmission =
            GetLatestSubmission(
                manuscript
            )

            Dim latestDecision As EditorialDecisionEvent =
            GetLatestDecision(
                latestSubmission
            )

            If latestDecision IsNot Nothing Then

                Dim daysAgo As Integer =
                CInt(
                    Math.Floor(
                        (
                            DateTime.Today -
                            latestDecision.DecisionDate.Date
                        ).TotalDays
                    )
                )

                insightColor =
                UiTheme.DangerColor()

                Return "Rejected " &
                daysAgo.ToString() &
                " day" &
                If(
                    daysAgo = 1,
                    "",
                    "s"
                ) &
                " ago • choose the next target"

            End If

        End If


        ' =================================================
        ' Missing target
        ' =================================================

        If HasMissingTargetJournal(
        manuscript
    ) Then

            insightColor =
            UiTheme.WarningColor()

            Return "No target journal selected"

        End If


        ' =================================================
        ' File Drawer suggestion
        ' =================================================

        If manuscript.Location =
            ManuscriptLocation.Pipeline AndAlso
       manuscript.RejectionCount >=
            appSettings.FileDrawerSuggestionThreshold Then

            insightColor =
            UiTheme.WarningColor()

            Return "Consider filing after " &
            manuscript.RejectionCount.ToString() &
            " rejections"

        End If


        Return String.Empty

    End Function

    Private Sub ConfigureSectionHeader(
    label As Label,
    text As String
)

        label.Text = text
        label.AutoSize = True
        label.Anchor = AnchorStyles.Left

        label.Font =
        New Font(
            Me.Font.FontFamily,
            10.0F,
            FontStyle.Bold
        )

        label.ForeColor =
        UiTheme.PrimaryText()

    End Sub


    ' =====================================================
    ' Persistence
    ' =====================================================

    Private Function LoadManuscripts() As Boolean

        Try

            manuscripts =
            repository.Load()

            If repository.LastLoadRecoveredFromBackup Then

                Dim recoveryMessage As String =
                "PaperRoute detected a problem with the primary manuscript data file." &
                Environment.NewLine &
                Environment.NewLine &
                "Your library was recovered successfully from the automatic safety backup." &
                Environment.NewLine &
                Environment.NewLine &
                manuscripts.Count.ToString() &
                " manuscript(s) were recovered."

                If Not String.IsNullOrWhiteSpace(
                repository.LastRecoveryPreservedFilePath
            ) Then

                    recoveryMessage &=
                    Environment.NewLine &
                    Environment.NewLine &
                    "The damaged primary file was preserved for recovery and diagnostics at:" &
                    Environment.NewLine &
                    repository.LastRecoveryPreservedFilePath

                End If

                recoveryMessage &=
                Environment.NewLine &
                Environment.NewLine &
                "PaperRoute has restored a valid primary data file and can continue normally."

                MessageBox.Show(
                Me,
                recoveryMessage,
                "Library Recovered",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            )

                lblStatus.Text =
                "Library recovered from safety backup - " &
                DateTime.Now.ToString("h:mm tt")

            Else

                lblStatus.Text =
                "Loaded " &
                manuscripts.Count.ToString() &
                " manuscript(s) from local storage."

            End If

            Return True

        Catch ex As Exception

            manuscripts =
            New List(Of Manuscript)()

            Dim errorMessage As String =
            "PaperRoute could not safely load your manuscript library." &
            Environment.NewLine &
            Environment.NewLine &
            "The primary data file could not be read, and PaperRoute could not recover a valid safety backup." &
            Environment.NewLine &
            Environment.NewLine &
            "PaperRoute will close rather than continue with an empty library. Your existing data files have not been intentionally overwritten." &
            Environment.NewLine &
            Environment.NewLine &
            "Error details:" &
            Environment.NewLine &
            ex.Message

            MessageBox.Show(
            Me,
            errorMessage,
            "Library Could Not Be Loaded",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )

            lblStatus.Text =
            "Library could not be loaded safely."

            Return False

        End Try

    End Function

    Private Function SaveManuscripts() As Boolean

        Try

            repository.Save(
            manuscripts
        )

            lblStatus.Text =
            "Saved locally - " &
            DateTime.Now.ToString("h:mm tt")

            Return True

        Catch ex As Exception

            MessageBox.Show(
            Me,
            "PaperRoute could not save your data." &
            Environment.NewLine &
            Environment.NewLine &
            ex.Message,
            "Save Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        )

            lblStatus.Text =
            "Warning: latest changes were not saved."

            Return False

        End Try

    End Function

    ' =====================================================
    ' Flow panels
    ' =====================================================

    Private Sub ConfigureFlowPanel(
    panel As FlowLayoutPanel
)

        panel.Dock = DockStyle.Fill
        panel.FlowDirection = FlowDirection.TopDown
        panel.WrapContents = False
        panel.AutoScroll = True
        panel.AutoSize = False
        panel.Padding = New Padding(8)
        panel.BackColor = UiTheme.BoardBackground()
        panel.BorderStyle = BorderStyle.None

    End Sub


    ' =====================================================
    ' Render manuscripts
    ' =====================================================

    Private Sub StyleCardButton(
    button As Button,
    accentColor As Color
)

        button.FlatStyle =
        FlatStyle.Flat

        button.UseVisualStyleBackColor =
        False

        button.BackColor =
        UiTheme.CardBackground()

        button.ForeColor =
        accentColor

        button.FlatAppearance.BorderColor =
        accentColor

        button.FlatAppearance.BorderSize =
        1

        button.FlatAppearance.MouseOverBackColor =
        UiTheme.HoverBackground()

        button.FlatAppearance.MouseDownBackColor =
        UiTheme.HoverBackground()

        button.Cursor =
        Cursors.Hand

    End Sub

    Private Sub BoardFilterChanged(
        sender As Object,
        e As EventArgs
    )

        RenderManuscripts()

    End Sub


    Private Sub ClearBoardFilters(
        sender As Object,
        e As EventArgs
    )

        txtBoardSearch.Text =
            String.Empty

        cboStageFilter.SelectedIndex =
            0

        cboBoardSort.SelectedIndex =
            0

        activeAttentionFilter =
    AttentionFilter.None

        RenderManuscripts()

    End Sub


    Private Function HasActiveBoardFilters() As Boolean

        If Not String.IsNullOrWhiteSpace(
            txtBoardSearch.Text
        ) Then

            Return True

        End If

        If cboStageFilter.SelectedIndex > 0 Then
            Return True
        End If

        If cboBoardSort.SelectedIndex > 0 Then
            Return True
        End If

        If activeAttentionFilter <>
            AttentionFilter.None Then

            Return True

        End If

        Return False

    End Function


    Private Function ContainsSearchText(
        value As String,
        query As String
    ) As Boolean

        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If

        Return value.IndexOf(
            query,
            StringComparison.CurrentCultureIgnoreCase
        ) >= 0

    End Function


    Private Function ManuscriptMatchesBoardFilters(
    manuscript As Manuscript
) As Boolean

        Dim query As String =
        txtBoardSearch.Text.Trim()


        If Not String.IsNullOrWhiteSpace(
        query
    ) Then

            Dim matchesSearch As Boolean =
            ContainsSearchText(
                manuscript.Title,
                query
            ) OrElse
            ContainsSearchText(
                manuscript.TargetJournal,
                query
            ) OrElse
            ContainsSearchText(
                manuscript.CoAuthors,
                query
            )

            If Not matchesSearch Then
                Return False
            End If

        End If


        If cboStageFilter.SelectedIndex > 0 Then

            Dim selectedStage As String =
            CStr(
                cboStageFilter.SelectedItem
            )

            If Not String.Equals(
            FormatStage(
                manuscript.CurrentStage
            ),
            selectedStage,
            StringComparison.OrdinalIgnoreCase
        ) Then

                Return False

            End If

        End If


        Select Case activeAttentionFilter

            Case AttentionFilter.OverdueRevision

                If Not HasOverdueRevision(
                manuscript
            ) Then

                    Return False

                End If


            Case AttentionFilter.RevisionDueSoon

                If Not IsRevisionDueSoon(
                manuscript
            ) Then

                    Return False

                End If


            Case AttentionFilter.LongReview

                If Not IsLongWaitingManuscript(
                manuscript
            ) Then

                    Return False

                End If


            Case AttentionFilter.MissingTargetJournal

                If Not HasMissingTargetJournal(
                manuscript
            ) Then

                    Return False

                End If


            Case AttentionFilter.RecentRejection

                If Not WasRecentlyRejected(
                manuscript
            ) Then

                    Return False

                End If

        End Select


        Return True

    End Function

    Private Function IsRevisionDueSoon(
    manuscript As Manuscript
) As Boolean

        If manuscript.Location <>
        ManuscriptLocation.Pipeline Then

            Return False

        End If

        If manuscript.CurrentStage <>
        PaperStage.Revision Then

            Return False

        End If


        Dim latestSubmission As JournalSubmission =
        GetLatestSubmission(
            manuscript
        )

        Dim latestDecision As EditorialDecisionEvent =
        GetLatestDecision(
            latestSubmission
        )


        If latestDecision Is Nothing OrElse
       Not latestDecision.RevisionDeadline.HasValue Then

            Return False

        End If


        Dim daysRemaining As Integer =
        CInt(
            Math.Floor(
                (
                    latestDecision.RevisionDeadline.Value.Date -
                    DateTime.Today
                ).TotalDays
            )
        )


        Return daysRemaining >= 0 AndAlso
        daysRemaining <=
        appSettings.RevisionWarningDays

    End Function

    Private Function GetVisibleManuscripts() As List(Of Manuscript)

        Dim result As New List(Of Manuscript)()

        For Each manuscript As Manuscript In manuscripts

            If ManuscriptMatchesBoardFilters(
                manuscript
            ) Then

                result.Add(
                    manuscript
                )

            End If

        Next


        Select Case cboBoardSort.SelectedIndex

            Case 1

                result.Sort(
                    AddressOf CompareTitleAscending
                )

            Case 2

                result.Sort(
                    AddressOf CompareTitleDescending
                )

            Case 3

                result.Sort(
                    AddressOf CompareRejectionsDescending
                )

            Case 4

                result.Sort(
                    AddressOf CompareRejectionsAscending
                )

            Case 5

                result.Sort(
                    AddressOf CompareStageDateDescending
                )

            Case 6

                result.Sort(
                    AddressOf CompareStageDateAscending
                )

        End Select


        Return result

    End Function


    Private Function CompareTitleAscending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return StringComparer.CurrentCultureIgnoreCase.Compare(
            first.Title,
            second.Title
        )

    End Function


    Private Function CompareTitleDescending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return StringComparer.CurrentCultureIgnoreCase.Compare(
            second.Title,
            first.Title
        )

    End Function


    Private Function CompareRejectionsDescending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return second.RejectionCount.CompareTo(
            first.RejectionCount
        )

    End Function


    Private Function CompareRejectionsAscending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return first.RejectionCount.CompareTo(
            second.RejectionCount
        )

    End Function


    Private Function CompareStageDateDescending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return second.StageEnteredDate.CompareTo(
            first.StageEnteredDate
        )

    End Function


    Private Function CompareStageDateAscending(
        first As Manuscript,
        second As Manuscript
    ) As Integer

        Return first.StageEnteredDate.CompareTo(
            second.StageEnteredDate
        )

    End Function


    Private Function BuildSectionTitle(
        title As String,
        visibleCount As Integer,
        totalCount As Integer
    ) As String

        If HasActiveBoardFilters() AndAlso
           visibleCount <> totalCount Then

            Return title &
                " (" &
                visibleCount.ToString() &
                " of " &
                totalCount.ToString() &
                ")"

        End If

        Return title &
            " (" &
            totalCount.ToString() &
            ")"

    End Function

    Private Function GetLatestSubmission(
    manuscript As Manuscript
) As JournalSubmission

        Dim latest As JournalSubmission =
        Nothing

        For Each submission As JournalSubmission In manuscript.Submissions

            If latest Is Nothing OrElse
           submission.SubmittedDate >
           latest.SubmittedDate Then

                latest =
                submission

            End If

        Next

        Return latest

    End Function


    Private Function GetLatestDecision(
    submission As JournalSubmission
) As EditorialDecisionEvent

        If submission Is Nothing Then
            Return Nothing
        End If

        Dim latest As EditorialDecisionEvent =
        Nothing

        For Each decision As EditorialDecisionEvent In submission.Decisions

            If latest Is Nothing OrElse
           decision.DecisionDate >
           latest.DecisionDate Then

                latest =
                decision

            End If

        Next

        Return latest

    End Function


    Private Function HasOverdueRevision(
    manuscript As Manuscript
) As Boolean

        If manuscript.Location <>
        ManuscriptLocation.Pipeline Then

            Return False

        End If

        If manuscript.CurrentStage <>
        PaperStage.Revision Then

            Return False

        End If

        Dim latestSubmission As JournalSubmission =
        GetLatestSubmission(
            manuscript
        )

        Dim latestDecision As EditorialDecisionEvent =
        GetLatestDecision(
            latestSubmission
        )

        If latestDecision Is Nothing OrElse
       Not latestDecision.RevisionDeadline.HasValue Then

            Return False

        End If

        Return latestDecision.RevisionDeadline.Value.Date <
        DateTime.Today

    End Function


    Private Function IsLongWaitingManuscript(
    manuscript As Manuscript
) As Boolean

        If manuscript.Location <>
        ManuscriptLocation.Pipeline Then

            Return False

        End If

        If manuscript.CurrentStage <>
            PaperStage.Submitted AndAlso
       manuscript.CurrentStage <>
            PaperStage.UnderReview Then

            Return False

        End If

        Dim latestSubmission As JournalSubmission =
        GetLatestSubmission(
            manuscript
        )

        If latestSubmission Is Nothing Then
            Return False
        End If

        Dim waitingDays As Integer =
        CInt(
            Math.Floor(
                (
                    DateTime.Today -
                    latestSubmission.SubmittedDate.Date
                ).TotalDays
            )
        )

        Return waitingDays >=
        appSettings.LongReviewThresholdDays

    End Function


    Private Function HasMissingTargetJournal(
    manuscript As Manuscript
) As Boolean

        If manuscript.Location <>
        ManuscriptLocation.Pipeline Then

            Return False

        End If

        If manuscript.CurrentStage <>
            PaperStage.Idea AndAlso
       manuscript.CurrentStage <>
            PaperStage.Draft Then

            Return False

        End If

        Return String.IsNullOrWhiteSpace(
        manuscript.TargetJournal
    )

    End Function


    Private Function WasRecentlyRejected(
    manuscript As Manuscript
) As Boolean

        Dim latestSubmission As JournalSubmission =
        GetLatestSubmission(
            manuscript
        )

        Dim latestDecision As EditorialDecisionEvent =
        GetLatestDecision(
            latestSubmission
        )

        If latestDecision Is Nothing Then
            Return False
        End If


        Select Case latestDecision.Decision

            Case EditorialDecision.Rejected,
             EditorialDecision.DeskRejected,
             EditorialDecision.RejectedAfterReview

            Case Else

                Return False

        End Select


        Dim daysAgo As Integer =
        CInt(
            Math.Floor(
                (
                    DateTime.Today -
                    latestDecision.DecisionDate.Date
                ).TotalDays
            )
        )

        Return daysAgo >= 0 AndAlso
        daysAgo <=
        appSettings.RecentRejectionThresholdDays

    End Function

    Private Sub RenderManuscripts()

        RefreshAttentionDashboard()

        pipelinePanel.Controls.Clear()
        publishedPanel.Controls.Clear()
        fileDrawerPanel.Controls.Clear()


        ' =================================================
        ' Total counts
        ' =================================================

        Dim pipelineTotal As Integer = 0
        Dim publishedTotal As Integer = 0
        Dim drawerTotal As Integer = 0

        For Each manuscript As Manuscript In manuscripts

            Select Case manuscript.Location

                Case ManuscriptLocation.Pipeline
                    pipelineTotal += 1

                Case ManuscriptLocation.Published
                    publishedTotal += 1

                Case ManuscriptLocation.FileDrawer
                    drawerTotal += 1

            End Select

        Next


        ' =================================================
        ' Filtered / sorted records
        ' =================================================

        Dim visibleManuscripts As List(Of Manuscript) =
        GetVisibleManuscripts()

        Dim pipelineCount As Integer = 0
        Dim publishedCount As Integer = 0
        Dim drawerCount As Integer = 0


        For Each manuscript As Manuscript In visibleManuscripts

            Select Case manuscript.Location

                Case ManuscriptLocation.Pipeline

                    Dim card As Panel =
                    CreateManuscriptCard(
                        manuscript,
                        pipelinePanel
                    )

                    pipelinePanel.Controls.Add(
                    card
                )

                    pipelineCount += 1


                Case ManuscriptLocation.Published

                    Dim card As Panel =
                    CreateManuscriptCard(
                        manuscript,
                        publishedPanel
                    )

                    publishedPanel.Controls.Add(
                    card
                )

                    publishedCount += 1


                Case ManuscriptLocation.FileDrawer

                    Dim card As Panel =
                    CreateManuscriptCard(
                        manuscript,
                        fileDrawerPanel
                    )

                    fileDrawerPanel.Controls.Add(
                    card
                )

                    drawerCount += 1

            End Select

        Next


        ' =================================================
        ' Section headings
        ' =================================================

        lblPipelineHeader.Text =
        BuildSectionTitle(
            "PIPELINE",
            pipelineCount,
            pipelineTotal
        )

        lblPublishedHeader.Text =
        BuildSectionTitle(
            "PUBLISHED",
            publishedCount,
            publishedTotal
        )

        lblFileDrawerHeader.Text =
        BuildSectionTitle(
            "FILE DRAWER",
            drawerCount,
            drawerTotal
        )


        ' =================================================
        ' Empty states
        ' =================================================

        If pipelineCount = 0 Then

            Dim pipelineMessage As String

            If pipelineTotal = 0 Then

                pipelineMessage =
                "No active manuscripts. Click + Add Manuscript."

            Else

                pipelineMessage =
                "No Pipeline manuscripts match the current search or filter."

            End If

            pipelinePanel.Controls.Add(
            CreateEmptyLabel(
                pipelineMessage
            )
        )

        End If


        If publishedCount = 0 Then

            Dim publishedMessage As String

            If publishedTotal = 0 Then

                publishedMessage =
                "No published manuscripts yet."

            Else

                publishedMessage =
                "No Published manuscripts match the current search or filter."

            End If

            publishedPanel.Controls.Add(
            CreateEmptyLabel(
                publishedMessage
            )
        )

        End If


        If drawerCount = 0 Then

            Dim drawerMessage As String

            If drawerTotal = 0 Then

                drawerMessage =
                "The File Drawer is empty."

            Else

                drawerMessage =
                "No File Drawer manuscripts match the current search or filter."

            End If

            fileDrawerPanel.Controls.Add(
            CreateEmptyLabel(
                drawerMessage
            )
        )

        End If


        btnClearBoardFilters.Enabled =
        HasActiveBoardFilters()


        ' =================================================
        ' Finish layout and repaint
        ' =================================================

        ResizeCards(
        pipelinePanel
    )

        ResizeCards(
        publishedPanel
    )

        ResizeCards(
        fileDrawerPanel
    )


        pipelinePanel.PerformLayout()
        publishedPanel.PerformLayout()
        fileDrawerPanel.PerformLayout()

        pipelinePanel.Invalidate(True)
        publishedPanel.Invalidate(True)
        fileDrawerPanel.Invalidate(True)

    End Sub


    Private Function CreateManuscriptCard(
    manuscript As Manuscript,
    parentPanel As FlowLayoutPanel
) As Panel

        Dim card As New RoundedPanel With {
            .Height = 122,
            .Width = GetCardWidth(parentPanel),
            .BackColor = UiTheme.CardBackground(),
            .BorderColor = UiTheme.CardBorder(),
            .BorderThickness = 1.0F,
            .CornerRadius = 14,
            .Margin = New Padding(4, 4, 4, 10),
            .Cursor = Cursors.Hand
        }


        ' =================================================
        ' Title
        ' =================================================

        Dim lblTitle As New Label With {
            .Text = manuscript.Title,
            .AutoEllipsis = True,
            .Left = 18,
            .Top = 14,
            .Height = 26,
            .Font = New Font(
                Me.Font.FontFamily,
                11.0F,
                FontStyle.Bold
            ),
            .ForeColor = UiTheme.PrimaryText(),
            .Cursor = Cursors.Hand
        }


        ' =================================================
        ' Stage pill
        ' =================================================

        Dim stageText As String =
            FormatStage(
                manuscript.CurrentStage
            )

        Dim badgeFont As New Font(
            Me.Font.FontFamily,
            8.5F,
            FontStyle.Bold
        )

        Dim badgeWidth As Integer =
            TextRenderer.MeasureText(
                stageText.ToUpperInvariant(),
                badgeFont
            ).Width + 22

        Dim stageBadge As New PillLabel With {
            .Text = stageText.ToUpperInvariant(),
            .Left = 18,
            .Top = 45,
            .Width = badgeWidth,
            .Height = 25,
            .Font = badgeFont,
            .BackColor = UiTheme.StageBackground(manuscript.CurrentStage),
            .ForeColor = UiTheme.StageForeground(manuscript.CurrentStage)
        }


        ' =================================================
        ' Journal
        ' =================================================

        Dim journalText As String

        If String.IsNullOrWhiteSpace(
            manuscript.TargetJournal
        ) Then

            journalText =
                "Target journal not set"

        Else

            journalText =
                manuscript.TargetJournal

        End If

        Dim lblJournal As New Label With {
            .Text = journalText,
            .AutoEllipsis = True,
            .Left = 18 + badgeWidth + 10,
            .Top = 47,
            .Height = 23,
            .ForeColor = UiTheme.SecondaryText(),
            .Cursor = Cursors.Hand
        }


        ' =================================================
        ' Stats
        ' =================================================

        Dim lblStats As New Label With {
            .Text =
                manuscript.SubmissionCount.ToString() &
                " submissions  •  " &
                manuscript.RejectionCount.ToString() &
                " rejections",
            .AutoSize = True,
            .Left = 18,
            .Top = 82,
            .ForeColor = UiTheme.PrimaryText(),
            .Cursor = Cursors.Hand
        }


        ' =================================================
        ' Double-click behavior
        ' =================================================

        AddHandler card.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        AddHandler lblTitle.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        AddHandler lblJournal.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        AddHandler lblStats.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub


        ' =================================================
        ' Delete
        ' =================================================

        Dim btnDelete As New Button With {
            .Text = "Delete",
            .Width = 88,
            .Height = 34,
            .Top = 44,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        btnDelete.Left =
            card.ClientSize.Width -
            btnDelete.Width -
            18

        StyleCardButton(
            btnDelete,
            UiTheme.DangerColor()
        )

        AddHandler btnDelete.Click,
            Sub(sender, e)
                DeleteManuscript(manuscript)
            End Sub

        Dim nextRight As Integer =
            btnDelete.Left - 10


        ' =================================================
        ' Location-specific action
        ' =================================================

        If manuscript.Location =
            ManuscriptLocation.Pipeline Then

            Dim btnMoveToDrawer As New Button With {
                .Text = "Move to File Drawer",
                .Height = 34,
                .Top = 44,
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            }

            btnMoveToDrawer.Width =
                Math.Max(
                    175,
                    TextRenderer.MeasureText(
                        btnMoveToDrawer.Text,
                        btnMoveToDrawer.Font
                    ).Width + 32
                )

            btnMoveToDrawer.Left =
                nextRight -
                btnMoveToDrawer.Width

            nextRight =
                btnMoveToDrawer.Left - 10

            StyleCardButton(
                btnMoveToDrawer,
                UiTheme.WarningColor()
            )

            AddHandler btnMoveToDrawer.Click,
                Sub(sender, e)
                    MoveToFileDrawer(manuscript)
                End Sub

            card.Controls.Add(
                btnMoveToDrawer
            )

        ElseIf manuscript.Location =
            ManuscriptLocation.FileDrawer Then

            Dim btnRestoreToPipeline As New Button With {
                .Text = "Restore to Pipeline",
                .Height = 34,
                .Top = 44,
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right
            }

            btnRestoreToPipeline.Width =
                Math.Max(
                    175,
                    TextRenderer.MeasureText(
                        btnRestoreToPipeline.Text,
                        btnRestoreToPipeline.Font
                    ).Width + 32
                )

            btnRestoreToPipeline.Left =
                nextRight -
                btnRestoreToPipeline.Width

            nextRight =
                btnRestoreToPipeline.Left - 10

            StyleCardButton(
                btnRestoreToPipeline,
                UiTheme.SuccessColor()
            )

            AddHandler btnRestoreToPipeline.Click,
                Sub(sender, e)
                    RestoreToPipeline(manuscript)
                End Sub

            card.Controls.Add(
                btnRestoreToPipeline
            )

        End If

        ' =================================================
        ' Open
        ' =================================================

        Dim btnOpen As New Button With {
            .Text = "Open",
            .Width = 88,
            .Height = 34,
            .Top = 44,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }

        btnOpen.Left =
            nextRight -
            btnOpen.Width

        StyleCardButton(
            btnOpen,
            UiTheme.AccentColor()
        )

        AddHandler btnOpen.Click,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub


        ' =================================================
        ' Responsive text width
        ' =================================================

        Dim textWidth As Integer =
            Math.Max(
                220,
                btnOpen.Left - 36
            )

        lblTitle.Width =
            textWidth

        lblJournal.Width =
            Math.Max(
                80,
                textWidth -
                badgeWidth -
                10
            )

        lblTitle.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right

        lblJournal.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right


        ' =================================================
        ' Assemble
        ' =================================================

        card.Controls.Add(lblTitle)
        card.Controls.Add(stageBadge)
        card.Controls.Add(lblJournal)
        card.Controls.Add(lblStats)
        card.Controls.Add(btnOpen)
        card.Controls.Add(btnDelete)


        ' =================================================
        ' File Drawer suggestion
        ' =================================================

        Dim insightColor As Color

        Dim insightText As String =
    BuildManuscriptInsight(
        manuscript,
        insightColor
    )


        If Not String.IsNullOrWhiteSpace(
    insightText
) Then

            card.Height =
        148

            Dim lblInsight As New Label With {
        .Text = insightText,
        .AutoSize = True,
        .Left = 18,
        .Top = 111,
        .ForeColor = insightColor,
        .Font = New Font(
            Me.Font.FontFamily,
            9.0F,
            FontStyle.Bold
        )
    }

            card.Controls.Add(
        lblInsight
    )

        End If

        Return card

    End Function

    Private Function CreateEmptyLabel(
    text As String
) As Label

        Return New Label With {
        .Text = text,
        .AutoSize = False,
        .Width = 650,
        .Height = 48,
        .Padding = New Padding(12),
        .ForeColor = UiTheme.SecondaryText(),
        .BackColor = UiTheme.BoardBackground()
    }

    End Function


    Private Function GetCardWidth(
        panel As FlowLayoutPanel
    ) As Integer

        Return Math.Max(
            500,
            panel.ClientSize.Width -
            panel.Padding.Horizontal -
            30
        )

    End Function


    Private Sub ResizeCards(
        panel As FlowLayoutPanel
    )

        Dim newWidth As Integer =
            GetCardWidth(
                panel
            )

        For Each control As Control In panel.Controls

            If TypeOf control Is Panel Then

                control.Width =
                    newWidth

            ElseIf TypeOf control Is Label Then

                control.Width =
                    newWidth

            End If

        Next

    End Sub


    ' =====================================================
    ' Add / open
    ' =====================================================

    Private Sub AddManuscript(
        sender As Object,
        e As EventArgs
    )

        Using dialog As New AddManuscriptForm()

            If dialog.ShowDialog(Me) =
                DialogResult.OK AndAlso
               dialog.CreatedManuscript IsNot Nothing Then

                manuscripts.Add(
                    dialog.CreatedManuscript
                )

                SaveManuscripts()
                RenderManuscripts()

            End If

        End Using

    End Sub


    Private Sub OpenManuscript(
        manuscript As Manuscript
    )

        Using dialog As New EditManuscriptForm(
            manuscript
        )

            Dim result As DialogResult =
                dialog.ShowDialog(Me)

            If dialog.DeleteRequested Then

                manuscripts.Remove(
                    manuscript
                )

                SaveManuscripts()
                RenderManuscripts()

                Return

            End If

            If result =
                DialogResult.OK Then

                SaveManuscripts()
                RenderManuscripts()

            End If

        End Using

    End Sub


    ' =====================================================
    ' Delete manuscript
    ' =====================================================

    Private Sub DeleteManuscript(
        manuscript As Manuscript
    )

        Using dialog As New DeleteManuscriptForm(
            manuscript.Title
        )

            If dialog.ShowDialog(Me) <>
                DialogResult.OK Then

                Return

            End If

        End Using

        manuscripts.Remove(
            manuscript
        )

        SaveManuscripts()
        RenderManuscripts()

    End Sub


    ' =====================================================
    ' File Drawer
    ' =====================================================

    Private Sub MoveToFileDrawer(
        manuscript As Manuscript
    )

        Dim result As DialogResult =
            MessageBox.Show(
                Me,
                "Move '" &
                manuscript.Title &
                "' to the File Drawer?" &
                Environment.NewLine &
                Environment.NewLine &
                "Its history, submissions, decisions, and correspondence will be preserved.",
                "Move to File Drawer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

        If result <>
            DialogResult.Yes Then

            Return

        End If

        Dim reason As String =
            Microsoft.VisualBasic.Interaction.InputBox(
                "Optional: Why are you filing this manuscript?",
                "File Drawer",
                ""
            )

        manuscript.Location =
            ManuscriptLocation.FileDrawer

        manuscript.FileDrawerDate =
            DateTime.Now

        manuscript.FileDrawerReason =
            reason.Trim()

        Dim historyNote As String =
            "Moved to File Drawer."

        If Not String.IsNullOrWhiteSpace(
            reason
        ) Then

            historyNote &=
                " Reason: " &
                reason.Trim()

        End If

        manuscript.History.Add(
            New HistoryEvent With {
                .Stage =
                    manuscript.CurrentStage,
                .Note =
                    historyNote
            }
        )

        SaveManuscripts()
        RenderManuscripts()

    End Sub


    Private Sub RestoreToPipeline(
        manuscript As Manuscript
    )

        manuscript.Location =
            ManuscriptLocation.Pipeline

        manuscript.FileDrawerDate =
            Nothing

        manuscript.FileDrawerReason =
            String.Empty

        manuscript.History.Add(
            New HistoryEvent With {
                .Stage =
                    manuscript.CurrentStage,
                .Note =
                    "Restored from File Drawer to active Pipeline."
            }
        )

        SaveManuscripts()
        RenderManuscripts()

    End Sub

    ' =====================================================
    ' Standard Excel template
    ' =====================================================

    Private Sub ExportBlankTemplate(
    sender As Object,
    e As EventArgs
)

        Using dialog As New SaveFileDialog()

            dialog.Title =
            "Save PaperRoute Import Template"

            dialog.Filter =
            "Excel workbook (*.xlsx)|*.xlsx"

            dialog.DefaultExt =
            "xlsx"

            dialog.AddExtension =
            True

            dialog.FileName =
            "PaperRoute_Import_Template.xlsx"

            dialog.OverwritePrompt =
            True

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Try

                Dim generator As New StandardTemplateGenerator()

                generator.Generate(
                dialog.FileName
            )

                MessageBox.Show(
                Me,
                "The PaperRoute import template was created successfully." &
                Environment.NewLine &
                Environment.NewLine &
                dialog.FileName,
                "Template Created",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not create the Excel template." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Template Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            End Try

        End Using

    End Sub


    ' =====================================================
    ' Export complete library
    ' =====================================================

    Private Sub ExportLibraryExcel(
    sender As Object,
    e As EventArgs
)


        If manuscripts.Count = 0 Then

            MessageBox.Show(
            Me,
            "There are no manuscripts to export.",
            "Nothing to Export",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

            Return

        End If

        Using dialog As New SaveFileDialog()

            dialog.Title =
            "Export PaperRoute Library"

            dialog.Filter =
            "Excel workbook (*.xlsx)|*.xlsx"

            dialog.DefaultExt =
            "xlsx"

            dialog.AddExtension =
            True

            dialog.FileName =
            "PaperRoute_Export_" &
            DateTime.Now.ToString("yyyy-MM-dd") &
            ".xlsx"

            dialog.OverwritePrompt =
            True

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Try

                Dim exporter As New LibraryExcelExporter()

                exporter.Export(
                dialog.FileName,
                manuscripts
            )

                MessageBox.Show(
                Me,
                "Your PaperRoute library was exported successfully." &
                Environment.NewLine &
                Environment.NewLine &
                manuscripts.Count.ToString() &
                " manuscript(s)" &
                Environment.NewLine &
                Environment.NewLine &
                dialog.FileName &
                Environment.NewLine &
                Environment.NewLine &
                "Note: The workbook contains correspondence metadata and file paths. It does not embed the actual document files.",
                "Export Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not export the library." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Export Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            End Try

        End Using

    End Sub

    ' =====================================================
    ' Portable backup
    ' =====================================================

    Private Sub BackupLibrary(
    sender As Object,
    e As EventArgs
)

        If manuscripts.Count = 0 Then

            MessageBox.Show(
            Me,
            "There are no manuscripts to back up.",
            "Nothing to Back Up",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

            Return

        End If

        ' Make absolutely sure JSON and managed copies are current.
        If Not SaveManuscripts() Then
            Return
        End If

        Using dialog As New SaveFileDialog()

            dialog.Title =
            "Back Up PaperRoute Library"

            dialog.Filter =
            "ZIP archive (*.zip)|*.zip"

            dialog.DefaultExt =
            "zip"

            dialog.AddExtension =
            True

            dialog.FileName =
            "PaperRoute_Backup_" &
            DateTime.Now.ToString("yyyy-MM-dd_HHmmss") &
            ".zip"

            dialog.OverwritePrompt =
            True

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Try

                Dim backupService As New PortableBackupService()

                backupService.CreateBackup(
                dialog.FileName,
                manuscripts,
                repository
            )

                MessageBox.Show(
                Me,
                "Your PaperRoute library was backed up successfully." &
                Environment.NewLine &
                Environment.NewLine &
                dialog.FileName &
                Environment.NewLine &
                Environment.NewLine &
                "The backup contains:" &
                Environment.NewLine &
                "- Native PaperRoute data" &
                Environment.NewLine &
                "- Excel library export" &
                Environment.NewLine &
                "- Managed document files",
                "Backup Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not create the backup." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Backup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            End Try

        End Using

    End Sub

    ' =====================================================
    ' Portable restore
    ' =====================================================

    Private Sub RestoreLibraryBackup(
    sender As Object,
    e As EventArgs
)

        Using dialog As New OpenFileDialog()

            dialog.Title =
            "Restore PaperRoute Backup"

            dialog.Filter =
            "PaperRoute backup (*.zip)|*.zip|ZIP archives (*.zip)|*.zip"

            dialog.CheckFileExists =
            True

            dialog.Multiselect =
            False

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Dim restoreService As New PortableRestoreService()
            Dim inspection As BackupInspection

            Try

                inspection =
                restoreService.InspectBackup(
                    dialog.FileName
                )

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "This backup could not be validated." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Invalid Backup",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

                Return

            End Try

            Dim sizeInMb As Double =
            inspection.UncompressedBytes / 1024.0 / 1024.0

            Dim preview As String =
            "Restore this PaperRoute backup?" &
            Environment.NewLine &
            Environment.NewLine &
            "Manuscripts: " &
            inspection.ManuscriptCount.ToString() &
            Environment.NewLine &
            "Submissions: " &
            inspection.SubmissionCount.ToString() &
            Environment.NewLine &
            "Editorial decisions: " &
            inspection.DecisionCount.ToString() &
            Environment.NewLine &
            "Correspondence records: " &
            inspection.CorrespondenceCount.ToString() &
            Environment.NewLine &
            "Managed files: " &
            inspection.ManagedFileCount.ToString() &
            Environment.NewLine &
            "Archive entries: " &
            inspection.ArchiveEntryCount.ToString() &
            Environment.NewLine &
            "Expanded size: " &
            sizeInMb.ToString("N1") &
            " MB" &
            Environment.NewLine &
            Environment.NewLine &
            "IMPORTANT: This will REPLACE the library currently loaded in PaperRoute." &
            Environment.NewLine &
            Environment.NewLine &
            "An emergency backup of your current library will be created before the restore begins."

            Dim confirmation As DialogResult =
            MessageBox.Show(
                Me,
                preview,
                "Restore Backup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            )

            If confirmation <> DialogResult.Yes Then
                Return
            End If

            Dim typedConfirmation As String =
            Microsoft.VisualBasic.Interaction.InputBox(
                "Type RESTORE to replace your current PaperRoute library.",
                "Confirm Restore",
                ""
            )

            If Not String.Equals(
            typedConfirmation.Trim(),
            "RESTORE",
            StringComparison.Ordinal
        ) Then

                MessageBox.Show(
                Me,
                "Restore cancelled. The confirmation text did not match RESTORE.",
                "Restore Cancelled",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

                Return

            End If

            Try

                Dim restoreResult As RestoreResult =
                restoreService.RestoreBackup(
                    dialog.FileName,
                    manuscripts,
                    repository
                )

                manuscripts =
                repository.Load()

                RenderManuscripts()

                Dim completionMessage As String =
                "The PaperRoute backup was restored successfully." &
                Environment.NewLine &
                Environment.NewLine &
                restoreResult.ManuscriptCount.ToString() &
                " manuscript(s) restored."

                If Not String.IsNullOrWhiteSpace(
                restoreResult.EmergencyBackupPath
            ) Then

                    completionMessage &=
                    Environment.NewLine &
                    Environment.NewLine &
                    "Your previous library was backed up automatically to:" &
                    Environment.NewLine &
                    restoreResult.EmergencyBackupPath

                End If

                MessageBox.Show(
                Me,
                completionMessage,
                "Restore Complete",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

                lblStatus.Text =
                "Backup restored - " &
                DateTime.Now.ToString("h:mm tt")

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not restore the backup." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine &
                "The existing library was preserved or rolled back where possible.",
                "Restore Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            End Try

        End Using

    End Sub

    ' =====================================================
    ' Excel import
    ' =====================================================

    Private Sub ImportExcelHistory(
    sender As Object,
    e As EventArgs
)

        Using dialog As New OpenFileDialog()

            dialog.Title = "Import Spreadsheet into PaperRoute"
            dialog.Filter = "Excel workbooks (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All files (*.*)|*.*"
            dialog.CheckFileExists = True
            dialog.Multiselect = False

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Dim importResult As ExcelImportResult
            Dim detectedFormat As String

            Try

                Dim isStandardTemplate As Boolean = False

                Using workbook As New ClosedXML.Excel.XLWorkbook(dialog.FileName)

                    Dim hasManuscripts As Boolean = False
                    Dim hasSubmissions As Boolean = False
                    Dim hasDecisions As Boolean = False
                    Dim hasCorrespondence As Boolean = False

                    For Each worksheet As ClosedXML.Excel.IXLWorksheet In workbook.Worksheets

                        Select Case worksheet.Name.Trim().ToUpperInvariant()

                            Case "MANUSCRIPTS"
                                hasManuscripts = True

                            Case "SUBMISSIONS"
                                hasSubmissions = True

                            Case "DECISIONS"
                                hasDecisions = True

                            Case "CORRESPONDENCE"
                                hasCorrespondence = True

                        End Select

                    Next

                    isStandardTemplate =
                    hasManuscripts AndAlso
                    hasSubmissions AndAlso
                    hasDecisions AndAlso
                    hasCorrespondence

                End Using

                If isStandardTemplate Then

                    Dim importer As New StandardExcelImporter()

                    importResult = importer.Import(dialog.FileName)
                    detectedFormat = "Standard PaperRoute template"

                Else

                    Dim legacyImporter As New LegacyExcelImporter()

                    If legacyImporter.CanImport(dialog.FileName) Then

                        importResult = legacyImporter.Import(dialog.FileName)
                        detectedFormat = "Legacy tracker"

                    Else

                        Using mappingDialog As New ExcelMappingForm(dialog.FileName)

                            If mappingDialog.ShowDialog(Me) <> DialogResult.OK Then
                                Return
                            End If

                            Dim flexibleImporter As New FlexibleExcelImporter()

                            importResult =
                                flexibleImporter.Import(
                                    dialog.FileName,
                                    mappingDialog.SelectedWorksheetName,
                                    mappingDialog.HeaderRow,
                                    mappingDialog.Mappings
                                )

                            detectedFormat = "Mapped spreadsheet"

                        End Using

                    End If

                End If

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not read this workbook." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Excel Import Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

                Return

            End Try


            Dim existingTitles As New HashSet(Of String)(
            StringComparer.OrdinalIgnoreCase
        )

            For Each existingManuscript As Manuscript In manuscripts

                If Not String.IsNullOrWhiteSpace(existingManuscript.Title) Then
                    existingTitles.Add(existingManuscript.Title.Trim())
                End If

            Next


            Dim manuscriptsToAdd As New List(Of Manuscript)()
            Dim duplicateCount As Integer = 0

            For Each importedManuscript As Manuscript In importResult.Manuscripts

                If existingTitles.Contains(importedManuscript.Title.Trim()) Then

                    duplicateCount += 1

                Else

                    manuscriptsToAdd.Add(importedManuscript)

                End If

            Next


            If manuscriptsToAdd.Count = 0 Then

                MessageBox.Show(
                Me,
                "No new manuscripts are available to import." &
                Environment.NewLine &
                Environment.NewLine &
                duplicateCount.ToString() &
                " manuscript(s) matched titles already in PaperRoute.",
                "Nothing to Import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

                Return

            End If


            Dim submissionCount As Integer = 0
            Dim decisionCount As Integer = 0
            Dim correspondenceCount As Integer = 0

            For Each importedManuscript As Manuscript In manuscriptsToAdd

                submissionCount += importedManuscript.Submissions.Count

                For Each submission As JournalSubmission In importedManuscript.Submissions

                    decisionCount += submission.Decisions.Count
                    correspondenceCount += submission.Correspondence.Count

                Next

            Next


            Dim preview As String =
            "Detected format: " &
            detectedFormat &
            Environment.NewLine &
            Environment.NewLine &
            "Manuscripts to add: " &
            manuscriptsToAdd.Count.ToString() &
            Environment.NewLine &
            "Journal submissions: " &
            submissionCount.ToString() &
            Environment.NewLine &
            "Editorial decisions: " &
            decisionCount.ToString() &
            Environment.NewLine &
            "Correspondence/files: " &
            correspondenceCount.ToString()


            If duplicateCount > 0 Then

                preview &=
                Environment.NewLine &
                "Existing manuscript titles skipped: " &
                duplicateCount.ToString()

            End If


            If importResult.Warnings.Count > 0 Then

                preview &=
                Environment.NewLine &
                "Import warnings: " &
                importResult.Warnings.Count.ToString()

                Dim warningLimit As Integer =
                Math.Min(5, importResult.Warnings.Count)

                preview &=
                Environment.NewLine &
                Environment.NewLine &
                "First warnings:"

                For i As Integer = 0 To warningLimit - 1

                    preview &=
                    Environment.NewLine &
                    "- " &
                    importResult.Warnings(i)

                Next

                If importResult.Warnings.Count > warningLimit Then

                    preview &=
                    Environment.NewLine &
                    "- ...and " &
                    (importResult.Warnings.Count - warningLimit).ToString() &
                    " more."

                End If

            End If


            preview &=
            Environment.NewLine &
            Environment.NewLine &
            "Import these records?"


            Dim confirmation As DialogResult =
            MessageBox.Show(
                Me,
                preview,
                "Excel Import Preview",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If confirmation <> DialogResult.Yes Then
                Return
            End If


            Try

                repository.CreatePreImportBackup()

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "PaperRoute could not create the pre-import backup." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message &
                Environment.NewLine &
                Environment.NewLine &
                "The import has been cancelled.",
                "Backup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

                Return

            End Try


            For Each importedManuscript As Manuscript In manuscriptsToAdd
                manuscripts.Add(importedManuscript)
            Next


            If Not SaveManuscripts() Then

                For Each importedManuscript As Manuscript In manuscriptsToAdd
                    manuscripts.Remove(importedManuscript)
                Next

                Return

            End If


            RenderManuscripts()

            MessageBox.Show(
            Me,
            manuscriptsToAdd.Count.ToString() &
            " manuscript(s) were imported successfully." &
            Environment.NewLine &
            Environment.NewLine &
            "Format: " &
            detectedFormat,
            "Import Complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

        End Using

    End Sub

    ' =====================================================
    ' Updates
    ' =====================================================

    Private Async Sub BeginAutomaticUpdateCheck()

        ' Let the main window finish rendering before any update prompt appears.
        Await Task.Delay(1500)

        Await UpdateService.CheckAndOfferUpdateAsync(
            Me,
            appSettings.UpdateChannel,
            False
        )

    End Sub


    Private Async Sub CheckForUpdatesNow(
        sender As Object,
        e As EventArgs
    )

        Await UpdateService.CheckAndOfferUpdateAsync(
            Me,
            appSettings.UpdateChannel,
            True
        )

    End Sub


    ' =====================================================
    ' Settings
    ' =====================================================

    Private Sub OpenSettings(
    sender As Object,
    e As EventArgs
)

        Using dialog As New SettingsForm(
        appSettings
    )

            If dialog.ShowDialog(Me) <>
            DialogResult.OK Then

                Return

            End If

            RenderManuscripts()

            If dialog.AppearanceChanged Then

                Dim restartResult As DialogResult =
                MessageBox.Show(
                    Me,
                    "Restart PaperRoute now to apply the new appearance?",
                    "Restart Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                )

                If restartResult =
                DialogResult.Yes Then

                    Application.Restart()

                End If

            End If

        End Using

    End Sub


    Private Sub OpenDiagnostics(
        sender As Object,
        e As EventArgs
    )

        Using dialog As New DiagnosticsForm(
            appSettings
        )

            dialog.ShowDialog(Me)

        End Using

    End Sub

    ' =====================================================
    ' Formatting
    ' =====================================================

    Private Function FormatStage(
        stage As PaperStage
    ) As String

        Select Case stage

            Case PaperStage.Idea
                Return "Idea"

            Case PaperStage.Draft
                Return "Draft"

            Case PaperStage.Submitted
                Return "Submitted"

            Case PaperStage.UnderReview
                Return "Under Review"

            Case PaperStage.Revision
                Return "Revision"

            Case PaperStage.Accepted
                Return "Accepted"

            Case PaperStage.InPress
                Return "In Press"

            Case PaperStage.Published
                Return "Published"

            Case Else
                Return stage.ToString()

        End Select

    End Function

End Class