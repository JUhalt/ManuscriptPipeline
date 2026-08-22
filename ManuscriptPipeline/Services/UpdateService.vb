Imports System
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports ManuscriptPipeline.Forms
Imports ManuscriptPipeline.Models
Imports Velopack
Imports Velopack.Sources

Namespace Services

    Public NotInheritable Class UpdateService

        Private Const RepositoryUrl As String =
            "https://github.com/JUhalt/PaperRoute-Tracker"


        Private Sub New()
        End Sub


        Public Shared Function CurrentVersionText() As String

            Dim version As String =
                Application.ProductVersion

            Dim metadataIndex As Integer =
                version.IndexOf("+"c)

            If metadataIndex >= 0 Then
                version = version.Substring(0, metadataIndex)
            End If

            Return version

        End Function


        Public Shared Function IsInstalledBuild(
            channel As AppUpdateChannel
        ) As Boolean

            Try

                Dim manager As UpdateManager =
                    CreateManager(channel)

                Return manager.IsInstalled

            Catch

                Return False

            End Try

        End Function


        Public Shared Function ChannelDisplayName(
            channel As AppUpdateChannel
        ) As String

            If channel = AppUpdateChannel.Preview Then
                Return "Preview"
            End If

            Return "Stable"

        End Function


        Public Shared Async Function CheckAndOfferUpdateAsync(
            owner As IWin32Window,
            channel As AppUpdateChannel,
            interactive As Boolean
        ) As Task(Of Boolean)

            Try

                Dim manager As UpdateManager =
                    CreateManager(channel)

                If Not manager.IsInstalled Then

                    If interactive Then

                        MessageBox.Show(
                            owner,
                            "Update checking is available in installed PaperRoute builds." &
                            Environment.NewLine & Environment.NewLine &
                            "This copy appears to be running from Visual Studio or as a portable build. " &
                            "Install PaperRoute using the Setup program from a GitHub release to test automatic updates.",
                            "Updates Unavailable",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        )

                    End If

                    Return False

                End If


                Dim updateInfo As UpdateInfo =
                    Await manager.CheckForUpdatesAsync()

                If updateInfo Is Nothing Then

                    If interactive Then

                        MessageBox.Show(
                            owner,
                            "You're up to date." &
                            Environment.NewLine & Environment.NewLine &
                            "Current version: " & CurrentInstalledVersion(manager) &
                            Environment.NewLine &
                            "Channel: " & ChannelDisplayName(channel),
                            "PaperRoute Updates",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        )

                    End If

                    Return False

                End If


                Dim availableVersion As String =
                    updateInfo.TargetFullRelease.Version.ToString()

                Dim releaseNotes As String =
                    updateInfo.TargetFullRelease.NotesMarkdown

                If String.IsNullOrWhiteSpace(releaseNotes) Then
                    releaseNotes = "No release notes were included with this update."
                End If


                Using prompt As New UpdatePromptForm(
                    CurrentInstalledVersion(manager),
                    availableVersion,
                    ChannelDisplayName(channel),
                    releaseNotes
                )

                    If prompt.ShowDialog(owner) <> DialogResult.OK Then
                        Return False
                    End If

                End Using


                Using progressDialog As New UpdateProgressForm(
                    availableVersion
                )

                    progressDialog.Show(owner)
                    progressDialog.SetProgress(0)
                    progressDialog.SetStatus("Downloading update...")

                    Dim progressCallback As New Action(Of Integer)(
                        Sub(value As Integer)
                            progressDialog.SetProgress(value)
                        End Sub
                    )

                    Await manager.DownloadUpdatesAsync(
                        updateInfo,
                        progressCallback
                    )

                    progressDialog.SetProgress(100)
                    progressDialog.SetStatus("Installing update and restarting PaperRoute...")
                    progressDialog.Refresh()

                    manager.ApplyUpdatesAndRestart(
                        updateInfo.TargetFullRelease
                    )

                End Using

                Return True

            Catch ex As Exception

                If interactive Then

                    MessageBox.Show(
                        owner,
                        "PaperRoute could not complete the update check." &
                        Environment.NewLine & Environment.NewLine &
                        ex.Message,
                        "Update Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    )

                End If

                Return False

            End Try

        End Function


        Private Shared Function CreateManager(
            channel As AppUpdateChannel
        ) As UpdateManager

            Dim includePrereleases As Boolean =
                channel = AppUpdateChannel.Preview

            Dim source As New GithubSource(
                RepositoryUrl,
                Nothing,
                includePrereleases
            )

            Dim options As New UpdateOptions With {
                .ExplicitChannel = ChannelName(channel),
                .AllowVersionDowngrade = False
            }

            Return New UpdateManager(
                source,
                options
            )

        End Function


        Private Shared Function ChannelName(
            channel As AppUpdateChannel
        ) As String

            If channel = AppUpdateChannel.Preview Then
                Return "preview"
            End If

            Return "stable"

        End Function


        Private Shared Function CurrentInstalledVersion(
            manager As UpdateManager
        ) As String

            If manager.CurrentVersion IsNot Nothing Then
                Return manager.CurrentVersion.ToString()
            End If

            Return CurrentVersionText()

        End Function

    End Class

End Namespace
