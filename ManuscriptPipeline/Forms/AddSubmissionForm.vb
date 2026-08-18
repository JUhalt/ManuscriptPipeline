Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models

Namespace Forms

    Public Class AddSubmissionForm
        Inherits Form

        Private ReadOnly _existingSubmission As JournalSubmission

        Private ReadOnly txtJournal As New TextBox()
        Private ReadOnly txtManuscriptNumber As New TextBox()
        Private ReadOnly dtpSubmitted As New DateTimePicker()
        Private ReadOnly txtPortalUrl As New TextBox()
        Private ReadOnly txtNotes As New TextBox()

        Private _createdSubmission As JournalSubmission


        Public ReadOnly Property CreatedSubmission As JournalSubmission
            Get
                Return _createdSubmission
            End Get
        End Property


        Public Sub New()

            _existingSubmission = Nothing

            BuildInterface()

        End Sub


        Public Sub New(
            existingSubmission As JournalSubmission
        )

            _existingSubmission =
                existingSubmission

            BuildInterface()

            LoadExistingSubmission()

        End Sub


        Private Sub BuildInterface()

            If _existingSubmission Is Nothing Then

                Me.Text =
                    "Record Journal Submission"

            Else

                Me.Text =
                    "Edit Journal Submission"

            End If

            Me.StartPosition =
                FormStartPosition.CenterParent

            Me.FormBorderStyle =
                FormBorderStyle.FixedDialog

            Me.MaximizeBox = False
            Me.MinimizeBox = False

            Me.ClientSize =
                New Size(650, 520)

            Me.Font =
                New Font("Segoe UI", 10.0F)

            Me.AutoScaleMode =
                AutoScaleMode.Dpi

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 7,
                .Padding = New Padding(22)
            }

            root.ColumnStyles.Add(
                New ColumnStyle(
                    SizeType.Absolute,
                    170
                )
            )

            root.ColumnStyles.Add(
                New ColumnStyle(
                    SizeType.Percent,
                    100
                )
            )

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))

            txtJournal.Dock =
                DockStyle.Fill

            txtManuscriptNumber.Dock =
                DockStyle.Fill

            txtPortalUrl.Dock =
                DockStyle.Fill

            dtpSubmitted.Format =
                DateTimePickerFormat.Short

            dtpSubmitted.Value =
                DateTime.Today

            dtpSubmitted.Width =
                180

            txtNotes.Dock =
                DockStyle.Fill

            txtNotes.Multiline =
                True

            txtNotes.ScrollBars =
                ScrollBars.Vertical

            root.Controls.Add(
                CreateFieldLabel("Journal"),
                0,
                0
            )

            root.Controls.Add(
                txtJournal,
                1,
                0
            )

            root.Controls.Add(
                CreateFieldLabel("Manuscript number"),
                0,
                1
            )

            root.Controls.Add(
                txtManuscriptNumber,
                1,
                1
            )

            root.Controls.Add(
                CreateFieldLabel("Submitted"),
                0,
                2
            )

            root.Controls.Add(
                dtpSubmitted,
                1,
                2
            )

            root.Controls.Add(
                CreateFieldLabel("Publisher portal URL"),
                0,
                3
            )

            root.Controls.Add(
                txtPortalUrl,
                1,
                3
            )

            Dim lblNotes As New Label With {
                .Text = "Submission Notes",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Font =
                    New Font(
                        Me.Font,
                        FontStyle.Bold
                    )
            }

            root.Controls.Add(
                lblNotes,
                0,
                4
            )

            root.SetColumnSpan(
                lblNotes,
                2
            )

            root.Controls.Add(
                txtNotes,
                0,
                5
            )

            root.SetColumnSpan(
                txtNotes,
                2
            )

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection =
                    FlowDirection.RightToLeft,
                .WrapContents = False
            }

            Dim btnSave As New Button With {
                .AutoSize = True,
                .Height = 36
            }

            If _existingSubmission Is Nothing Then

                btnSave.Text =
                    "Add Submission"

            Else

                btnSave.Text =
                    "Save Changes"

            End If

            Dim btnCancel As New Button With {
                .Text = "Cancel",
                .AutoSize = True,
                .Height = 36,
                .DialogResult =
                    DialogResult.Cancel
            }

            AddHandler btnSave.Click,
                AddressOf SaveSubmission

            buttons.Controls.Add(
                btnSave
            )

            buttons.Controls.Add(
                btnCancel
            )

            root.Controls.Add(
                buttons,
                0,
                6
            )

            root.SetColumnSpan(
                buttons,
                2
            )

            Me.AcceptButton =
                btnSave

            Me.CancelButton =
                btnCancel

            Me.Controls.Add(
                root
            )

        End Sub


        Private Sub LoadExistingSubmission()

            txtJournal.Text =
                _existingSubmission.JournalName

            txtManuscriptNumber.Text =
                _existingSubmission.ManuscriptNumber

            dtpSubmitted.Value =
                _existingSubmission.SubmittedDate

            txtPortalUrl.Text =
                _existingSubmission.PortalUrl

            txtNotes.Text =
                _existingSubmission.Notes

        End Sub


        Private Function CreateFieldLabel(
            text As String
        ) As Label

            Return New Label With {
                .Text = text,
                .AutoSize = True,
                .Anchor =
                    AnchorStyles.Left,
                .Font =
                    New Font(
                        Me.Font,
                        FontStyle.Bold
                    )
            }

        End Function


        Private Sub SaveSubmission(
            sender As Object,
            e As EventArgs
        )

            If String.IsNullOrWhiteSpace(
                txtJournal.Text
            ) Then

                MessageBox.Show(
                    Me,
                    "Please enter the journal name.",
                    "Journal Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

                txtJournal.Focus()

                Return

            End If

            Dim portalText As String =
                txtPortalUrl.Text.Trim()

            If Not String.IsNullOrWhiteSpace(
                portalText
            ) Then

                Dim portalUri As Uri =
                    Nothing

                Dim validUri As Boolean =
                    Uri.TryCreate(
                        portalText,
                        UriKind.Absolute,
                        portalUri
                    )

                If Not validUri OrElse
                   portalUri Is Nothing OrElse
                   (
                       portalUri.Scheme <>
                           Uri.UriSchemeHttp AndAlso
                       portalUri.Scheme <>
                           Uri.UriSchemeHttps
                   ) Then

                    MessageBox.Show(
                        Me,
                        "The publisher portal must be a valid http:// or https:// address." &
                        Environment.NewLine &
                        Environment.NewLine &
                        "You may also leave this field blank.",
                        "Invalid Publisher URL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    )

                    txtPortalUrl.Focus()

                    Return

                End If

            End If

            Dim submissionId As Guid

            Dim decisions As List(Of EditorialDecisionEvent)

            Dim correspondence As List(Of CorrespondenceItem)

            If _existingSubmission Is Nothing Then

                submissionId =
                    Guid.NewGuid()

                decisions =
                    New List(Of EditorialDecisionEvent)()

                correspondence =
                    New List(Of CorrespondenceItem)()

            Else

                submissionId =
                    _existingSubmission.Id

                decisions =
                    New List(Of EditorialDecisionEvent)(
                        _existingSubmission.Decisions
                    )

                correspondence =
                    New List(Of CorrespondenceItem)(
                        _existingSubmission.Correspondence
                    )

            End If

            _createdSubmission =
                New JournalSubmission With {
                    .Id =
                        submissionId,
                    .JournalName =
                        txtJournal.Text.Trim(),
                    .ManuscriptNumber =
                        txtManuscriptNumber.Text.Trim(),
                    .SubmittedDate =
                        dtpSubmitted.Value.Date,
                    .PortalUrl =
                        portalText,
                    .Notes =
                        txtNotes.Text.Trim(),
                    .Decisions =
                        decisions,
                    .Correspondence =
                        correspondence
                }

            Me.DialogResult =
                DialogResult.OK

        End Sub

    End Class

End Namespace