Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class PortableBackupService

        Public Sub CreateBackup(
            destinationZipPath As String,
            manuscripts As List(Of Manuscript),
            repository As ManuscriptRepository
        )

            If String.IsNullOrWhiteSpace(destinationZipPath) Then
                Throw New ArgumentException("A backup destination is required.")
            End If

            If manuscripts Is Nothing Then
                Throw New ArgumentNullException(NameOf(manuscripts))
            End If

            If repository Is Nothing Then
                Throw New ArgumentNullException(NameOf(repository))
            End If

            If Not File.Exists(repository.DataFilePath) Then
                Throw New FileNotFoundException(
                    "The PaperRoute data file could not be found.",
                    repository.DataFilePath
                )
            End If

            Dim stagingDirectory As String =
                Path.Combine(
                    Path.GetTempPath(),
                    "PaperRouteBackup_" & Guid.NewGuid().ToString("N")
                )

            Try

                Directory.CreateDirectory(stagingDirectory)

                ' =============================================
                ' Native PaperRoute data
                ' =============================================

                Dim jsonDestination As String =
                    Path.Combine(
                        stagingDirectory,
                        "manuscripts.json"
                    )

                File.Copy(
                    repository.DataFilePath,
                    jsonDestination,
                    True
                )

                ' =============================================
                ' Human-readable Excel export
                ' =============================================

                Dim excelDestination As String =
                    Path.Combine(
                        stagingDirectory,
                        "library.xlsx"
                    )

                Dim excelExporter As New LibraryExcelExporter()

                excelExporter.Export(
                    excelDestination,
                    manuscripts
                )

                ' =============================================
                ' Managed document library
                ' =============================================

                Dim managedLibrary As New ManagedLibraryService()

                If Directory.Exists(managedLibrary.RootDirectory) Then

                    Dim filesDestination As String =
                        Path.Combine(
                            stagingDirectory,
                            "files"
                        )

                    CopyDirectory(
                        managedLibrary.RootDirectory,
                        filesDestination
                    )

                End If

                ' =============================================
                ' Backup information
                ' =============================================

                Dim submissionCount As Integer = 0
                Dim decisionCount As Integer = 0
                Dim correspondenceCount As Integer = 0

                For Each manuscript As Manuscript In manuscripts

                    submissionCount += manuscript.Submissions.Count

                    For Each submission As JournalSubmission In manuscript.Submissions

                        decisionCount += submission.Decisions.Count
                        correspondenceCount += submission.Correspondence.Count

                    Next

                Next

                Dim backupInfo As String =
                    "PaperRoute Portable Backup" &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Created: " &
                    DateTime.Now.ToString("O") &
                    Environment.NewLine &
                    "Manuscripts: " &
                    manuscripts.Count.ToString() &
                    Environment.NewLine &
                    "Submissions: " &
                    submissionCount.ToString() &
                    Environment.NewLine &
                    "Editorial decisions: " &
                    decisionCount.ToString() &
                    Environment.NewLine &
                    "Correspondence records: " &
                    correspondenceCount.ToString() &
                    Environment.NewLine &
                    Environment.NewLine &
                    "manuscripts.json is the native PaperRoute data file." &
                    Environment.NewLine &
                    "library.xlsx is a human-readable export of the library." &
                    Environment.NewLine &
                    "files contains documents managed by PaperRoute." &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Externally linked files are referenced by path but are not copied into this backup."

                File.WriteAllText(
                    Path.Combine(
                        stagingDirectory,
                        "backup-info.txt"
                    ),
                    backupInfo
                )

                ' =============================================
                ' ZIP
                ' =============================================

                If File.Exists(destinationZipPath) Then
                    File.Delete(destinationZipPath)
                End If

                ZipFile.CreateFromDirectory(
                    stagingDirectory,
                    destinationZipPath,
                    CompressionLevel.Optimal,
                    False
                )

            Finally

                If Directory.Exists(stagingDirectory) Then

                    Try

                        Directory.Delete(
                            stagingDirectory,
                            True
                        )

                    Catch
                        ' Temporary cleanup is best-effort.
                    End Try

                End If

            End Try

        End Sub


        Private Sub CopyDirectory(
            sourceDirectory As String,
            destinationDirectory As String
        )

            Directory.CreateDirectory(destinationDirectory)

            For Each sourceFile As String In Directory.GetFiles(sourceDirectory)

                Dim destinationFile As String =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceFile)
                    )

                File.Copy(
                    sourceFile,
                    destinationFile,
                    True
                )

            Next

            For Each sourceSubdirectory As String In Directory.GetDirectories(sourceDirectory)

                Dim destinationSubdirectory As String =
                    Path.Combine(
                        destinationDirectory,
                        Path.GetFileName(sourceSubdirectory)
                    )

                CopyDirectory(
                    sourceSubdirectory,
                    destinationSubdirectory
                )

            Next

        End Sub

    End Class

End Namespace