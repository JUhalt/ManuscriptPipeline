Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Services

Namespace Forms

    Public Class AboutForm
        Inherits Form

        Public Sub New()

            BuildInterface()

            UiPolish.ApplyDialog(Me)

        End Sub


        Private Sub BuildInterface()

            Me.Text = "About ManuscriptPipeline"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.ClientSize = New Size(520, 360)
            Me.Font = New Font("Segoe UI", 10.0F)
            Me.AutoScaleMode = AutoScaleMode.Dpi


            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 6,
                .Padding = New Padding(24)
            }


            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 54)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 34)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 50)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 36)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 48)
            )


            Dim lblTitle As New Label With {
                .Text = "ManuscriptPipeline",
                .AutoSize = True,
                .Font = New Font(
                    Me.Font.FontFamily,
                    18.0F,
                    FontStyle.Bold
                ),
                .Anchor = AnchorStyles.Left
            }


            Dim lblVersion As New Label With {
                .Text =
                    "Version " &
                    Application.ProductVersion,
                .AutoSize = True,
                .ForeColor = SystemColors.GrayText,
                .Anchor = AnchorStyles.Left
            }


            Dim lblTagline As New Label With {
                .Text =
                    "Local-first academic manuscript tracking",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left
            }


            Dim lblDescription As New Label With {
                .Text =
                    "Track manuscripts, journal submissions, editorial decisions, correspondence, publication progress, and File Drawer outcomes — while keeping your data on your own computer." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Open-source software.",
                .Dock = DockStyle.Fill,
                .AutoSize = False
            }


            Dim lblCopyright As New Label With {
                .Text =
                    "© " &
                    DateTime.Now.Year.ToString() &
                    " Joshua Uhalt",
                .AutoSize = True,
                .ForeColor = SystemColors.GrayText,
                .Anchor = AnchorStyles.Left
            }


            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False
            }


            Dim btnClose As New Button With {
                .Text = "Close",
                .Width = 95,
                .Height = 36,
                .DialogResult = DialogResult.OK
            }


            buttons.Controls.Add(
                btnClose
            )


            root.Controls.Add(lblTitle, 0, 0)
            root.Controls.Add(lblVersion, 0, 1)
            root.Controls.Add(lblTagline, 0, 2)
            root.Controls.Add(lblDescription, 0, 3)
            root.Controls.Add(lblCopyright, 0, 4)
            root.Controls.Add(buttons, 0, 5)


            Me.AcceptButton =
                btnClose

            Me.CancelButton =
                btnClose


            Me.Controls.Add(
                root
            )

        End Sub

    End Class

End Namespace