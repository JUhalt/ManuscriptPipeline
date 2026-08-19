Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports ManuscriptPipeline.Models

Namespace Services

    Public NotInheritable Class StorageMigrationService

        Public Const CurrentSchemaVersion As Integer = 1

        Private Sub New()
        End Sub


        Public Shared Sub EnsureCurrentStorage()

            EnsureCurrentStorage(
                CurrentDataRoot(),
                LegacyDataRoot(),
                CurrentManagedLibraryRoot(),
                LegacyManagedLibraryRoot()
            )

        End Sub


        Friend Shared Sub EnsureCurrentStorage(
            currentDataRoot As String,
            legacyDataRoot As String,
            currentManagedLibraryRoot As String,
            legacyManagedLibraryRoot As String
        )

            ValidateRoot(currentDataRoot, NameOf(currentDataRoot))
            ValidateRoot(legacyDataRoot, NameOf(legacyDataRoot))
            ValidateRoot(currentManagedLibraryRoot, NameOf(currentManagedLibraryRoot))
            ValidateRoot(legacyManagedLibraryRoot, NameOf(legacyManagedLibraryRoot))

            MigrateManagedLibraryIfNeeded(
                currentManagedLibraryRoot,
                legacyManagedLibraryRoot
            )

            MigrateApplicationDataIfNeeded(
                currentDataRoot,
                legacyDataRoot,
                currentManagedLibraryRoot,
                legacyManagedLibraryRoot
            )

            EnsureSchemaVersion(currentDataRoot)

        End Sub


        Public Shared Function CurrentDataRoot() As String

            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProductInfo.DataFolderName
            )

        End Function


        Public Shared Function LegacyDataRoot() As String

            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProductInfo.LegacyDataFolderName
            )

        End Function


        Public Shared Function CurrentManagedLibraryRoot() As String

            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                ProductInfo.ManagedLibraryFolderName
            )

        End Function


        Public Shared Function LegacyManagedLibraryRoot() As String

            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                ProductInfo.LegacyManagedLibraryFolderName
            )

        End Function


        Public Shared Function SchemaFilePath() As String
            Return SchemaFilePath(CurrentDataRoot())
        End Function


        Friend Shared Function SchemaFilePath(
            currentDataRoot As String
        ) As String

            Return Path.Combine(
                currentDataRoot,
                "data",
                "schema.json"
            )

        End Function


        Public Shared Function ReadSchemaVersion() As Integer
            Return ReadSchemaVersion(SchemaFilePath())
        End Function


        Friend Shared Function ReadSchemaVersion(
            schemaPath As String
        ) As Integer

            Try

                If Not File.Exists(schemaPath) Then
                    Return 0
                End If

                Using document As JsonDocument = JsonDocument.Parse(File.ReadAllText(schemaPath))

                    Dim root As JsonElement = document.RootElement
                    Dim versionElement As JsonElement

                    If root.TryGetProperty("SchemaVersion", versionElement) AndAlso
                       versionElement.ValueKind = JsonValueKind.Number Then

                        Return versionElement.GetInt32()

                    End If

                End Using

            Catch
                Return 0
            End Try

            Return 0

        End Function


        Private Shared Sub MigrateApplicationDataIfNeeded(
            currentRoot As String,
            legacyRoot As String,
            currentManagedLibraryRoot As String,
            legacyManagedLibraryRoot As String
        )

            If HasCurrentApplicationData(currentRoot) Then
                Return
            End If

            If Not Directory.Exists(legacyRoot) Then
                Directory.CreateDirectory(currentRoot)
                Return
            End If

            If Directory.Exists(currentRoot) AndAlso IsDirectoryEmpty(currentRoot) Then
                Directory.Delete(currentRoot, True)
            End If

            If Directory.Exists(currentRoot) Then
                Return
            End If

            Dim stagingRoot As String =
                currentRoot & ".migration-" & Guid.NewGuid().ToString("N")

            Try

                CopyDirectory(legacyRoot, stagingRoot)

                RewriteManagedLibraryPathsInCopiedData(
                    stagingRoot,
                    legacyManagedLibraryRoot,
                    currentManagedLibraryRoot
                )

                ValidateCopiedManuscriptData(stagingRoot)

                Directory.Move(stagingRoot, currentRoot)

                WriteMigrationReceipt(
                    currentRoot,
                    legacyRoot,
                    legacyManagedLibraryRoot,
                    currentManagedLibraryRoot
                )

            Catch ex As Exception

                DeleteDirectoryBestEffort(stagingRoot)

                Throw New InvalidOperationException(
                    "PaperRoute could not safely migrate the legacy ManuscriptPipeline data folder. " &
                    "The original data was left unchanged." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    ex
                )

            End Try

        End Sub


        Private Shared Sub MigrateManagedLibraryIfNeeded(
            currentRoot As String,
            legacyRoot As String
        )

            If Directory.Exists(currentRoot) AndAlso Not IsDirectoryEmpty(currentRoot) Then
                Return
            End If

            If Not Directory.Exists(legacyRoot) Then
                Return
            End If

            If Directory.Exists(currentRoot) AndAlso IsDirectoryEmpty(currentRoot) Then
                Directory.Delete(currentRoot, True)
            End If

            Dim stagingRoot As String =
                currentRoot & ".migration-" & Guid.NewGuid().ToString("N")

            Try

                CopyDirectory(legacyRoot, stagingRoot)
                Directory.Move(stagingRoot, currentRoot)

            Catch ex As Exception

                DeleteDirectoryBestEffort(stagingRoot)

                Throw New InvalidOperationException(
                    "PaperRoute could not safely migrate the managed manuscript library. " &
                    "The original library was left unchanged." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    ex
                )

            End Try

        End Sub


        Private Shared Sub EnsureSchemaVersion(
            currentRoot As String
        )

            Dim dataDirectory As String = Path.Combine(currentRoot, "data")
            Dim schemaPath As String = SchemaFilePath(currentRoot)

            Directory.CreateDirectory(dataDirectory)

            Dim existingVersion As Integer = ReadSchemaVersion(schemaPath)

            If existingVersion > CurrentSchemaVersion Then

                Throw New InvalidOperationException(
                    "This PaperRoute library was created by a newer storage schema (" &
                    existingVersion.ToString() & "). Update PaperRoute before opening it."
                )

            End If

            If existingVersion = CurrentSchemaVersion Then
                Return
            End If

            Dim payload As New Dictionary(Of String, Object) From {
                {"SchemaVersion", CurrentSchemaVersion},
                {"UpdatedAtUtc", DateTime.UtcNow.ToString("O")}
            }

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True
            }

            File.WriteAllText(
                schemaPath,
                JsonSerializer.Serialize(payload, options)
            )

        End Sub


        Private Shared Function HasCurrentApplicationData(
            root As String
        ) As Boolean

            If Not Directory.Exists(root) Then
                Return False
            End If

            Return File.Exists(Path.Combine(root, "settings.json")) OrElse
                   File.Exists(Path.Combine(root, "data", "manuscripts.json")) OrElse
                   File.Exists(Path.Combine(root, "data", "schema.json"))

        End Function


        Private Shared Sub RewriteManagedLibraryPathsInCopiedData(
            copiedApplicationRoot As String,
            legacyManagedLibraryRoot As String,
            currentManagedLibraryRoot As String
        )

            Dim dataPath As String =
                Path.Combine(copiedApplicationRoot, "data", "manuscripts.json")

            If Not File.Exists(dataPath) Then
                Return
            End If

            Dim json As String = File.ReadAllText(dataPath)

            If String.IsNullOrWhiteSpace(json) Then
                Return
            End If

            Dim options As JsonSerializerOptions = CreateManuscriptJsonOptions()

            Dim manuscripts As List(Of Manuscript) =
                JsonSerializer.Deserialize(Of List(Of Manuscript))(json, options)

            If manuscripts Is Nothing Then
                Throw New InvalidDataException("The copied manuscript data could not be read.")
            End If

            Dim legacyRoot As String =
                NormalizeDirectoryPrefix(legacyManagedLibraryRoot)

            Dim currentRoot As String =
                NormalizeDirectoryPrefix(currentManagedLibraryRoot)

            Dim changed As Boolean = False

            For Each manuscript As Manuscript In manuscripts

                If manuscript.Submissions Is Nothing Then
                    Continue For
                End If

                For Each submission As JournalSubmission In manuscript.Submissions

                    If submission.Correspondence Is Nothing Then
                        Continue For
                    End If

                    For Each item As CorrespondenceItem In submission.Correspondence

                        If String.IsNullOrWhiteSpace(item.LocalFilePath) Then
                            Continue For
                        End If

                        Dim fullPath As String

                        Try
                            fullPath = Path.GetFullPath(item.LocalFilePath)
                        Catch
                            Continue For
                        End Try

                        If fullPath.StartsWith(legacyRoot, StringComparison.OrdinalIgnoreCase) Then

                            Dim relativePath As String = fullPath.Substring(legacyRoot.Length)

                            item.LocalFilePath =
                                Path.Combine(currentRoot, relativePath)

                            changed = True

                        End If

                    Next

                Next

            Next

            If changed Then

                File.WriteAllText(
                    dataPath,
                    JsonSerializer.Serialize(manuscripts, options)
                )

            End If

        End Sub


        Private Shared Sub ValidateCopiedManuscriptData(
            copiedApplicationRoot As String
        )

            Dim dataPath As String =
                Path.Combine(copiedApplicationRoot, "data", "manuscripts.json")

            If Not File.Exists(dataPath) Then
                Return
            End If

            Dim json As String = File.ReadAllText(dataPath)

            If String.IsNullOrWhiteSpace(json) Then
                Return
            End If

            Dim manuscripts As List(Of Manuscript) =
                JsonSerializer.Deserialize(Of List(Of Manuscript))(
                    json,
                    CreateManuscriptJsonOptions()
                )

            If manuscripts Is Nothing Then
                Throw New InvalidDataException("The copied manuscript data could not be validated.")
            End If

        End Sub


        Private Shared Function CreateManuscriptJsonOptions() As JsonSerializerOptions

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True,
                .IgnoreReadOnlyProperties = True,
                .PropertyNameCaseInsensitive = True
            }

            options.Converters.Add(New JsonStringEnumConverter())

            Return options

        End Function


        Private Shared Sub CopyDirectory(
            sourceDirectory As String,
            destinationDirectory As String
        )

            Directory.CreateDirectory(destinationDirectory)

            For Each sourceFile As String In Directory.EnumerateFiles(sourceDirectory)

                Dim destinationFile As String =
                    Path.Combine(destinationDirectory, Path.GetFileName(sourceFile))

                File.Copy(sourceFile, destinationFile, False)

            Next

            For Each sourceSubdirectory As String In Directory.EnumerateDirectories(sourceDirectory)

                Dim destinationSubdirectory As String =
                    Path.Combine(destinationDirectory, Path.GetFileName(sourceSubdirectory))

                CopyDirectory(sourceSubdirectory, destinationSubdirectory)

            Next

        End Sub


        Private Shared Function IsDirectoryEmpty(
            directoryPath As String
        ) As Boolean

            If Not Directory.Exists(directoryPath) Then
                Return True
            End If

            Return Not Directory.EnumerateFileSystemEntries(directoryPath).Any()

        End Function


        Private Shared Function NormalizeDirectoryPrefix(
            directoryPath As String
        ) As String

            Dim fullPath As String = Path.GetFullPath(directoryPath)

            Return fullPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            ) & Path.DirectorySeparatorChar

        End Function


        Private Shared Sub DeleteDirectoryBestEffort(
            directoryPath As String
        )

            Try
                If Directory.Exists(directoryPath) Then
                    Directory.Delete(directoryPath, True)
                End If
            Catch
                ' The source data is never deleted. Cleanup is best-effort.
            End Try

        End Sub


        Private Shared Sub WriteMigrationReceipt(
            currentRoot As String,
            legacyRoot As String,
            legacyLibraryRoot As String,
            currentLibraryRoot As String
        )

            Try

                Dim receiptPath As String =
                    Path.Combine(currentRoot, "migration.json")

                Dim payload As New Dictionary(Of String, Object) From {
                    {"MigratedAtUtc", DateTime.UtcNow.ToString("O")},
                    {"SourceDataRoot", legacyRoot},
                    {"DestinationDataRoot", currentRoot},
                    {"SourceManagedLibrary", legacyLibraryRoot},
                    {"DestinationManagedLibrary", currentLibraryRoot},
                    {"LegacyDataPreserved", True}
                }

                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }

                File.WriteAllText(
                    receiptPath,
                    JsonSerializer.Serialize(payload, options)
                )

            Catch
                ' The migration receipt is useful diagnostics, not a reason
                ' to fail an otherwise successful migration.
            End Try

        End Sub


        Private Shared Sub ValidateRoot(
            rootPath As String,
            parameterName As String
        )

            If String.IsNullOrWhiteSpace(rootPath) Then
                Throw New ArgumentException("A storage root path is required.", parameterName)
            End If

        End Sub

    End Class

End Namespace
