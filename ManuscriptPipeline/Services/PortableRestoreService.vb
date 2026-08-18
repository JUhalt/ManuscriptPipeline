Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class BackupInspection

        Public Property ManuscriptCount As Integer
        Public Property SubmissionCount As Integer
        Public Property DecisionCount As Integer
        Public Property CorrespondenceCount As Integer
        Public Property ManagedFileCount As Integer
        Public Property ArchiveEntryCount As Integer
        Public Property UncompressedBytes As Long

    End Class


    Public Class RestoreResult

        Public Property ManuscriptCount As Integer
        Public Property EmergencyBackupPath As String

    End Class


    Public Class PortableRestoreService

        Private Const MaximumArchiveEntries As Integer = 20000
        Private Const MaximumUncompressedBytes As Long = 21474836480L

        Private ReadOnly _jsonOptions As JsonSerializerOptions


        Public Sub New()

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
        ' Inspect
        ' =====================================================

        Public Function InspectBackup(
            backupZipPath As String
        ) As BackupInspection

            ValidateBackupPath(
                backupZipPath
            )

            ValidateArchiveLimits(
                backupZipPath
            )

            Dim extractionDirectory As String =
                CreateTemporaryDirectory()

            Try

                ZipFile.ExtractToDirectory(
                    backupZipPath,
                    extractionDirectory
                )

                Dim jsonPath As String =
                    Path.Combine(
                        extractionDirectory,
                        "manuscripts.json"
                    )

                If Not File.Exists(jsonPath) Then

                    Throw New InvalidDataException(
                        "This archive does not contain manuscripts.json and does not appear to be a ManuscriptPipeline backup."
                    )

                End If

                Dim restoredManuscripts As List(Of Manuscript) =
                    ReadManuscriptsJson(
                        jsonPath
                    )

                ValidateManagedFiles(
                    restoredManuscripts,
                    extractionDirectory
                )

                Dim inspection As BackupInspection =
                    BuildInspection(
                        restoredManuscripts
                    )

                Using archive As ZipArchive = ZipFile.OpenRead(backupZipPath)

                    inspection.ArchiveEntryCount =
                        archive.Entries.Count

                    For Each entry As ZipArchiveEntry In archive.Entries
                        inspection.UncompressedBytes += entry.Length
                    Next

                End Using

                Return inspection

            Finally

                SafeDeleteDirectory(
                    extractionDirectory
                )

            End Try

        End Function


        ' =====================================================
        ' Restore
        ' =====================================================

        Public Function RestoreBackup(
            backupZipPath As String,
            currentManuscripts As List(Of Manuscript),
            repository As ManuscriptRepository
        ) As RestoreResult

            ValidateBackupPath(
                backupZipPath
            )

            ValidateArchiveLimits(
                backupZipPath
            )

            If currentManuscripts Is Nothing Then
                Throw New ArgumentNullException(NameOf(currentManuscripts))
            End If

            If repository Is Nothing Then
                Throw New ArgumentNullException(NameOf(repository))
            End If

            Dim extractionDirectory As String =
                CreateTemporaryDirectory()

            Dim managedLibrary As New ManagedLibraryService()

            Dim restoreId As String =
                Guid.NewGuid().ToString("N")

            Dim dataDirectory As String =
                Path.GetDirectoryName(
                    repository.DataFilePath
                )

            If String.IsNullOrWhiteSpace(dataDirectory) Then
                Throw New InvalidOperationException("The ManuscriptPipeline data directory could not be determined.")
            End If

            Directory.CreateDirectory(
                dataDirectory
            )

            Dim managedRoot As String =
                managedLibrary.RootDirectory

            Dim managedParent As String =
                Path.GetDirectoryName(
                    managedRoot
                )

            If String.IsNullOrWhiteSpace(managedParent) Then
                Throw New InvalidOperationException("The managed-library parent directory could not be determined.")
            End If

            Directory.CreateDirectory(
                managedParent
            )

            Dim stagedFilesDirectory As String =
                Path.Combine(
                    managedParent,
                    "ManuscriptPipeline Library.restore-staging-" & restoreId
                )

            Dim rollbackFilesDirectory As String =
                Path.Combine(
                    managedParent,
                    "ManuscriptPipeline Library.restore-rollback-" & restoreId
                )

            Dim stagedJsonPath As String =
                Path.Combine(
                    dataDirectory,
                    "restore-staging-" & restoreId & ".json"
                )

            Dim rollbackJsonPath As String =
                Path.Combine(
                    dataDirectory,
                    "restore-rollback-" & restoreId & ".json"
                )

            Dim emergencyBackupPath As String =
                String.Empty

            Try

                ' =============================================
                ' Extract backup into temporary staging area.
                ' =============================================

                ZipFile.ExtractToDirectory(
                    backupZipPath,
                    extractionDirectory
                )

                Dim extractedJsonPath As String =
                    Path.Combine(
                        extractionDirectory,
                        "manuscripts.json"
                    )

                If Not File.Exists(extractedJsonPath) Then

                    Throw New InvalidDataException(
                        "This archive does not contain manuscripts.json."
                    )

                End If

                Dim restoredManuscripts As List(Of Manuscript) =
                    ReadManuscriptsJson(
                        extractedJsonPath
                    )

                ValidateManagedFiles(
                    restoredManuscripts,
                    extractionDirectory
                )

                ' =============================================
                ' Rewrite managed paths for THIS computer.
                ' =============================================

                RewriteManagedPaths(
                    restoredManuscripts,
                    managedRoot
                )

                ' =============================================
                ' Build staged managed library.
                ' =============================================

                If Directory.Exists(stagedFilesDirectory) Then
                    Directory.Delete(stagedFilesDirectory, True)
                End If

                Directory.CreateDirectory(
                    stagedFilesDirectory
                )

                Dim extractedFilesDirectory As String =
                    Path.Combine(
                        extractionDirectory,
                        "files"
                    )

                If Directory.Exists(extractedFilesDirectory) Then

                    CopyDirectory(
                        extractedFilesDirectory,
                        stagedFilesDirectory
                    )

                End If

                ' =============================================
                ' Serialize restored JSON to staging.
                ' =============================================

                Dim restoredJson As String =
                    JsonSerializer.Serialize(
                        restoredManuscripts,
                        _jsonOptions
                    )

                File.WriteAllText(
                    stagedJsonPath,
                    restoredJson
                )

                ' =============================================
                ' Emergency backup of current state.
                ' =============================================

                If File.Exists(repository.DataFilePath) Then

                    Dim backupDirectory As String =
                        Path.Combine(
                            dataDirectory,
                            "backups"
                        )

                    Directory.CreateDirectory(
                        backupDirectory
                    )

                    emergencyBackupPath =
                        Path.Combine(
                            backupDirectory,
                            "ManuscriptPipeline_PreRestore_" &
                            DateTime.Now.ToString("yyyy-MM-dd_HHmmss") &
                            ".zip"
                        )

                    Dim backupService As New PortableBackupService()

                    backupService.CreateBackup(
                        emergencyBackupPath,
                        currentManuscripts,
                        repository
                    )

                End If

                ' =============================================
                ' Swap live managed library.
                ' =============================================

                Dim oldLibraryMoved As Boolean = False
                Dim newLibraryInstalled As Boolean = False
                Dim originalJsonExisted As Boolean =
                    File.Exists(repository.DataFilePath)

                Try

                    If Directory.Exists(managedRoot) Then

                        If Directory.Exists(rollbackFilesDirectory) Then
                            Directory.Delete(rollbackFilesDirectory, True)
                        End If

                        Directory.Move(
                            managedRoot,
                            rollbackFilesDirectory
                        )

                        oldLibraryMoved =
                            True

                    End If

                    Directory.Move(
                        stagedFilesDirectory,
                        managedRoot
                    )

                    newLibraryInstalled =
                        True

                    ' =========================================
                    ' Swap JSON last.
                    ' =========================================

                    If originalJsonExisted Then

                        File.Copy(
                            repository.DataFilePath,
                            rollbackJsonPath,
                            True
                        )

                    End If

                    File.Copy(
                        stagedJsonPath,
                        repository.DataFilePath,
                        True
                    )

                Catch

                    ' =========================================
                    ' Roll back JSON.
                    ' =========================================

                    Try

                        If originalJsonExisted AndAlso
                           File.Exists(rollbackJsonPath) Then

                            File.Copy(
                                rollbackJsonPath,
                                repository.DataFilePath,
                                True
                            )

                        ElseIf Not originalJsonExisted AndAlso
                               File.Exists(repository.DataFilePath) Then

                            File.Delete(
                                repository.DataFilePath
                            )

                        End If

                    Catch
                        ' Best-effort rollback.
                    End Try

                    ' =========================================
                    ' Roll back managed library.
                    ' =========================================

                    Try

                        If newLibraryInstalled AndAlso
                           Directory.Exists(managedRoot) Then

                            Directory.Delete(
                                managedRoot,
                                True
                            )

                        End If

                        If oldLibraryMoved AndAlso
                           Directory.Exists(rollbackFilesDirectory) Then

                            Directory.Move(
                                rollbackFilesDirectory,
                                managedRoot
                            )

                        End If

                    Catch
                        ' Best-effort rollback.
                    End Try

                    Throw

                End Try

                ' =============================================
                ' Restore succeeded.
                ' Old temporary live-state copies may go.
                ' Emergency ZIP remains intentionally.
                ' =============================================

                SafeDeleteDirectory(
                    rollbackFilesDirectory
                )

                SafeDeleteFile(
                    rollbackJsonPath
                )

                Return New RestoreResult With {
                    .ManuscriptCount = restoredManuscripts.Count,
                    .EmergencyBackupPath = emergencyBackupPath
                }

            Finally

                SafeDeleteDirectory(
                    extractionDirectory
                )

                SafeDeleteDirectory(
                    stagedFilesDirectory
                )

                SafeDeleteFile(
                    stagedJsonPath
                )

            End Try

        End Function


        ' =====================================================
        ' Archive validation
        ' =====================================================

        Private Sub ValidateBackupPath(
            backupZipPath As String
        )

            If String.IsNullOrWhiteSpace(backupZipPath) Then
                Throw New ArgumentException("A backup ZIP path is required.")
            End If

            If Not File.Exists(backupZipPath) Then

                Throw New FileNotFoundException(
                    "The selected backup archive could not be found.",
                    backupZipPath
                )

            End If

        End Sub


        Private Sub ValidateArchiveLimits(
            backupZipPath As String
        )

            Using archive As ZipArchive = ZipFile.OpenRead(backupZipPath)

                If archive.Entries.Count > MaximumArchiveEntries Then

                    Throw New InvalidDataException(
                        "The backup archive contains too many entries to restore safely."
                    )

                End If

                Dim totalUncompressedBytes As Long = 0

                For Each entry As ZipArchiveEntry In archive.Entries

                    totalUncompressedBytes +=
                        entry.Length

                    If totalUncompressedBytes > MaximumUncompressedBytes Then

                        Throw New InvalidDataException(
                            "The backup archive expands beyond the restore safety limit."
                        )

                    End If

                Next

            End Using

        End Sub


        ' =====================================================
        ' JSON
        ' =====================================================

        Private Function ReadManuscriptsJson(
            jsonPath As String
        ) As List(Of Manuscript)

            Dim json As String =
                File.ReadAllText(
                    jsonPath
                )

            If String.IsNullOrWhiteSpace(json) Then

                Throw New InvalidDataException(
                    "The backup contains an empty manuscripts.json file."
                )

            End If

            Dim restoredManuscripts As List(Of Manuscript) =
                JsonSerializer.Deserialize(Of List(Of Manuscript))(json, _jsonOptions)

            If restoredManuscripts Is Nothing Then

                Throw New InvalidDataException(
                    "The backup manuscript data could not be read."
                )

            End If

            NormalizeLoadedData(
                restoredManuscripts
            )

            Return restoredManuscripts

        End Function


        Private Sub NormalizeLoadedData(
            manuscripts As List(Of Manuscript)
        )

            For Each manuscript As Manuscript In manuscripts

                If manuscript.History Is Nothing Then
                    manuscript.History = New List(Of HistoryEvent)()
                End If

                If manuscript.Submissions Is Nothing Then
                    manuscript.Submissions = New List(Of JournalSubmission)()
                End If

                For Each submission As JournalSubmission In manuscript.Submissions

                    If submission.Decisions Is Nothing Then
                        submission.Decisions = New List(Of EditorialDecisionEvent)()
                    End If

                    If submission.Correspondence Is Nothing Then
                        submission.Correspondence = New List(Of CorrespondenceItem)()
                    End If

                Next

            Next

        End Sub


        ' =====================================================
        ' Managed files
        ' =====================================================

        Private Sub ValidateManagedFiles(
            manuscripts As List(Of Manuscript),
            extractionDirectory As String
        )

            Dim extractedFilesRoot As String =
                Path.Combine(
                    extractionDirectory,
                    "files"
                )

            For Each manuscript As Manuscript In manuscripts

                For Each submission As JournalSubmission In manuscript.Submissions

                    For Each item As CorrespondenceItem In submission.Correspondence

                        If Not item.IsManagedCopy Then
                            Continue For
                        End If

                        If String.IsNullOrWhiteSpace(item.LocalFilePath) Then

                            Throw New InvalidDataException(
                                "A managed correspondence record does not contain a file path."
                            )

                        End If

                        Dim fileName As String =
                            Path.GetFileName(
                                item.LocalFilePath
                            )

                        Dim expectedBackupFile As String =
                            Path.Combine(
                                extractedFilesRoot,
                                manuscript.Id.ToString("N"),
                                submission.Id.ToString("N"),
                                item.Id.ToString("N"),
                                fileName
                            )

                        If Not File.Exists(expectedBackupFile) Then

                            Throw New InvalidDataException(
                                "The backup is missing a managed file: " &
                                fileName
                            )

                        End If

                    Next

                Next

            Next

        End Sub


        Private Sub RewriteManagedPaths(
            manuscripts As List(Of Manuscript),
            managedRoot As String
        )

            For Each manuscript As Manuscript In manuscripts

                For Each submission As JournalSubmission In manuscript.Submissions

                    For Each item As CorrespondenceItem In submission.Correspondence

                        If Not item.IsManagedCopy Then
                            Continue For
                        End If

                        Dim fileName As String =
                            Path.GetFileName(
                                item.LocalFilePath
                            )

                        item.LocalFilePath =
                            Path.Combine(
                                managedRoot,
                                manuscript.Id.ToString("N"),
                                submission.Id.ToString("N"),
                                item.Id.ToString("N"),
                                fileName
                            )

                    Next

                Next

            Next

        End Sub


        ' =====================================================
        ' Inspection
        ' =====================================================

        Private Function BuildInspection(
            manuscripts As List(Of Manuscript)
        ) As BackupInspection

            Dim result As New BackupInspection()

            result.ManuscriptCount =
                manuscripts.Count

            For Each manuscript As Manuscript In manuscripts

                result.SubmissionCount +=
                    manuscript.Submissions.Count

                For Each submission As JournalSubmission In manuscript.Submissions

                    result.DecisionCount +=
                        submission.Decisions.Count

                    result.CorrespondenceCount +=
                        submission.Correspondence.Count

                    For Each item As CorrespondenceItem In submission.Correspondence

                        If item.IsManagedCopy Then
                            result.ManagedFileCount += 1
                        End If

                    Next

                Next

            Next

            Return result

        End Function


        ' =====================================================
        ' Filesystem helpers
        ' =====================================================

        Private Function CreateTemporaryDirectory() As String

            Dim directoryPath As String =
                Path.Combine(
                    Path.GetTempPath(),
                    "ManuscriptPipelineRestore_" &
                    Guid.NewGuid().ToString("N")
                )

            Directory.CreateDirectory(
                directoryPath
            )

            Return directoryPath

        End Function


        Private Sub CopyDirectory(
            sourceDirectory As String,
            destinationDirectory As String
        )

            Directory.CreateDirectory(
                destinationDirectory
            )

            For Each sourceFile As String In Directory.GetFiles(sourceDirectory)

                File.Copy(
                    sourceFile,
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceFile)
                    ),
                    True
                )

            Next

            For Each sourceSubdirectory As String In Directory.GetDirectories(sourceDirectory)

                CopyDirectory(
                    sourceSubdirectory,
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceSubdirectory)
                    )
                )

            Next

        End Sub


        Private Sub SafeDeleteDirectory(
            directoryPath As String
        )

            If String.IsNullOrWhiteSpace(directoryPath) Then
                Return
            End If

            Try

                If Directory.Exists(directoryPath) Then
                    Directory.Delete(directoryPath, True)
                End If

            Catch
                ' Best-effort cleanup.
            End Try

        End Sub


        Private Sub SafeDeleteFile(
            filePath As String
        )

            If String.IsNullOrWhiteSpace(filePath) Then
                Return
            End If

            Try

                If File.Exists(filePath) Then
                    File.Delete(filePath)
                End If

            Catch
                ' Best-effort cleanup.
            End Try

        End Sub

    End Class

End Namespace