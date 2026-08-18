Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

Namespace Forms

    Public Class SettingsForm
        Inherits Form

        Private ReadOnly _settings As AppSettings
        Private ReadOnly _settingsService As New AppSettingsService()

        Private ReadOnly rbSystem As New RadioButton()
        Private ReadOnly rbLight As New RadioButton()
        Private ReadOnly rbDark As New RadioButton()

        Private ReadOnly numFileDrawerThreshold As New NumericUpDown()

        Private _appearanceChanged As Boolean = False


        Public ReadOnly Property AppearanceChanged As Boolean
            Get
                Return _appearanceChanged
            End Get
        End Property


        Public Sub New(
            settings As AppSettings
        )

            _settings = settings

            BuildInterface()
            LoadValues()

            UiPolish.ApplyDialog(Me)

        End Sub


        Private Sub BuildInterface()

            Me.Text = "Settings"
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
                .RowCount = 3,
                .Padding = New Padding(20)
            }

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 175)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100)
            )

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 55)
            )

            ' =================================================
            ' Appearance
            ' =================================================

            Dim appearanceGroup As New GroupBox With {
                .Text = "Appearance",
                .Dock = DockStyle.Fill,
                .Padding = New Padding(16)
            }

            Dim appearancePanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False
            }

            rbSystem.Text = "Follow Windows"
            rbSystem.AutoSize = True

            rbLight.Text = "Light"
            rbLight.AutoSize = True

            rbDark.Text = "Dark"
            rbDark.AutoSize = True

            Dim appearanceHelp As New Label With {
                .Text = "Appearance changes take effect after ManuscriptPipeline restarts.",
                .AutoSize = True,
                .ForeColor = SystemColors.GrayText,
                .Margin = New Padding(22, 12, 0, 0)
            }

            appearancePanel.Controls.Add(rbSystem)
            appearancePanel.Controls.Add(rbLight)
            appearancePanel.Controls.Add(rbDark)
            appearancePanel.Controls.Add(appearanceHelp)

            appearanceGroup.Controls.Add(
                appearancePanel
            )

            ' =================================================
            ' File Drawer
            ' =================================================

            Dim drawerGroup As New GroupBox With {
                .Text = "File Drawer",
                .Dock = DockStyle.Fill,
                .Padding = New Padding(16)
            }

            Dim drawerPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False
            }

            Dim lblThreshold As New Label With {
                .Text = "Suggest the File Drawer after",
                .AutoSize = True,
                .Margin = New Padding(0, 7, 8, 0)
            }

            numFileDrawerThreshold.Minimum = 1
            numFileDrawerThreshold.Maximum = 20
            numFileDrawerThreshold.Width = 60

            Dim lblRejections As New Label With {
                .Text = "rejections.",
                .AutoSize = True,
                .Margin = New Padding(8, 7, 0, 0)
            }

            drawerPanel.Controls.Add(lblThreshold)
            drawerPanel.Controls.Add(numFileDrawerThreshold)
            drawerPanel.Controls.Add(lblRejections)

            drawerGroup.Controls.Add(
                drawerPanel
            )

            ' =================================================
            ' Buttons
            ' =================================================

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
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
                AddressOf SaveSettings

            buttons.Controls.Add(btnSave)
            buttons.Controls.Add(btnCancel)

            root.Controls.Add(appearanceGroup, 0, 0)
            root.Controls.Add(drawerGroup, 0, 1)
            root.Controls.Add(buttons, 0, 2)

            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel

            Me.Controls.Add(root)

        End Sub


        Private Sub LoadValues()

            Select Case _settings.Appearance

                Case AppAppearance.Light

                    rbLight.Checked = True

                Case AppAppearance.Dark

                    rbDark.Checked = True

                Case Else

                    rbSystem.Checked = True

            End Select

            numFileDrawerThreshold.Value =
                Math.Max(
                    numFileDrawerThreshold.Minimum,
                    Math.Min(
                        numFileDrawerThreshold.Maximum,
                        _settings.FileDrawerSuggestionThreshold
                    )
                )

        End Sub


        Private Sub SaveSettings(
            sender As Object,
            e As EventArgs
        )

            Dim newAppearance As AppAppearance

            If rbLight.Checked Then

                newAppearance =
                    AppAppearance.Light

            ElseIf rbDark.Checked Then

                newAppearance =
                    AppAppearance.Dark

            Else

                newAppearance =
                    AppAppearance.System

            End If

            _appearanceChanged =
                newAppearance <> _settings.Appearance

            _settings.Appearance =
                newAppearance

            _settings.FileDrawerSuggestionThreshold =
                CInt(
                    numFileDrawerThreshold.Value
                )

            Try

                _settingsService.Save(
                    _settings
                )

            Catch ex As Exception

                MessageBox.Show(
                    Me,
                    "ManuscriptPipeline could not save the settings." &
                    Environment.NewLine &
                    Environment.NewLine &
                    ex.Message,
                    "Settings Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

                Return

            End Try

            Me.DialogResult =
                DialogResult.OK

        End Sub

    End Class

End Namespace