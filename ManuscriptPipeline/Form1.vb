Imports ManuscriptPipeline.Models

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim manuscript As New Manuscript With {
            .Title = "A Test Manuscript",
            .TargetJournal = "Journal of Example Studies",
            .CurrentStage = PaperStage.Draft,
            .Location = ManuscriptLocation.Pipeline
        }

        manuscript.History.Add(
            New HistoryEvent With {
                .Stage = manuscript.CurrentStage,
                .Note = "Created as a development test manuscript."
            }
        )

        Me.Text = "ManuscriptPipeline"

        Dim titleLabel As New System.Windows.Forms.Label With {
            .AutoSize = True,
            .Left = 30,
            .Top = 30,
            .Text = $"{manuscript.Title} — {manuscript.CurrentStage}"
        }

        Me.Controls.Add(titleLabel)

    End Sub

End Class