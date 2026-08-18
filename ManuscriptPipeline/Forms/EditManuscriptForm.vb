Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models

Namespace Forms

    Public Class EditManuscriptForm
        Inherits Form

        Private ReadOnly _originalManuscript As Manuscript
        Private ReadOnly _workingManuscript As Manuscript

        Private _deleteRequested As Boolean = False

        Private ReadOnly txtTitle As New TextBox()
        Private ReadOnly txtCoAuthors As New TextBox()
        Private ReadOnly txtTargetJournal As New TextBox()
        Private ReadOnly cmbStage As New ComboBox()

        Private ReadOnly lstSubmissions As New ListBox()

        Private ReadOnly btnViewSubmission As New Button()
        Private ReadOnly btnEditSubmission As New Button()
        Private ReadOnly btnDeleteSubmission As New Button()

        Private ReadOnly lblSubmissionInfo As New Label()

        Private ReadOnly _displayedSubmissions As New List(Of JournalSubmission)()


        Public ReadOnly Property DeleteRequested As Boolean
            Get
                Return _deleteRequested
            End Get
        End Property


        Public Sub New(manuscript As Manuscript)

            _originalManuscript =
                manuscript

            _workingManuscript =
                CloneManuscript(manuscript)

            BuildInterface()
            LoadManuscript()

        End Sub


        ' =====================================================
        ' Interface
        ' =====================================================

        Private Sub BuildInterface()

            Me.Text = "Manuscript Details"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.Size = New Size(920, 720)
            Me.MinimumSize = New Size(820, 620)
            Me.Font = New Font("Segoe UI", 10.0F)
            Me.AutoScaleMode = AutoScaleMode.Dpi

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(20)
            }

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 245))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 62))

            ' =================================================
            ' Manuscript metadata
            ' =================================================

            Dim detailsGroup As New GroupBox With {
                .Text = "Manuscript",
                .Dock = DockStyle.Fill,
                .Padding = New Padding(14)
            }

            Dim details As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 4
            }

            details.ColumnStyles.Add(
                New ColumnStyle(SizeType.Absolute, 145)
            )

            details.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100)
            )

            For i As Integer = 0 To 3
                details.RowStyles.Add(
                    New RowStyle(SizeType.Percent, 25)
                )
            Next

            txtTitle.Dock = DockStyle.Fill
            txtCoAuthors.Dock = DockStyle.Fill
            txtTargetJournal.Dock = DockStyle.Fill

            cmbStage.Dock = DockStyle.Fill
            cmbStage.DropDownStyle = ComboBoxStyle.DropDownList

            For Each stage As PaperStage In
                System.Enum.GetValues(GetType(PaperStage))

                cmbStage.Items.Add(stage)

            Next

            details.Controls.Add(CreateFieldLabel("Title"), 0, 0)
            details.Controls.Add(txtTitle, 1, 0)

            details.Controls.Add(CreateFieldLabel("Co-authors"), 0, 1)
            details.Controls.Add(txtCoAuthors, 1, 1)

            details.Controls.Add(CreateFieldLabel("Target journal"), 0, 2)
            details.Controls.Add(txtTargetJournal, 1, 2)

            details.Controls.Add(CreateFieldLabel("Current stage"), 0, 3)
            details.Controls.Add(cmbStage, 1, 3)

            detailsGroup.Controls.Add(details)

            ' =================================================
            ' Submissions
            ' =================================================

            Dim submissionsGroup As New GroupBox With {
                .Text = "Journal Submissions",
                .Dock = DockStyle.Fill,
                .Padding = New Padding(14)
            }

            Dim submissionsLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2
            }

            submissionsLayout.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 48)
            )

            submissionsLayout.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100)
            )

            Dim submissionToolbar As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1
            }

            submissionToolbar.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100)
            )

            submissionToolbar.ColumnStyles.Add(
                New ColumnStyle(SizeType.AutoSize)
            )

            lblSubmissionInfo.AutoSize = True
            lblSubmissionInfo.Anchor = AnchorStyles.Left
            lblSubmissionInfo.ForeColor = SystemColors.GrayText

            Dim submissionButtons As New FlowLayoutPanel With {
                .AutoSize = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False
            }

            btnViewSubmission.Text = "View"
            btnViewSubmission.AutoSize = True
            btnViewSubmission.Height = 34
            btnViewSubmission.Visible = False

            btnEditSubmission.Text = "Edit Submission"
            btnEditSubmission.AutoSize = True
            btnEditSubmission.Height = 34
            btnEditSubmission.Visible = False

            btnDeleteSubmission.Text = "Delete Submission"
            btnDeleteSubmission.AutoSize = True
            btnDeleteSubmission.Height = 34
            btnDeleteSubmission.Visible = False

            Dim btnAddSubmission As New Button With {
                .Text = "+ Add Submission",
                .AutoSize = True,
                .Height = 34
            }

            AddHandler btnViewSubmission.Click,
                AddressOf ViewSelectedSubmission

            AddHandler btnEditSubmission.Click,
                AddressOf EditSelectedSubmission

            AddHandler btnDeleteSubmission.Click,
                AddressOf DeleteSelectedSubmission

            AddHandler btnAddSubmission.Click,
                AddressOf AddSubmission

            submissionButtons.Controls.Add(btnViewSubmission)
            submissionButtons.Controls.Add(btnEditSubmission)
            submissionButtons.Controls.Add(btnDeleteSubmission)
            submissionButtons.Controls.Add(btnAddSubmission)

            submissionToolbar.Controls.Add(lblSubmissionInfo, 0, 0)
            submissionToolbar.Controls.Add(submissionButtons, 1, 0)

            lstSubmissions.Dock = DockStyle.Fill
            lstSubmissions.IntegralHeight = False

            AddHandler lstSubmissions.SelectedIndexChanged,
                AddressOf SubmissionSelectionChanged

            AddHandler lstSubmissions.DoubleClick,
                AddressOf ViewSelectedSubmission

            submissionsLayout.Controls.Add(submissionToolbar, 0, 0)
            submissionsLayout.Controls.Add(lstSubmissions, 0, 1)

            submissionsGroup.Controls.Add(submissionsLayout)

            ' =================================================
            ' Footer
            ' =================================================

            Dim footer As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .Padding = New Padding(0, 8, 0, 0)
            }

            footer.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 50)
            )

            footer.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 50)
            )

            Dim btnDeleteManuscript As New Button With {
                .Text = "Delete Manuscript",
                .AutoSize = True,
                .Height = 36,
                .Anchor = AnchorStyles.Left
            }

            AddHandler btnDeleteManuscript.Click,
                AddressOf RequestDelete

            Dim rightButtons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False
            }

            Dim btnSave As New Button With {
                .Text = "Save & Close",
                .AutoSize = True,
                .Height = 36
            }

            Dim btnCancel As New Button With {
                .Text = "Cancel",
                .AutoSize = True,
                .Height = 36,
                .DialogResult = DialogResult.Cancel
            }

            AddHandler btnSave.Click,
                AddressOf SaveChanges

            rightButtons.Controls.Add(btnSave)
            rightButtons.Controls.Add(btnCancel)

            footer.Controls.Add(btnDeleteManuscript, 0, 0)
            footer.Controls.Add(rightButtons, 1, 0)

            root.Controls.Add(detailsGroup, 0, 0)
            root.Controls.Add(submissionsGroup, 0, 1)
            root.Controls.Add(footer, 0, 2)

            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel

            Me.Controls.Add(root)

        End Sub


        Private Function CreateFieldLabel(text As String) As Label

            Return New Label With {
                .Text = text,
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Font = New Font(Me.Font, FontStyle.Bold)
            }

        End Function


        ' =====================================================
        ' Load
        ' =====================================================

        Private Sub LoadManuscript()

            txtTitle.Text =
                _workingManuscript.Title

            txtCoAuthors.Text =
                _workingManuscript.CoAuthors

            txtTargetJournal.Text =
                _workingManuscript.TargetJournal

            cmbStage.SelectedItem =
                _workingManuscript.CurrentStage

            RefreshSubmissionList()

        End Sub


        ' =====================================================
        ' Submission list
        ' =====================================================

        Private Sub RefreshSubmissionList()

            lstSubmissions.Items.Clear()
            _displayedSubmissions.Clear()

            For Each submission As JournalSubmission In
                _workingManuscript.Submissions

                _displayedSubmissions.Add(submission)

                lstSubmissions.Items.Add(
                    FormatSubmission(submission)
                )

            Next

            If _displayedSubmissions.Count = 0 Then

                lblSubmissionInfo.Text =
                    "No journal submissions recorded. Add one to begin."

            Else

                lblSubmissionInfo.Text =
                    "Select a submission to view, edit, or delete it."

            End If

            UpdateSubmissionButtons()

        End Sub


        Private Function FormatSubmission(
            submission As JournalSubmission
        ) As String

            Dim result As String =
                submission.SubmittedDate.ToString("MMM d, yyyy") &
                " - " &
                submission.JournalName

            If Not String.IsNullOrWhiteSpace(
                submission.ManuscriptNumber
            ) Then

                result &=
                    " - " &
                    submission.ManuscriptNumber

            End If

            If Not String.IsNullOrWhiteSpace(
                submission.Notes
            ) Then

                result &=
                    " - Notes"

            End If

            If Not String.IsNullOrWhiteSpace(
                submission.PortalUrl
            ) Then

                result &=
                    " - Portal"

            End If

            If submission.Decisions.Count > 0 Then

                result &=
                    " - " &
                    submission.Decisions.Count.ToString() &
                    " decision(s)"

            End If

            If submission.Correspondence.Count > 0 Then

                result &=
                    " - " &
                    submission.Correspondence.Count.ToString() &
                    " file(s)"

            End If

            Return result

        End Function


        Private Function GetSelectedSubmission() As JournalSubmission

            Dim selectedIndex As Integer =
                lstSubmissions.SelectedIndex

            If selectedIndex < 0 OrElse
               selectedIndex >= _displayedSubmissions.Count Then

                Return Nothing

            End If

            Return _displayedSubmissions(selectedIndex)

        End Function


        Private Sub SubmissionSelectionChanged(
            sender As Object,
            e As EventArgs
        )

            UpdateSubmissionButtons()

        End Sub


        Private Sub UpdateSubmissionButtons()

            Dim hasSelection As Boolean =
                GetSelectedSubmission() IsNot Nothing

            btnViewSubmission.Visible =
                hasSelection

            btnEditSubmission.Visible =
                hasSelection

            btnDeleteSubmission.Visible =
                hasSelection

        End Sub


        ' =====================================================
        ' View submission
        ' =====================================================

        Private Sub ViewSelectedSubmission(
            sender As Object,
            e As EventArgs
        )

            Dim submission As JournalSubmission =
                GetSelectedSubmission()

            If submission Is Nothing Then
                Return
            End If

            Using dialog As New SubmissionDetailsForm(submission)

                dialog.ShowDialog(Me)

            End Using

            RefreshSubmissionList()

        End Sub


        ' =====================================================
        ' Add submission
        ' =====================================================

        Private Sub AddSubmission(
            sender As Object,
            e As EventArgs
        )

            Using dialog As New AddSubmissionForm()

                If dialog.ShowDialog(Me) =
                    DialogResult.OK AndAlso
                   dialog.CreatedSubmission IsNot Nothing Then

                    _workingManuscript.Submissions.Add(
                        dialog.CreatedSubmission
                    )

                    RefreshSubmissionList()

                    lstSubmissions.SelectedIndex =
                        lstSubmissions.Items.Count - 1

                End If

            End Using

        End Sub


        ' =====================================================
        ' Edit submission
        ' =====================================================

        Private Sub EditSelectedSubmission(
            sender As Object,
            e As EventArgs
        )

            Dim selected As JournalSubmission =
                GetSelectedSubmission()

            If selected Is Nothing Then
                Return
            End If

            Using dialog As New AddSubmissionForm(selected)

                If dialog.ShowDialog(Me) <>
                    DialogResult.OK OrElse
                   dialog.CreatedSubmission Is Nothing Then

                    Return

                End If

                Dim updated As JournalSubmission =
                    dialog.CreatedSubmission

                For i As Integer = 0 To _workingManuscript.Submissions.Count - 1

                    If _workingManuscript.Submissions(i).Id =
                        selected.Id Then

                        _workingManuscript.Submissions(i) =
                            updated

                        Exit For

                    End If

                Next

                RefreshSubmissionList()

                For i As Integer = 0 To _displayedSubmissions.Count - 1

                    If _displayedSubmissions(i).Id =
                        updated.Id Then

                        lstSubmissions.SelectedIndex =
                            i

                        Exit For

                    End If

                Next

            End Using

        End Sub


        ' =====================================================
        ' Delete submission
        ' =====================================================

        Private Sub DeleteSelectedSubmission(
            sender As Object,
            e As EventArgs
        )

            Dim selected As JournalSubmission =
                GetSelectedSubmission()

            If selected Is Nothing Then
                Return
            End If

            Dim warning As String =
                "Delete the submission to '" &
                selected.JournalName &
                "'?" &
                Environment.NewLine &
                Environment.NewLine &
                "This will also remove:" &
                Environment.NewLine &
                "- " &
                selected.Decisions.Count.ToString() &
                " editorial decision(s)" &
                Environment.NewLine &
                "- " &
                selected.Correspondence.Count.ToString() &
                " correspondence/file record(s)"

            Dim result As DialogResult =
                MessageBox.Show(
                    Me,
                    warning,
                    "Delete Journal Submission",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                )

            If result <>
                DialogResult.Yes Then

                Return

            End If

            _workingManuscript.Submissions.Remove(
                selected
            )

            RefreshSubmissionList()

        End Sub


        ' =====================================================
        ' Delete manuscript
        ' =====================================================

        Private Sub RequestDelete(
            sender As Object,
            e As EventArgs
        )

            Dim result As DialogResult =
                MessageBox.Show(
                    Me,
                    "Delete '" &
                    _workingManuscript.Title &
                    "' from ManuscriptPipeline?" &
                    Environment.NewLine &
                    Environment.NewLine &
                    "The complete manuscript record will be removed.",
                    "Delete Manuscript",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                )

            If result <>
                DialogResult.Yes Then

                Return

            End If

            _deleteRequested =
                True

            Me.DialogResult =
                DialogResult.Abort

        End Sub


        ' =====================================================
        ' Save working copy
        ' =====================================================

        Private Sub SaveChanges(
            sender As Object,
            e As EventArgs
        )

            If String.IsNullOrWhiteSpace(
                txtTitle.Text
            ) Then

                MessageBox.Show(
                    Me,
                    "Please enter a manuscript title.",
                    "Title Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

                txtTitle.Focus()

                Return

            End If

            If cmbStage.SelectedItem Is Nothing Then
                Return
            End If

            Dim oldStage As PaperStage =
                _workingManuscript.CurrentStage

            Dim newStage As PaperStage =
                CType(
                    cmbStage.SelectedItem,
                    PaperStage
                )

            If newStage = PaperStage.Published Then

                _workingManuscript.Location =
        ManuscriptLocation.Published

            ElseIf _workingManuscript.Location =
    ManuscriptLocation.Published Then

                _workingManuscript.Location =
        ManuscriptLocation.Pipeline

            End If

            _workingManuscript.Title =
                txtTitle.Text.Trim()

            _workingManuscript.CoAuthors =
                txtCoAuthors.Text.Trim()

            _workingManuscript.TargetJournal =
                txtTargetJournal.Text.Trim()

            If oldStage <> newStage Then

                _workingManuscript.CurrentStage =
                    newStage

                _workingManuscript.StageEnteredDate =
                    DateTime.Now

                _workingManuscript.History.Add(
                    New HistoryEvent With {
                        .Stage = newStage,
                        .Note =
                            "Stage changed from " &
                            oldStage.ToString() &
                            " to " &
                            newStage.ToString() &
                            "."
                    }
                )

            Else

                _workingManuscript.CurrentStage =
                    newStage

            End If

            CopyWorkingToOriginal()

            Me.DialogResult =
                DialogResult.OK

        End Sub


        ' =====================================================
        ' Clone manuscript
        ' =====================================================

        Private Function CloneManuscript(
            source As Manuscript
        ) As Manuscript

            Dim clone As New Manuscript With {
                .Id = source.Id,
                .Title = source.Title,
                .CoAuthors = source.CoAuthors,
                .TargetJournal = source.TargetJournal,
                .CurrentStage = source.CurrentStage,
                .Location = source.Location,
                .StageEnteredDate = source.StageEnteredDate,
                .RevisionDeadline = source.RevisionDeadline,
                .FileDrawerDate = source.FileDrawerDate,
                .FileDrawerReason = source.FileDrawerReason
            }

            For Each historyEvent As HistoryEvent In
                source.History

                clone.History.Add(
                    New HistoryEvent With {
                        .Id = historyEvent.Id,
                        .EventDate = historyEvent.EventDate,
                        .Stage = historyEvent.Stage,
                        .Note = historyEvent.Note
                    }
                )

            Next

            For Each submission As JournalSubmission In
                source.Submissions

                clone.Submissions.Add(
                    CloneSubmission(submission)
                )

            Next

            Return clone

        End Function


        Private Function CloneSubmission(
            source As JournalSubmission
        ) As JournalSubmission

            Dim clone As New JournalSubmission With {
                .Id = source.Id,
                .JournalName = source.JournalName,
                .ManuscriptNumber = source.ManuscriptNumber,
                .SubmittedDate = source.SubmittedDate,
                .Notes = source.Notes,
                .PortalUrl = source.PortalUrl
            }

            For Each decisionEvent As EditorialDecisionEvent In
                source.Decisions

                clone.Decisions.Add(
                    New EditorialDecisionEvent With {
                        .Id = decisionEvent.Id,
                        .DecisionDate = decisionEvent.DecisionDate,
                        .Decision = decisionEvent.Decision,
                        .RevisionDeadline = decisionEvent.RevisionDeadline,
                        .Notes = decisionEvent.Notes
                    }
                )

            Next

            For Each item As CorrespondenceItem In
                source.Correspondence

                clone.Correspondence.Add(
                    New CorrespondenceItem With {
                        .Id = item.Id,
                        .ItemDate = item.ItemDate,
                        .Type = item.Type,
                        .Title = item.Title,
                        .Notes = item.Notes,
                        .LocalFilePath = item.LocalFilePath,
                        .SourceUrl = item.SourceUrl,
                        .IsManagedCopy = item.IsManagedCopy
                    }
                )

            Next

            Return clone

        End Function


        ' =====================================================
        ' Commit working copy
        ' =====================================================

        Private Sub CopyWorkingToOriginal()

            Dim committed As Manuscript =
                CloneManuscript(
                    _workingManuscript
                )

            _originalManuscript.Id =
                committed.Id

            _originalManuscript.Title =
                committed.Title

            _originalManuscript.CoAuthors =
                committed.CoAuthors

            _originalManuscript.TargetJournal =
                committed.TargetJournal

            _originalManuscript.CurrentStage =
                committed.CurrentStage

            _originalManuscript.Location =
                committed.Location

            _originalManuscript.StageEnteredDate =
                committed.StageEnteredDate

            _originalManuscript.RevisionDeadline =
                committed.RevisionDeadline

            _originalManuscript.FileDrawerDate =
                committed.FileDrawerDate

            _originalManuscript.FileDrawerReason =
                committed.FileDrawerReason

            _originalManuscript.History =
                committed.History

            _originalManuscript.Submissions =
                committed.Submissions

        End Sub

    End Class

End Namespace