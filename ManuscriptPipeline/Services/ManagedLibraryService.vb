'Imports System
Imports System.Collections.Generic
Imports System.IO
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class ManagedLibraryService

        Private ReadOnly _rootDirectory As String


        Public Sub New()

            Dim documentsDirectory As String =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments
                )

            If String.IsNullOrWhiteSpace(documentsDirectory) Then

                Throw New InvalidOperationException(
                    "The user's Documents folder could not be located."
                )

            End If

            _rootDirectory =
                Path.Combine(
                    documentsDirectory,
                    ProductInfo.LegacyManagedLibraryFolderName
                )

        End Sub


        Public ReadOnly Property RootDirectory As String

            Get
                Return _rootDirectory
            End Get

        End Property


        Public Function IsManagedPath(
            filePath As String
        ) As Boolean

            If String.IsNullOrWhiteSpace(filePath) Then
                Return False
            End If

            Try

                Dim fullRoot As String =
                    Path.GetFullPath(_rootDirectory)

                fullRoot =
                    fullRoot.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    ) &
                    Path.DirectorySeparatorChar

                Dim fullPath As String =
                    Path.GetFullPath(filePath)

                Return fullPath.StartsWith(
                    fullRoot,
                    StringComparison.OrdinalIgnoreCase
                )

            Catch

                Return False

            End Try

        End Function


        Public Sub CommitManagedCopies(
            manuscripts As IEnumerable(Of Manuscript)
        )

            Dim operations As New List(Of CopyOperation)()

            ' =================================================
            ' Build copy plan first.
            ' Nothing is changed yet.
            ' =================================================

            For Each manuscript As Manuscript In manuscripts

                For Each submission As JournalSubmission In manuscript.Submissions

                    For Each item As CorrespondenceItem In submission.Correspondence

                        If Not item.IsManagedCopy Then
                            Continue For
                        End If

                        If String.IsNullOrWhiteSpace(item.LocalFilePath) Then
                            Continue For
                        End If

                        If IsManagedPath(item.LocalFilePath) Then
                            Continue For
                        End If

                        If Not File.Exists(item.LocalFilePath) Then

                            Throw New FileNotFoundException(
                                "A file marked for the PaperRoute Library could not be found.",
                                item.LocalFilePath
                            )

                        End If

                        Dim destinationDirectory As String =
                            Path.Combine(
                                _rootDirectory,
                                manuscript.Id.ToString("N"),
                                submission.Id.ToString("N"),
                                item.Id.ToString("N")
                            )

                        Dim sourceName As String =
                            Path.GetFileNameWithoutExtension(
                                item.LocalFilePath
                            )

                        Dim extension As String =
                            Path.GetExtension(
                                item.LocalFilePath
                            )

                        If String.IsNullOrWhiteSpace(sourceName) Then
                            sourceName = "document"
                        End If

                        Dim uniqueSuffix As String =
                            Guid.NewGuid().ToString("N").Substring(0, 8)

                        Dim destinationFileName As String =
                            sourceName &
                            "_" &
                            uniqueSuffix &
                            extension

                        Dim destinationPath As String =
                            Path.Combine(
                                destinationDirectory,
                                destinationFileName
                            )

                        operations.Add(
                            New CopyOperation(
                                item,
                                item.LocalFilePath,
                                destinationDirectory,
                                destinationPath
                            )
                        )

                    Next

                Next

            Next

            If operations.Count = 0 Then
                Return
            End If

            ' =================================================
            ' Copy files.
            ' Track anything created so we can clean up
            ' if one of the later copies fails.
            ' =================================================

            Dim createdFiles As New List(Of String)()

            Try

                For Each operation As CopyOperation In operations

                    Directory.CreateDirectory(
                        operation.DestinationDirectory
                    )

                    File.Copy(
                        operation.SourcePath,
                        operation.DestinationPath,
                        False
                    )

                    createdFiles.Add(
                        operation.DestinationPath
                    )

                Next

            Catch

                For Each createdFile As String In createdFiles

                    Try

                        If File.Exists(createdFile) Then
                            File.Delete(createdFile)
                        End If

                    Catch
                        ' Best-effort rollback only.
                    End Try

                Next

                Throw

            End Try

            ' =================================================
            ' Every copy succeeded.
            ' Only now update the in-memory records.
            ' =================================================

            For Each operation As CopyOperation In operations

                operation.Item.LocalFilePath =
                    operation.DestinationPath

                operation.Item.IsManagedCopy =
                    True

            Next

        End Sub


        Private Class CopyOperation

            Public ReadOnly Property Item As CorrespondenceItem
            Public ReadOnly Property SourcePath As String
            Public ReadOnly Property DestinationDirectory As String
            Public ReadOnly Property DestinationPath As String


            Public Sub New(
                item As CorrespondenceItem,
                sourcePath As String,
                destinationDirectory As String,
                destinationPath As String
            )

                Me.Item = item
                Me.SourcePath = sourcePath
                Me.DestinationDirectory = destinationDirectory
                Me.DestinationPath = destinationPath

            End Sub

        End Class

    End Class

End Namespace