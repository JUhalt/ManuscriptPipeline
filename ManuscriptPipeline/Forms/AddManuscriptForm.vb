Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

Namespace Forms

    Public Class AddManuscriptForm
        Inherits Form

        Private ReadOnly txtTitle As New TextBox()
        Private ReadOnly txtCoAuthors As New TextBox()
        Private ReadOnly txtTargetJournal As New TextBox()
        Private ReadOnly cmbStage As New ComboBox()

        Private _createdManuscript As Manuscript

        Public ReadOnly Property CreatedManuscript As Manuscript
            Get
                Return _createdManuscript
            End Get
        End Property

        Public Sub New()
            BuildInterface()
            UiPolish.ApplyDialog(Me)
        End Sub

        Private Sub BuildInterface()

            Me.Text = "Add Manuscript"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ClientSize = New Size(560, 340)
            Me.Font = New Font("Segoe UI", 10.0F)

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 6,
                .Padding = New Padding(20)
            }

            layout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Absolute, 145)
            )

            layout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100)
            )

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 50))

            txtTitle.Dock = DockStyle.Fill
            txtCoAuthors.Dock = DockStyle.Fill
            txtTargetJournal.Dock = DockStyle.Fill

            cmbStage.Dock = DockStyle.Fill
            cmbStage.DropDownStyle = ComboBoxStyle.DropDownList

            For Each stage As PaperStage In
                System.Enum.GetValues(GetType(PaperStage))

                cmbStage.Items.Add(stage)

            Next

            cmbStage.SelectedItem = PaperStage.Idea

            layout.Controls.Add(
                CreateFieldLabel("Title"),
                0,
                0
            )

            layout.Controls.Add(txtTitle, 1, 0)

            layout.Controls.Add(
                CreateFieldLabel("Co-authors"),
                0,
                1
            )

            layout.Controls.Add(txtCoAuthors, 1, 1)

            layout.Controls.Add(
                CreateFieldLabel("Target journal"),
                0,
                2
            )

            layout.Controls.Add(txtTargetJournal, 1, 2)

            layout.Controls.Add(
                CreateFieldLabel("Current stage"),
                0,
                3
            )

            layout.Controls.Add(cmbStage, 1, 3)

            Dim btnAdd As New Button With {
                .Text = "Add Manuscript",
                .AutoSize = True,
                .Height = 34
            }

            Dim btnCancel As New Button With {
                .Text = "Cancel",
                .AutoSize = True,
                .Height = 34,
                .DialogResult = DialogResult.Cancel
            }

            AddHandler btnAdd.Click, AddressOf AddManuscript

            Dim buttonPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False
            }

            buttonPanel.Controls.Add(btnAdd)
            buttonPanel.Controls.Add(btnCancel)

            layout.Controls.Add(buttonPanel, 0, 5)
            layout.SetColumnSpan(buttonPanel, 2)

            Me.AcceptButton = btnAdd
            Me.CancelButton = btnCancel

            Me.Controls.Add(layout)

        End Sub

        Private Function CreateFieldLabel(text As String) As Label

            Return New Label With {
                .Text = text,
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Font = New Font(Me.Font, FontStyle.Bold)
            }

        End Function

        Private Sub AddManuscript(
            sender As Object,
            e As EventArgs
        )

            If String.IsNullOrWhiteSpace(txtTitle.Text) Then

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

            Dim selectedStage As PaperStage =
                CType(cmbStage.SelectedItem, PaperStage)

            _createdManuscript = New Manuscript With {
                .Title = txtTitle.Text.Trim(),
                .CoAuthors = txtCoAuthors.Text.Trim(),
                .TargetJournal = txtTargetJournal.Text.Trim(),
                .CurrentStage = selectedStage,
                .Location = ManuscriptLocation.Pipeline,
                .StageEnteredDate = DateTime.Now
            }

            _createdManuscript.History.Add(
                New HistoryEvent With {
                    .Stage = selectedStage,
                    .Note = "Manuscript added to ManuscriptPipeline."
                }
            )

            Me.DialogResult = DialogResult.OK

        End Sub

    End Class

End Namespace