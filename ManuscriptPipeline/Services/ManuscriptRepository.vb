Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class ManuscriptRepository

        Private ReadOnly _dataDirectory As String
        Private ReadOnly _dataFilePath As String
        Private ReadOnly _backupFilePath As String
        Private ReadOnly _jsonOptions As JsonSerializerOptions
        Private ReadOnly _managedLibrary As New ManagedLibraryService()


        Public Sub New()

            _dataDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    ),
                    "ManuscriptPipeline",
                    "data"
                )

            _dataFilePath =
                Path.Combine(
                    _dataDirectory,
                    "manuscripts.json"
                )

            _backupFilePath =
                Path.Combine(
                    _dataDirectory,
                    "manuscripts.bak"
                )

            _jsonOptions =
                New JsonSerializerOptions With {
                    .WriteIndented = True,
                    .IgnoreReadOnlyProperties = True,
                    .PropertyNameCaseInsensitive = True
                }

            _jsonOptions.Converters.Add(
                New JsonStringEnumConverter()
            )

        End Sub


        ' =====================================================
        ' Paths
        ' =====================================================

        Public ReadOnly Property DataFilePath As String
            Get
                Return _dataFilePath
            End Get
        End Property


        Public ReadOnly Property BackupFilePath As String
            Get
                Return _backupFilePath
            End Get
        End Property


        ' =====================================================
        ' Load
        ' =====================================================

        Public Function Load() As List(Of Manuscript)

            Directory.CreateDirectory(
                _dataDirectory
            )

            If Not File.Exists(_dataFilePath) Then
                Return New List(Of Manuscript)()
            End If

            Dim json As String =
                File.ReadAllText(
                    _dataFilePath
                )

            If String.IsNullOrWhiteSpace(json) Then
                Return New List(Of Manuscript)()
            End If

            Dim loadedManuscripts As List(Of Manuscript) =
                JsonSerializer.Deserialize(Of List(Of Manuscript))(json, _jsonOptions)

            If loadedManuscripts Is Nothing Then
                Return New List(Of Manuscript)()
            End If

            NormalizeLoadedData(
                loadedManuscripts
            )

            Return loadedManuscripts

        End Function


        ' =====================================================
        ' Save
        ' =====================================================

        Public Sub Save(
            manuscripts As List(Of Manuscript)
        )

            Directory.CreateDirectory(
                _dataDirectory
            )

            ' Finish pending managed-library copies first.
            _managedLibrary.CommitManagedCopies(
                manuscripts
            )

            Dim json As String =
                JsonSerializer.Serialize(
                    manuscripts,
                    _jsonOptions
                )

            If File.Exists(_dataFilePath) Then

                File.Copy(
                    _dataFilePath,
                    _backupFilePath,
                    True
                )

            End If

            File.WriteAllText(
                _dataFilePath,
                json
            )

        End Sub


        ' =====================================================
        ' Pre-import safety backup
        ' =====================================================

        Public Function CreatePreImportBackup() As String

            If Not File.Exists(_dataFilePath) Then
                Return String.Empty
            End If

            Dim backupDirectory As String =
                Path.Combine(
                    _dataDirectory,
                    "backups"
                )

            Directory.CreateDirectory(
                backupDirectory
            )

            Dim backupName As String =
                "manuscripts_pre-import_" &
                DateTime.Now.ToString("yyyyMMdd_HHmmss") &
                ".json"

            Dim backupPath As String =
                Path.Combine(
                    backupDirectory,
                    backupName
                )

            File.Copy(
                _dataFilePath,
                backupPath,
                False
            )

            Return backupPath

        End Function


        ' =====================================================
        ' Compatibility / normalization
        ' =====================================================

        Private Sub NormalizeLoadedData(
            manuscripts As List(Of Manuscript)
        )

            For Each manuscript As Manuscript In manuscripts

                If manuscript.CurrentStage = PaperStage.Published AndAlso
   manuscript.Location = ManuscriptLocation.Pipeline Then

                    manuscript.Location = ManuscriptLocation.Published

                End If

                If manuscript.History Is Nothing Then

                    manuscript.History =
                        New List(Of HistoryEvent)()

                End If

                If manuscript.Submissions Is Nothing Then

                    manuscript.Submissions =
                        New List(Of JournalSubmission)()

                End If

                For Each submission As JournalSubmission In manuscript.Submissions

                    If submission.Decisions Is Nothing Then

                        submission.Decisions =
                            New List(Of EditorialDecisionEvent)()

                    End If

                    If submission.Correspondence Is Nothing Then

                        submission.Correspondence =
                            New List(Of CorrespondenceItem)()

                    End If

                Next

            Next

        End Sub

    End Class

End Namespace