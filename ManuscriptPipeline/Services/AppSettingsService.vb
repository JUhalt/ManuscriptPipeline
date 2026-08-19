Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class AppSettingsService

        Private ReadOnly _settingsDirectory As String
        Private ReadOnly _settingsPath As String
        Private ReadOnly _jsonOptions As JsonSerializerOptions


        Public Sub New()

            _settingsDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    ),
                    ProductInfo.DataFolderName
                )

            _settingsPath =
                Path.Combine(
                    _settingsDirectory,
                    "settings.json"
                )

            _jsonOptions =
                New JsonSerializerOptions With {
                    .WriteIndented = True,
                    .PropertyNameCaseInsensitive = True
                }

            _jsonOptions.Converters.Add(
                New JsonStringEnumConverter()
            )

        End Sub


        Public Function Load() As AppSettings

            Try

                If Not File.Exists(_settingsPath) Then
                    Return New AppSettings()
                End If

                Dim json As String =
                    File.ReadAllText(
                        _settingsPath
                    )

                If String.IsNullOrWhiteSpace(json) Then
                    Return New AppSettings()
                End If

                Dim settings As AppSettings =
                    JsonSerializer.Deserialize(Of AppSettings)(
                        json,
                        _jsonOptions
                    )

                If settings Is Nothing Then
                    Return New AppSettings()
                End If

                Normalize(
                    settings
                )

                Return settings

            Catch

                Return New AppSettings()

            End Try

        End Function


        Public Sub Save(
            settings As AppSettings
        )

            If settings Is Nothing Then
                Throw New ArgumentNullException(NameOf(settings))
            End If

            Normalize(
                settings
            )

            Directory.CreateDirectory(
                _settingsDirectory
            )

            Dim json As String =
                JsonSerializer.Serialize(
                    settings,
                    _jsonOptions
                )

            File.WriteAllText(
                _settingsPath,
                json
            )

        End Sub


        Private Sub Normalize(
            settings As AppSettings
        )

            settings.FileDrawerSuggestionThreshold =
                Math.Max(
                    1,
                    Math.Min(
                        20,
                        settings.FileDrawerSuggestionThreshold
                    )
                )

            settings.LongReviewThresholdDays =
                Math.Max(
                    1,
                    Math.Min(
                        730,
                        settings.LongReviewThresholdDays
                    )
                )

            settings.RevisionWarningDays =
                Math.Max(
                    1,
                    Math.Min(
                        180,
                        settings.RevisionWarningDays
                    )
                )

            settings.RecentRejectionThresholdDays =
                Math.Max(
                    1,
                    Math.Min(
                        365,
                        settings.RecentRejectionThresholdDays
                    )
                )

        End Sub

    End Class

End Namespace