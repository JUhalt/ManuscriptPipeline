Imports System
Imports System.Drawing
Imports System.Windows.Forms

Namespace Services

    Public NotInheritable Class UiPolish

        Private Sub New()
        End Sub


        Public Shared Sub ApplyDialog(
            form As Form
        )

            If form Is Nothing Then
                Return
            End If

            form.BackColor =
                UiTheme.BoardBackground()

            ApplyToControlTree(
                form
            )

        End Sub


        Private Shared Sub ApplyToControlTree(
            parent As Control
        )

            For Each control As Control In parent.Controls

                ApplyControlStyle(
                    control
                )

                If control.HasChildren Then

                    ApplyToControlTree(
                        control
                    )

                End If

            Next

        End Sub


        Private Shared Sub ApplyControlStyle(
            control As Control
        )

            If TypeOf control Is Button Then

                StyleButton(
                    DirectCast(
                        control,
                        Button
                    )
                )

                Return

            End If


            If TypeOf control Is TextBox Then

                Dim textBox As TextBox =
                    DirectCast(
                        control,
                        TextBox
                    )

                textBox.BackColor =
                    UiTheme.CardBackground()

                textBox.ForeColor =
                    UiTheme.PrimaryText()

                textBox.BorderStyle =
                    BorderStyle.FixedSingle

                Return

            End If


            If TypeOf control Is ComboBox Then

                Dim comboBox As ComboBox =
                    DirectCast(
                        control,
                        ComboBox
                    )

                comboBox.BackColor =
                    UiTheme.CardBackground()

                comboBox.ForeColor =
                    UiTheme.PrimaryText()

                comboBox.FlatStyle =
                    FlatStyle.Flat

                Return

            End If


            If TypeOf control Is ListBox Then

                Dim listBox As ListBox =
                    DirectCast(
                        control,
                        ListBox
                    )

                listBox.BackColor =
                    UiTheme.CardBackground()

                listBox.ForeColor =
                    UiTheme.PrimaryText()

                listBox.BorderStyle =
                    BorderStyle.FixedSingle

                Return

            End If


            If TypeOf control Is NumericUpDown Then

                Dim numeric As NumericUpDown =
                    DirectCast(
                        control,
                        NumericUpDown
                    )

                numeric.BackColor =
                    UiTheme.CardBackground()

                numeric.ForeColor =
                    UiTheme.PrimaryText()

                Return

            End If


            If TypeOf control Is DateTimePicker Then

                Dim datePicker As DateTimePicker =
                    DirectCast(
                        control,
                        DateTimePicker
                    )

                datePicker.CalendarMonthBackground =
                    UiTheme.CardBackground()

                datePicker.CalendarForeColor =
                    UiTheme.PrimaryText()

                Return

            End If


            If TypeOf control Is GroupBox Then

                Dim groupBox As GroupBox =
                    DirectCast(
                        control,
                        GroupBox
                    )

                groupBox.ForeColor =
                    UiTheme.PrimaryText()

                groupBox.BackColor =
                    UiTheme.BoardBackground()

                Return

            End If


            If TypeOf control Is Label Then

                Dim label As Label =
                    DirectCast(
                        control,
                        Label
                    )

                If label.ForeColor =
                    SystemColors.GrayText Then

                    label.ForeColor =
                        UiTheme.SecondaryText()

                Else

                    label.ForeColor =
                        UiTheme.PrimaryText()

                End If

                Return

            End If


            If TypeOf control Is TableLayoutPanel OrElse
               TypeOf control Is FlowLayoutPanel Then

                control.BackColor =
                    UiTheme.BoardBackground()

                Return

            End If

        End Sub


        Private Shared Sub StyleButton(
            button As Button
        )

            button.FlatStyle =
                FlatStyle.Flat

            button.UseVisualStyleBackColor =
                False

            button.BackColor =
                UiTheme.CardBackground()

            button.FlatAppearance.BorderSize =
                1

            button.Cursor =
                Cursors.Hand


            Dim buttonText As String =
                button.Text.Trim().ToUpperInvariant()


            If buttonText.Contains("DELETE") OrElse
               buttonText.Contains("REMOVE") Then

                ApplyButtonAccent(
                    button,
                    UiTheme.DangerColor()
                )

                Return

            End If


            If buttonText.Contains("FILE DRAWER") Then

                ApplyButtonAccent(
                    button,
                    UiTheme.WarningColor()
                )

                Return

            End If


            If buttonText.Contains("RESTORE") Then

                ApplyButtonAccent(
                    button,
                    UiTheme.SuccessColor()
                )

                Return

            End If


            If buttonText = "CANCEL" OrElse
               buttonText = "NO" OrElse
               buttonText = "CLOSE" Then

                ApplyButtonAccent(
                    button,
                    UiTheme.SecondaryText()
                )

                Return

            End If


            ApplyButtonAccent(
                button,
                UiTheme.AccentColor()
            )

        End Sub


        Private Shared Sub ApplyButtonAccent(
            button As Button,
            accent As Color
        )

            button.ForeColor =
                accent

            button.FlatAppearance.BorderColor =
                accent

            button.FlatAppearance.MouseOverBackColor =
                UiTheme.HoverBackground()

            button.FlatAppearance.MouseDownBackColor =
                UiTheme.HoverBackground()

        End Sub

    End Class

End Namespace