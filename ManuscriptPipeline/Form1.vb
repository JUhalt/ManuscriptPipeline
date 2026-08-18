Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

Public Class Form1

    Private Const FileDrawerSuggestionThreshold As Integer = 3

    Private manuscripts As New List(Of Manuscript)()

    Private ReadOnly repository As New ManuscriptRepository()

    Private ReadOnly pipelinePanel As New FlowLayoutPanel()
    Private ReadOnly fileDrawerPanel As New FlowLayoutPanel()
    Private ReadOnly lblStatus As New Label()

    Private uiInitialized As Boolean = False


    ' =====================================================
    ' Startup
    ' =====================================================

    Private Sub Form1_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        If uiInitialized Then
            Return
        End If

        uiInitialized = True

        BuildInterface()
        LoadManuscripts()
        RenderManuscripts()

    End Sub


    ' =====================================================
    ' Interface
    ' =====================================================

    Private Sub BuildInterface()

        Me.Controls.Clear()

        Me.Text = "ManuscriptPipeline"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1100, 760)
        Me.MinimumSize = New Size(850, 600)
        Me.Font = New Font("Segoe UI", 10.0F)
        Me.AutoScaleMode = AutoScaleMode.Dpi
        Me.BackColor = SystemColors.Control
        Me.DoubleBuffered = True

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = SystemColors.Control
        }

        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 78))
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))

        ' =================================================
        ' Header
        ' =================================================

        Dim header As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(18, 12, 18, 10),
            .BackColor = SystemColors.Window
        }

        Dim lblTitle As New Label With {
            .Text = "ManuscriptPipeline",
            .AutoSize = True,
            .Left = 18,
            .Top = 12,
            .Font = New Font(Me.Font.FontFamily, 16, FontStyle.Bold)
        }

        Dim lblSubtitle As New Label With {
            .Text = "Local-first academic manuscript tracking",
            .AutoSize = True,
            .Left = 20,
            .Top = 44,
            .ForeColor = SystemColors.GrayText
        }

        Dim headerActions As New FlowLayoutPanel With {
    .Dock = DockStyle.Right,
    .AutoSize = True,
    .FlowDirection = FlowDirection.RightToLeft,
    .WrapContents = False,
    .Padding = New Padding(0, 2, 0, 2)
}

        Dim btnAdd As New Button With {
    .Text = "+ Add Manuscript",
    .Width = 175,
    .Height = 44
}

        Dim btnImport As New Button With {
    .Text = "Import Excel...",
    .Width = 145,
    .Height = 44
}

        AddHandler btnAdd.Click, AddressOf AddManuscript
        AddHandler btnImport.Click, AddressOf ImportExcelHistory

        headerActions.Controls.Add(btnAdd)
        headerActions.Controls.Add(btnImport)

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblSubtitle)
        header.Controls.Add(headerActions)

        AddHandler btnAdd.Click,
            AddressOf AddManuscript

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblSubtitle)
        header.Controls.Add(btnAdd)

        ' =================================================
        ' Main body
        ' =================================================

        Dim body As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 4,
            .Padding = New Padding(18, 12, 18, 12)
        }

        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 55))
        body.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        body.RowStyles.Add(New RowStyle(SizeType.Percent, 45))

        Dim lblPipeline As New Label With {
            .Text = "PIPELINE",
            .AutoSize = True,
            .Anchor = AnchorStyles.Left,
            .Font = New Font(Me.Font, FontStyle.Bold)
        }

        Dim lblDrawer As New Label With {
            .Text = "FILE DRAWER",
            .AutoSize = True,
            .Anchor = AnchorStyles.Left,
            .Font = New Font(Me.Font, FontStyle.Bold)
        }

        ConfigureFlowPanel(pipelinePanel)
        ConfigureFlowPanel(fileDrawerPanel)

        body.Controls.Add(lblPipeline, 0, 0)
        body.Controls.Add(pipelinePanel, 0, 1)

        body.Controls.Add(lblDrawer, 0, 2)
        body.Controls.Add(fileDrawerPanel, 0, 3)

        ' =================================================
        ' Footer
        ' =================================================

        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        lblStatus.Padding = New Padding(18, 0, 0, 0)
        lblStatus.ForeColor = SystemColors.GrayText
        lblStatus.Text = "Local storage enabled."

        root.Controls.Add(header, 0, 0)
        root.Controls.Add(body, 0, 1)
        root.Controls.Add(lblStatus, 0, 2)

        Me.Controls.Add(root)

        AddHandler pipelinePanel.Resize,
            Sub(sender, e)
                ResizeCards(pipelinePanel)
            End Sub

        AddHandler fileDrawerPanel.Resize,
            Sub(sender, e)
                ResizeCards(fileDrawerPanel)
            End Sub

    End Sub


    ' =====================================================
    ' Persistence
    ' =====================================================

    Private Sub LoadManuscripts()

        Try

            manuscripts = repository.Load()

            lblStatus.Text =
                "Loaded " &
                manuscripts.Count.ToString() &
                " manuscript(s) from local storage."

        Catch ex As Exception

            manuscripts = New List(Of Manuscript)()

            MessageBox.Show(
                Me,
                "ManuscriptPipeline could not load your saved data." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Data Load Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            lblStatus.Text =
                "Saved data could not be loaded."

        End Try

    End Sub


    Private Function SaveManuscripts() As Boolean

        Try

            repository.Save(manuscripts)

            lblStatus.Text =
                "Saved locally - " &
                DateTime.Now.ToString("h:mm tt")

            Return True

        Catch ex As Exception

            MessageBox.Show(
                Me,
                "ManuscriptPipeline could not save your data." &
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
        panel.Padding = New Padding(8)
        panel.BackColor = SystemColors.Window
        panel.BorderStyle = BorderStyle.FixedSingle

    End Sub


    ' =====================================================
    ' Render manuscripts
    ' =====================================================

    Private Sub RenderManuscripts()

        pipelinePanel.SuspendLayout()
        fileDrawerPanel.SuspendLayout()

        pipelinePanel.Controls.Clear()
        fileDrawerPanel.Controls.Clear()

        Dim pipelineCount As Integer = 0
        Dim drawerCount As Integer = 0

        For Each manuscript As Manuscript In manuscripts

            If manuscript.Location = ManuscriptLocation.Pipeline Then

                pipelinePanel.Controls.Add(
                    CreateManuscriptCard(
                        manuscript,
                        pipelinePanel
                    )
                )

                pipelineCount += 1

            Else

                fileDrawerPanel.Controls.Add(
                    CreateManuscriptCard(
                        manuscript,
                        fileDrawerPanel
                    )
                )

                drawerCount += 1

            End If

        Next

        If pipelineCount = 0 Then

            pipelinePanel.Controls.Add(
                CreateEmptyLabel(
                    "No active manuscripts yet. Click + Add Manuscript."
                )
            )

        End If

        If drawerCount = 0 Then

            fileDrawerPanel.Controls.Add(
                CreateEmptyLabel(
                    "The File Drawer is empty."
                )
            )

        End If

        pipelinePanel.ResumeLayout()
        fileDrawerPanel.ResumeLayout()

        ResizeCards(pipelinePanel)
        ResizeCards(fileDrawerPanel)

    End Sub


    Private Function CreateManuscriptCard(
        manuscript As Manuscript,
        parentPanel As FlowLayoutPanel
    ) As Panel

        Dim card As New Panel With {
            .Height = 118,
            .Width = GetCardWidth(parentPanel),
            .BackColor = SystemColors.Window,
            .BorderStyle = BorderStyle.FixedSingle,
            .Margin = New Padding(4, 4, 4, 8),
            .Cursor = Cursors.Hand
        }

        Dim lblTitle As New Label With {
            .Text = manuscript.Title,
            .AutoEllipsis = True,
            .Left = 16,
            .Top = 12,
            .Height = 25,
            .Font = New Font(
                Me.Font.FontFamily,
                11,
                FontStyle.Bold
            ),
            .Cursor = Cursors.Hand
        }

        Dim journalText As String

        If String.IsNullOrWhiteSpace(
            manuscript.TargetJournal
        ) Then

            journalText =
                "Target journal: Not set"

        Else

            journalText =
                "Target journal: " &
                manuscript.TargetJournal

        End If

        Dim lblStage As New Label With {
            .Text =
                FormatStage(manuscript.CurrentStage) &
                " - " &
                journalText,
            .AutoEllipsis = True,
            .Left = 16,
            .Top = 42,
            .Height = 23,
            .ForeColor = SystemColors.GrayText,
            .Cursor = Cursors.Hand
        }

        Dim lblStats As New Label With {
            .Text =
                manuscript.SubmissionCount.ToString() &
                " submissions - " &
                manuscript.RejectionCount.ToString() &
                " rejections",
            .AutoSize = True,
            .Left = 16,
            .Top = 73,
            .Cursor = Cursors.Hand
        }

        AddHandler card.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        AddHandler lblTitle.DoubleClick,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        AddHandler lblStage.DoubleClick,
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
            .Width = 90,
            .Height = 34,
            .Top = 41,
            .Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Right,
            .Cursor = Cursors.Default
        }

        btnDelete.Left =
            card.ClientSize.Width -
            btnDelete.Width -
            16

        AddHandler btnDelete.Click,
            Sub(sender, e)
                DeleteManuscript(manuscript)
            End Sub

        ' =================================================
        ' Location
        ' =================================================

        Dim btnLocation As New Button With {
            .Height = 34,
            .Top = 41,
            .Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Right,
            .Cursor = Cursors.Default
        }

        If manuscript.Location =
            ManuscriptLocation.Pipeline Then

            btnLocation.Text =
                "Move to File Drawer"

            AddHandler btnLocation.Click,
                Sub(sender, e)
                    MoveToFileDrawer(manuscript)
                End Sub

        Else

            btnLocation.Text =
                "Restore to Pipeline"

            AddHandler btnLocation.Click,
                Sub(sender, e)
                    RestoreToPipeline(manuscript)
                End Sub

        End If

        Dim locationWidth As Integer =
            TextRenderer.MeasureText(
                btnLocation.Text,
                btnLocation.Font
            ).Width + 34

        btnLocation.Width =
            Math.Max(
                175,
                locationWidth
            )

        btnLocation.Left =
            btnDelete.Left -
            btnLocation.Width -
            8

        ' =================================================
        ' Open
        ' =================================================

        Dim btnOpen As New Button With {
            .Text = "Open",
            .Width = 88,
            .Height = 34,
            .Top = 41,
            .Left =
                btnLocation.Left - 96,
            .Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Right,
            .Cursor = Cursors.Default
        }

        AddHandler btnOpen.Click,
            Sub(sender, e)
                OpenManuscript(manuscript)
            End Sub

        Dim textWidth As Integer =
            Math.Max(
                200,
                btnOpen.Left - 32
            )

        lblTitle.Width =
            textWidth

        lblStage.Width =
            textWidth

        lblTitle.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right

        lblStage.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right

        card.Controls.Add(lblTitle)
        card.Controls.Add(lblStage)
        card.Controls.Add(lblStats)
        card.Controls.Add(btnOpen)
        card.Controls.Add(btnLocation)
        card.Controls.Add(btnDelete)

        ' =================================================
        ' File Drawer suggestion
        ' =================================================

        If manuscript.Location =
                ManuscriptLocation.Pipeline AndAlso
           manuscript.RejectionCount >=
                FileDrawerSuggestionThreshold Then

            card.Height = 142

            Dim lblSuggestion As New Label With {
                .Text =
                    "File Drawer suggestion: " &
                    manuscript.RejectionCount.ToString() &
                    " rejections",
                .AutoSize = True,
                .Left = 16,
                .Top = 99,
                .ForeColor = SystemColors.HotTrack,
                .Font = New Font(
                    Me.Font,
                    FontStyle.Bold
                )
            }

            card.Controls.Add(
                lblSuggestion
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
            .Height = 45,
            .Padding = New Padding(10),
            .ForeColor = SystemColors.GrayText
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
            GetCardWidth(panel)

        For Each control As Control In
            panel.Controls

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
    ' Delete
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
                "Its history and submission records will be preserved.",
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
    ' Formatting
    ' =====================================================

    Private Sub ImportExcelHistory(
    sender As Object,
    e As EventArgs
)

        Using dialog As New OpenFileDialog()

            dialog.Title =
            "Import Manuscript History"

            dialog.Filter =
            "Excel workbooks (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All files (*.*)|*.*"

            dialog.CheckFileExists =
            True

            dialog.Multiselect =
            False

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Dim importer As New LegacyExcelImporter()
            Dim importResult As ExcelImportResult

            Try

                importResult =
                importer.Import(
                    dialog.FileName
                )

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "ManuscriptPipeline could not read this workbook." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Excel Import Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

                Return

            End Try


            ' =================================================
            ' Prevent accidental duplicates
            ' =================================================

            Dim existingTitles As New HashSet(Of String)(
            StringComparer.OrdinalIgnoreCase
        )

            For Each existingManuscript As Manuscript In manuscripts

                If Not String.IsNullOrWhiteSpace(
                existingManuscript.Title
            ) Then

                    existingTitles.Add(
                    existingManuscript.Title.Trim()
                )

                End If

            Next

            Dim manuscriptsToAdd As New List(Of Manuscript)()
            Dim duplicateCount As Integer = 0

            For Each importedManuscript As Manuscript In importResult.Manuscripts

                If existingTitles.Contains(
                importedManuscript.Title.Trim()
            ) Then

                    duplicateCount += 1

                Else

                    manuscriptsToAdd.Add(
                    importedManuscript
                )

                End If

            Next


            If manuscriptsToAdd.Count = 0 Then

                MessageBox.Show(
                Me,
                "No new manuscripts are available to import." &
                Environment.NewLine &
                Environment.NewLine &
                duplicateCount.ToString() &
                " manuscript(s) matched titles already in ManuscriptPipeline.",
                "Nothing to Import",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            )

                Return

            End If


            ' =================================================
            ' Build preview
            ' =================================================

            Dim preview As String =
            "Manuscripts to add: " &
            manuscriptsToAdd.Count.ToString() &
            Environment.NewLine &
            "Journal submissions: " &
            importResult.SubmissionCount.ToString() &
            Environment.NewLine &
            "Editorial decisions: " &
            importResult.DecisionCount.ToString()

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
                Math.Min(
                    5,
                    importResult.Warnings.Count
                )

                preview &=
                Environment.NewLine &
                Environment.NewLine &
                "First warnings:"

                For i As Integer = 0 To warningLimit - 1

                    preview &=
                    Environment.NewLine &
                    "• " &
                    importResult.Warnings(i)

                Next

                If importResult.Warnings.Count > warningLimit Then

                    preview &=
                    Environment.NewLine &
                    "• ...and " &
                    (
                        importResult.Warnings.Count -
                        warningLimit
                    ).ToString() &
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


            ' =================================================
            ' Backup existing JSON before modifying anything
            ' =================================================

            Try

                repository.CreatePreImportBackup()

            Catch ex As Exception

                MessageBox.Show(
                Me,
                "ManuscriptPipeline could not create the pre-import backup." &
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


            ' =================================================
            ' Add imported manuscripts
            ' =================================================

            For Each importedManuscript As Manuscript In manuscriptsToAdd

                manuscripts.Add(
                importedManuscript
            )

            Next


            If Not SaveManuscripts() Then

                For Each importedManuscript As Manuscript In manuscriptsToAdd

                    manuscripts.Remove(
                    importedManuscript
                )

                Next

                Return

            End If


            RenderManuscripts()

            MessageBox.Show(
            Me,
            manuscriptsToAdd.Count.ToString() &
            " manuscript(s) were imported successfully.",
            "Import Complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        )

        End Using

    End Sub

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