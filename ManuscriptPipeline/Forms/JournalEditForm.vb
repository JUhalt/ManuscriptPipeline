Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

Namespace Forms

    Public Class JournalEditForm
        Inherits Form

        Private ReadOnly _source As JournalRecord

        Private ReadOnly txtName As New TextBox()
        Private ReadOnly txtPublisher As New TextBox()
        Private ReadOnly txtHomepage As New TextBox()
        Private ReadOnly txtPortal As New TextBox()
        Private ReadOnly txtNotes As New TextBox()
        Private ReadOnly chkFavorite As New CheckBox()
        Private ReadOnly chkShortlist As New CheckBox()

        Private _result As JournalRecord


        Public ReadOnly Property Result As JournalRecord
            Get
                Return _result
            End Get
        End Property


        Public Sub New(
            source As JournalRecord
        )

            _source = source

            BuildInterface()
            UiPolish.ApplyDialog(Me)
            LoadSource()

        End Sub


        Private Sub BuildInterface()

            Me.Text =
                If(
                    _source Is Nothing,
                    "Add Journal",
                    "Edit Journal"
                )

            Me.StartPosition =
                FormStartPosition.CenterParent

            Me.Size =
                New Size(
                    720,
                    620
                )

            Me.MinimumSize =
                New Size(
                    620,
                    540
                )

            Me.Font =
                New Font(
                    "Segoe UI",
                    10.0F
                )

            Me.AutoScaleMode =
                AutoScaleMode.Dpi

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 7,
                .Padding = New Padding(20)
            }

            root.ColumnStyles.Add(
                New ColumnStyle(
                    SizeType.Absolute,
                    180
                )
            )

            root.ColumnStyles.Add(
                New ColumnStyle(
                    SizeType.Percent,
                    100
                )
            )

            For index As Integer = 0 To 4

                root.RowStyles.Add(
                    New RowStyle(
                        SizeType.AutoSize
                    )
                )

            Next

            root.RowStyles.Add(
                New RowStyle(
                    SizeType.Percent,
                    100
                )
            )

            root.RowStyles.Add(
                New RowStyle(
                    SizeType.AutoSize
                )
            )

            txtName.Dock = DockStyle.Fill
            txtPublisher.Dock = DockStyle.Fill
            txtHomepage.Dock = DockStyle.Fill
            txtPortal.Dock = DockStyle.Fill

            txtNotes.Dock = DockStyle.Fill
            txtNotes.Multiline = True
            txtNotes.ScrollBars = ScrollBars.Vertical
            txtNotes.MinimumSize = New Size(0, 140)

            Dim flags As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = True
            }

            chkFavorite.Text =
                "Favorite"

            chkFavorite.AutoSize =
                True

            chkShortlist.Text =
                "Shortlist"

            chkShortlist.AutoSize =
                True

            flags.Controls.Add(
                chkFavorite
            )

            flags.Controls.Add(
                chkShortlist
            )

            root.Controls.Add(CreateLabel("Journal name"), 0, 0)
            root.Controls.Add(txtName, 1, 0)

            root.Controls.Add(CreateLabel("Publisher"), 0, 1)
            root.Controls.Add(txtPublisher, 1, 1)

            root.Controls.Add(CreateLabel("Homepage URL"), 0, 2)
            root.Controls.Add(txtHomepage, 1, 2)

            root.Controls.Add(CreateLabel("Submission portal"), 0, 3)
            root.Controls.Add(txtPortal, 1, 3)

            root.Controls.Add(CreateLabel("List status"), 0, 4)
            root.Controls.Add(flags, 1, 4)

            root.Controls.Add(CreateLabel("Notes"), 0, 5)
            root.Controls.Add(txtNotes, 1, 5)

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False
            }

            Dim btnSave As New Button With {
                .Text = "Save",
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
                AddressOf SaveJournal

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


        Private Function CreateLabel(
            text As String
        ) As Label

            Return New Label With {
                .Text = text,
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Font = New Font(
                    Me.Font,
                    FontStyle.Bold
                ),
                .Margin = New Padding(
                    3,
                    9,
                    3,
                    9
                )
            }

        End Function


        Private Sub LoadSource()

            If _source Is Nothing Then
                Return
            End If

            txtName.Text =
                _source.Name

            txtPublisher.Text =
                _source.Publisher

            txtHomepage.Text =
                _source.HomepageUrl

            txtPortal.Text =
                _source.SubmissionPortalUrl

            txtNotes.Text =
                _source.Notes

            chkFavorite.Checked =
                _source.IsFavorite

            chkShortlist.Checked =
                _source.IsShortlisted

        End Sub


        Private Sub SaveJournal(
            sender As Object,
            e As EventArgs
        )

            If String.IsNullOrWhiteSpace(
                txtName.Text
            ) Then

                MessageBox.Show(
                    Me,
                    "Enter a journal name.",
                    "Journal Name Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

                txtName.Focus()
                Return

            End If

            Dim homepage As String
            Dim portal As String

            Try

                homepage =
                    UrlSafetyService.NormalizeOptionalHttpUrl(
                        txtHomepage.Text,
                        "Journal homepage"
                    )

                portal =
                    UrlSafetyService.NormalizeOptionalHttpUrl(
                        txtPortal.Text,
                        "Submission portal"
                    )

            Catch ex As ArgumentException

                MessageBox.Show(
                    Me,
                    ex.Message,
                    "Check Journal URL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )

                Return

            End Try

            _result =
                New JournalRecord With {
                    .Id =
                        If(
                            _source Is Nothing,
                            Guid.NewGuid(),
                            _source.Id
                        ),
                    .Name = txtName.Text.Trim(),
                    .Publisher = txtPublisher.Text.Trim(),
                    .HomepageUrl = homepage,
                    .SubmissionPortalUrl = portal,
                    .Notes = txtNotes.Text.Trim(),
                    .IsFavorite = chkFavorite.Checked,
                    .IsShortlisted = chkShortlist.Checked
                }

            Me.DialogResult =
                DialogResult.OK

        End Sub

    End Class

End Namespace
