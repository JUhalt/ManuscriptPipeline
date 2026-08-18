Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Forms

Public Class Form1

    Private Const FileDrawerSuggestionThreshold As Integer = 3

    Private ReadOnly manuscripts As New List(Of Manuscript)()

    Private ReadOnly pipelinePanel As New FlowLayoutPanel()
    Private ReadOnly fileDrawerPanel As New FlowLayoutPanel()
    Private ReadOnly lblStatus As New Label()

    Private uiInitialized As Boolean = False

    Private Sub Form1_Load(
        sender As Object,
        e As EventArgs
    ) Handles MyBase.Load

        If uiInitialized Then Return

        uiInitialized = True

        BuildInterface()
        RenderManuscripts()

    End Sub

    Private Sub BuildInterface()

        Me.Controls.Clear()

        Me.Text = "ManuscriptPipeline"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(1100, 760)
        Me.MinimumSize = New Size(850, 600)
        Me.Font = New Font("Segoe UI", 10.0F)

        ' ---------------------------------
        ' Root layout
        ' ---------------------------------

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 3,
            .BackColor = Color.FromArgb(245, 245, 245)
        }

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 78)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100)
        )

        root.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 32)
        )

        ' ---------------------------------
        ' Header
        ' ---------------------------------

        Dim header As New Panel With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(18, 12, 18, 10),
            .BackColor = Color.White
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
            .ForeColor = Color.DimGray
        }

        Dim btnAdd As New Button With {
            .Text = "+ Add Manuscript",
            .Dock = DockStyle.Right,
            .Width = 160
        }

        AddHandler btnAdd.Click, AddressOf AddManuscript

        header.Controls.Add(lblTitle)
        header.Controls.Add(lblSubtitle)
        header.Controls.Add(btnAdd)

        ' ---------------------------------
        ' Main body
        ' ---------------------------------

        Dim body As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 4,
            .Padding = New Padding(18, 12, 18, 12)
        }

        body.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 38)
        )

        body.RowStyles.Add(
            New RowStyle(SizeType.Percent, 55)
        )

        body.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 38)
        )

        body.RowStyles.Add(
            New RowStyle(SizeType.Percent, 45)
        )

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

        ' ---------------------------------
        ' Footer
        ' ---------------------------------

        lblStatus.Dock = DockStyle.Fill
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        lblStatus.Padding = New Padding(18, 0, 0, 0)
        lblStatus.ForeColor = Color.DimGray
        lblStatus.Text =
            "Development build — data is not yet saved after closing."

        ' ---------------------------------
        ' Add everything
        ' ---------------------------------

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

    Private Sub ConfigureFlowPanel(
        panel As FlowLayoutPanel
    )

        panel.Dock = DockStyle.Fill
        panel.FlowDirection = FlowDirection.TopDown
        panel.WrapContents = False
        panel.AutoScroll = True
        panel.Padding = New Padding(8)
        panel.BackColor = Color.White
        panel.BorderStyle = BorderStyle.FixedSingle

    End Sub

    ' =====================================================
    ' Manuscript display
    ' =====================================================

    Private Sub RenderManuscripts()

        pipelinePanel.SuspendLayout()
        fileDrawerPanel.SuspendLayout()

        pipelinePanel.Controls.Clear()
        fileDrawerPanel.Controls.Clear()

        Dim pipelineCount As Integer = 0
        Dim drawerCount As Integer = 0

        For Each manuscript In manuscripts

            If manuscript.Location =
                ManuscriptLocation.Pipeline Then

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
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle,
            .Margin = New Padding(4, 4, 4, 8)
        }

        Dim lblTitle As New Label With {
            .Text = manuscript.Title,
            .AutoEllipsis = True,
            .Left = 16,
            .Top = 12,
            .Width = card.Width - 200,
            .Height = 25,
            .Font = New Font(Me.Font.FontFamily, 11, FontStyle.Bold),
            .Anchor = AnchorStyles.Top Or
                      AnchorStyles.Left Or
                      AnchorStyles.Right
        }

        Dim journalText As String

        If String.IsNullOrWhiteSpace(
            manuscript.TargetJournal
        ) Then

            journalText = "Target journal: Not set"

        Else

            journalText =
                $"Target journal: {manuscript.TargetJournal}"

        End If

        Dim lblStage As New Label With {
            .Text =
                $"{FormatStage(manuscript.CurrentStage)}  •  {journalText}",
            .AutoEllipsis = True,
            .Left = 16,
            .Top = 42,
            .Width = card.Width - 210,
            .Height = 23,
            .ForeColor = Color.DimGray,
            .Anchor = AnchorStyles.Top Or
                      AnchorStyles.Left Or
                      AnchorStyles.Right
        }

        Dim lblStats As New Label With {
            .Text =
                $"{manuscript.SubmissionCount} submissions  •  " &
                $"{manuscript.RejectionCount} rejections",
            .AutoSize = True,
            .Left = 16,
            .Top = 73
        }

        Dim btnLocation As New Button With {
            .Width = 155,
            .Height = 34,
            .Left = card.Width - 172,
            .Top = 41,
            .Anchor = AnchorStyles.Top Or
                      AnchorStyles.Right
        }

        If manuscript.Location =
            ManuscriptLocation.Pipeline Then

            btnLocation.Text = "Move to File Drawer"

            AddHandler btnLocation.Click,
                Sub(sender, e)
                    MoveToFileDrawer(manuscript)
                End Sub

        Else

            btnLocation.Text = "Restore to Pipeline"

            AddHandler btnLocation.Click,
                Sub(sender, e)
                    RestoreToPipeline(manuscript)
                End Sub

        End If

        card.Controls.Add(lblTitle)
        card.Controls.Add(lblStage)
        card.Controls.Add(lblStats)
        card.Controls.Add(btnLocation)

        If manuscript.Location =
                ManuscriptLocation.Pipeline AndAlso
           manuscript.RejectionCount >=
                FileDrawerSuggestionThreshold Then

            card.Height = 142

            Dim lblSuggestion As New Label With {
                .Text =
                    $"File Drawer suggestion: " &
                    $"{manuscript.RejectionCount} rejections",
                .AutoSize = True,
                .Left = 16,
                .Top = 99,
                .ForeColor = Color.Firebrick,
                .Font = New Font(Me.Font, FontStyle.Bold)
            }

            card.Controls.Add(lblSuggestion)

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
            .ForeColor = Color.Gray
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
                control.Width = newWidth
            End If

        Next

    End Sub

    ' =====================================================
    ' Add manuscript
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

                RenderManuscripts()

            End If

        End Using

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
                $"Move '{manuscript.Title}' to the File Drawer?" &
                Environment.NewLine &
                Environment.NewLine &
                "Its history and submission records will be preserved.",
                "Move to File Drawer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

        If result <> DialogResult.Yes Then Return

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

        If Not String.IsNullOrWhiteSpace(reason) Then
            historyNote &=
                $" Reason: {reason.Trim()}"
        End If

        manuscript.History.Add(
            New HistoryEvent With {
                .Stage = manuscript.CurrentStage,
                .Note = historyNote
            }
        )

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
                .Stage = manuscript.CurrentStage,
                .Note =
                    "Restored from File Drawer to active Pipeline."
            }
        )

        RenderManuscripts()

    End Sub

    ' =====================================================
    ' Display helpers
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