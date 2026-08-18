Imports Microsoft.VisualBasic.ApplicationServices
Imports System.Windows.Forms
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

Namespace My

    Partial Friend Class MyApplication

        Private Sub MyApplication_ApplyApplicationDefaults(
            sender As Object,
            e As ApplyApplicationDefaultsEventArgs
        ) Handles Me.ApplyApplicationDefaults

            Dim settingsService As New AppSettingsService()

            Dim settings As AppSettings =
                settingsService.Load()

            Select Case settings.Appearance

                Case AppAppearance.Light

                    e.ColorMode =
                        SystemColorMode.Classic

                Case AppAppearance.Dark

                    e.ColorMode =
                        SystemColorMode.Dark

                Case Else

                    e.ColorMode =
                        SystemColorMode.System

            End Select

        End Sub

    End Class

End Namespace