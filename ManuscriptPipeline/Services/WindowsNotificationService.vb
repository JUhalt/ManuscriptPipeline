Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms

Namespace Services

    Public Class WindowsNotificationService
        Implements IDisposable

        Private ReadOnly _activeIcons As New List(Of NotifyIcon)()
        Private ReadOnly _timers As New List(Of Timer)()

        Private _disposed As Boolean = False


        Public Function TryShow(
            title As String,
            message As String
        ) As Boolean

            If _disposed Then
                Return False
            End If

            If String.IsNullOrWhiteSpace(
                message
            ) Then

                Return False

            End If

            Try

                Dim icon As New NotifyIcon With {
                    .Icon = SystemIcons.Information,
                    .Text = "PaperRoute",
                    .Visible = True,
                    .BalloonTipTitle =
                        If(
                            String.IsNullOrWhiteSpace(
                                title
                            ),
                            "PaperRoute reminder",
                            title.Trim()
                        ),
                    .BalloonTipText = message.Trim(),
                    .BalloonTipIcon = ToolTipIcon.Info
                }

                _activeIcons.Add(
                    icon
                )

                icon.ShowBalloonTip(
                    8000
                )

                Dim cleanupTimer As New Timer With {
                    .Interval = 12000
                }

                _timers.Add(
                    cleanupTimer
                )

                AddHandler cleanupTimer.Tick,
                    Sub(sender, e)

                        cleanupTimer.Stop()

                        _timers.Remove(
                            cleanupTimer
                        )

                        cleanupTimer.Dispose()

                        If _activeIcons.Contains(
                            icon
                        ) Then

                            _activeIcons.Remove(
                                icon
                            )

                        End If

                        icon.Visible =
                            False

                        icon.Dispose()

                    End Sub

                cleanupTimer.Start()

                Return True

            Catch

                Return False

            End Try

        End Function


        Public Sub Dispose() Implements IDisposable.Dispose

            If _disposed Then
                Return
            End If

            _disposed =
                True

            For Each timer As Timer In _timers

                Try

                    timer.Stop()
                    timer.Dispose()

                Catch
                End Try

            Next

            _timers.Clear()

            For Each icon As NotifyIcon In _activeIcons

                Try

                    icon.Visible =
                        False

                    icon.Dispose()

                Catch
                End Try

            Next

            _activeIcons.Clear()

        End Sub

    End Class

End Namespace
