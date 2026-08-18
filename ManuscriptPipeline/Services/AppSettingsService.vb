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
                    "ManuscriptPipeline"
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

                If settings.FileDrawerSuggestionThreshold < 1 Then
                    settings.FileDrawerSuggestionThreshold = 1
                End If

                If settings.FileDrawerSuggestionThreshold > 20 Then
                    settings.FileDrawerSuggestionThreshold = 20
                End If

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

    End Class

End Namespace