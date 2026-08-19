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
        Private ReadOnly numLongReview As New NumericUpDown()
        Private ReadOnly numRevisionWarning As New NumericUpDown()
        Private ReadOnly numRecentRejection As New NumericUpDown()

        Private ReadOnly cboUpdateChannel As New ComboBox()
        Private ReadOnly chkAutomaticUpdates As New CheckBox()

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

            Me.Text = "PaperRoute Preferences"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.Sizable
            Me.MaximizeBox = True
            Me.MinimizeBox = False
            Me.SizeGripStyle = SizeGripStyle.Show
            Me.ClientSize = New Size(760, 720)
            Me.MinimumSize = New Size(620, 520)
            Me.Font = New Font("Segoe UI", 10.0F)
            Me.AutoScaleMode = AutoScaleMode.Dpi

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .Padding = New Padding(12)
            }

            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim contentHost As New Panel With {
                .Dock = DockStyle.Fill,
                .AutoScroll = True,
                .Padding = New Padding(4)
            }

            Dim settingsStack As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .ColumnCount = 1,
                .RowCount = 4,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            settingsStack.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            settingsStack.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            settingsStack.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            settingsStack.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            settingsStack.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            ' =================================================
            ' Appearance
            ' =================================================

            Dim appearanceGroup As New GroupBox With {
                .Text = "Appearance",
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .Padding = New Padding(16),
                .Margin = New Padding(0, 0, 0, 10)
            }

            Dim appearancePanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False,
                .Margin = New Padding(0)
            }

            rbSystem.Text = "Follow Windows"
            rbSystem.AutoSize = True
            rbSystem.Margin = New Padding(0, 4, 0, 6)

            rbLight.Text = "Light"
            rbLight.AutoSize = True
            rbLight.Margin = New Padding(0, 4, 0, 6)

            rbDark.Text = "Dark"
            rbDark.AutoSize = True
            rbDark.Margin = New Padding(0, 4, 0, 6)

            Dim appearanceHelp As New Label With {
                .Text = "Theme changes take effect after PaperRoute restarts.",
                .AutoSize = True,
                .ForeColor = SystemColors.GrayText,
                .Margin = New Padding(22, 8, 0, 4)
            }

            appearancePanel.Controls.Add(rbSystem)
            appearancePanel.Controls.Add(rbLight)
            appearancePanel.Controls.Add(rbDark)
            appearancePanel.Controls.Add(appearanceHelp)
            appearanceGroup.Controls.Add(appearancePanel)

            ' =================================================
            ' Needs Attention
            ' =================================================

            Dim attentionGroup As New GroupBox With {
                .Text = "Needs Attention",
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .Padding = New Padding(16),
                .Margin = New Padding(0, 0, 0, 10)
            }

            Dim attentionGrid As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 3,
                .RowCount = 3,
                .Margin = New Padding(0)
            }

            attentionGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            attentionGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 82))
            attentionGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))

            ConfigureNumber(numLongReview, 1, 730)
            ConfigureNumber(numRevisionWarning, 1, 180)
            ConfigureNumber(numRecentRejection, 1, 365)

            AddSettingRow(attentionGrid, 0, "Flag a long review after", numLongReview, "days")
            AddSettingRow(attentionGrid, 1, "Warn about revision deadlines", numRevisionWarning, "days early")
            AddSettingRow(attentionGrid, 2, "Treat a rejection as recent for", numRecentRejection, "days")

            attentionGroup.Controls.Add(attentionGrid)

            ' =================================================
            ' File Drawer
            ' =================================================

            Dim drawerGroup As New GroupBox With {
                .Text = "File Drawer",
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .Padding = New Padding(16),
                .Margin = New Padding(0, 0, 0, 10)
            }

            Dim drawerGrid As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 3,
                .RowCount = 1,
                .Margin = New Padding(0)
            }

            drawerGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            drawerGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 82))
            drawerGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))
            drawerGrid.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim lblThreshold As New Label With {
                .Text = "Suggest the File Drawer after",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(0, 10, 8, 10)
            }

            ConfigureNumber(numFileDrawerThreshold, 1, 20)
            numFileDrawerThreshold.Anchor = AnchorStyles.Left
            numFileDrawerThreshold.Margin = New Padding(0, 7, 0, 7)

            Dim lblRejections As New Label With {
                .Text = "rejections",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(8, 10, 0, 10)
            }

            drawerGrid.Controls.Add(lblThreshold, 0, 0)
            drawerGrid.Controls.Add(numFileDrawerThreshold, 1, 0)
            drawerGrid.Controls.Add(lblRejections, 2, 0)
            drawerGroup.Controls.Add(drawerGrid)

            ' =================================================
            ' Updates
            ' =================================================

            Dim updatesGroup As New GroupBox With {
                .Text = "Updates",
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .Padding = New Padding(16),
                .Margin = New Padding(0, 0, 0, 4)
            }

            Dim updatesGrid As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 2,
                .RowCount = 2,
                .Margin = New Padding(0)
            }

            updatesGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 165))
            updatesGrid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            updatesGrid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            updatesGrid.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim lblChannel As New Label With {
                .Text = "Update channel",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(0, 10, 8, 10)
            }

            cboUpdateChannel.DropDownStyle = ComboBoxStyle.DropDownList
            cboUpdateChannel.Width = 190
            cboUpdateChannel.Items.Add("Stable")
            cboUpdateChannel.Items.Add("Preview")
            cboUpdateChannel.Anchor = AnchorStyles.Left
            cboUpdateChannel.Margin = New Padding(0, 6, 0, 6)

            Dim lblAutomatic As New Label With {
                .Text = "Automatic checks",
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(0, 10, 8, 10)
            }

            chkAutomaticUpdates.Text = "Check for updates when PaperRoute starts"
            chkAutomaticUpdates.AutoSize = True
            chkAutomaticUpdates.Anchor = AnchorStyles.Left
            chkAutomaticUpdates.Margin = New Padding(0, 8, 0, 8)

            updatesGrid.Controls.Add(lblChannel, 0, 0)
            updatesGrid.Controls.Add(cboUpdateChannel, 1, 0)
            updatesGrid.Controls.Add(lblAutomatic, 0, 1)
            updatesGrid.Controls.Add(chkAutomaticUpdates, 1, 1)

            updatesGroup.Controls.Add(updatesGrid)

            settingsStack.Controls.Add(appearanceGroup, 0, 0)
            settingsStack.Controls.Add(attentionGroup, 0, 1)
            settingsStack.Controls.Add(drawerGroup, 0, 2)
            settingsStack.Controls.Add(updatesGroup, 0, 3)

            contentHost.Controls.Add(settingsStack)

            ' =================================================
            ' Buttons
            ' =================================================

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .Padding = New Padding(0, 10, 0, 0)
            }

            Dim btnSave As New Button With {
                .Text = "Save",
                .Width = 95,
                .Height = 36
            }

            Dim btnCancel As New Button With {
                .Text = "Cancel",
                .Width = 95,
                .Height = 36,
                .DialogResult = DialogResult.Cancel
            }

            AddHandler btnSave.Click,
                AddressOf SaveSettings

            buttons.Controls.Add(btnSave)
            buttons.Controls.Add(btnCancel)

            root.Controls.Add(contentHost, 0, 0)
            root.Controls.Add(buttons, 0, 1)

            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
            Me.Controls.Add(root)

        End Sub


        Private Sub ConfigureNumber(
            numeric As NumericUpDown,
            minimum As Integer,
            maximum As Integer
        )

            numeric.Minimum = minimum
            numeric.Maximum = maximum
            numeric.Width = 70

        End Sub


        Private Sub AddSettingRow(
            grid As TableLayoutPanel,
            row As Integer,
            description As String,
            numeric As NumericUpDown,
            suffix As String
        )

            grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim label As New Label With {
                .Text = description,
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(0, 10, 8, 10)
            }

            numeric.Anchor = AnchorStyles.Left
            numeric.Margin = New Padding(0, 7, 0, 7)

            Dim suffixLabel As New Label With {
                .Text = suffix,
                .AutoSize = True,
                .Anchor = AnchorStyles.Left,
                .Margin = New Padding(8, 10, 0, 10)
            }

            grid.Controls.Add(label, 0, row)
            grid.Controls.Add(numeric, 1, row)
            grid.Controls.Add(suffixLabel, 2, row)

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

            numFileDrawerThreshold.Value = _settings.FileDrawerSuggestionThreshold
            numLongReview.Value = _settings.LongReviewThresholdDays
            numRevisionWarning.Value = _settings.RevisionWarningDays
            numRecentRejection.Value = _settings.RecentRejectionThresholdDays

            If _settings.UpdateChannel = AppUpdateChannel.Preview Then
                cboUpdateChannel.SelectedItem = "Preview"
            Else
                cboUpdateChannel.SelectedItem = "Stable"
            End If

            chkAutomaticUpdates.Checked =
                _settings.CheckForUpdatesAutomatically

        End Sub


        Private Function SelectedUpdateChannel() As AppUpdateChannel

            If String.Equals(
                CStr(cboUpdateChannel.SelectedItem),
                "Preview",
                StringComparison.OrdinalIgnoreCase
            ) Then

                Return AppUpdateChannel.Preview

            End If

            Return AppUpdateChannel.Stable

        End Function


        Private Sub SaveSettings(
            sender As Object,
            e As EventArgs
        )

            Dim newAppearance As AppAppearance

            If rbLight.Checked Then
                newAppearance = AppAppearance.Light
            ElseIf rbDark.Checked Then
                newAppearance = AppAppearance.Dark
            Else
                newAppearance = AppAppearance.System
            End If

            _appearanceChanged =
                newAppearance <> _settings.Appearance

            _settings.Appearance = newAppearance
            _settings.FileDrawerSuggestionThreshold = CInt(numFileDrawerThreshold.Value)
            _settings.LongReviewThresholdDays = CInt(numLongReview.Value)
            _settings.RevisionWarningDays = CInt(numRevisionWarning.Value)
            _settings.RecentRejectionThresholdDays = CInt(numRecentRejection.Value)
            _settings.UpdateChannel = SelectedUpdateChannel()
            _settings.CheckForUpdatesAutomatically = chkAutomaticUpdates.Checked

            Try

                _settingsService.Save(_settings)

            Catch ex As Exception

                MessageBox.Show(
                    Me,
                    "PaperRoute could not save the preferences." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "Preferences Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )

                Return

            End Try

            Me.DialogResult = DialogResult.OK

        End Sub

    End Class

End Namespace
