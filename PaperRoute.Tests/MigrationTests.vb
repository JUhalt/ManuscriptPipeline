Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

<TestClass>
Public Class MigrationTests

    Private _root As String = String.Empty
    Private _legacyData As String = String.Empty
    Private _currentData As String = String.Empty
    Private _legacyLibrary As String = String.Empty
    Private _currentLibrary As String = String.Empty


    <TestInitialize>
    Public Sub Initialize()

        _root = CreateTemporaryRoot()
        _legacyData = Path.Combine(_root, "legacy-data")
        _currentData = Path.Combine(_root, "paperroute-data")
        _legacyLibrary = Path.Combine(_root, "legacy-library")
        _currentLibrary = Path.Combine(_root, "paperroute-library")

    End Sub


    <TestCleanup>
    Public Sub Cleanup()
        DeleteTemporaryRoot(_root)
    End Sub


    <TestMethod>
    Public Sub Migration_CopiesDataAndPreservesLegacySource()

        WriteLegacyLibrary()

        StorageMigrationService.EnsureCurrentStorage(
            _currentData,
            _legacyData,
            _currentLibrary,
            _legacyLibrary
        )

        Assert.IsTrue(File.Exists(Path.Combine(_currentData, "data", "manuscripts.json")))
        Assert.IsTrue(File.Exists(Path.Combine(_legacyData, "data", "manuscripts.json")))
        Assert.IsTrue(File.Exists(Path.Combine(_currentData, "migration.json")))
        Assert.AreEqual(
            StorageMigrationService.CurrentSchemaVersion,
            StorageMigrationService.ReadSchemaVersion(
                StorageMigrationService.SchemaFilePath(_currentData)
            )
        )

    End Sub


    <TestMethod>
    Public Sub Migration_CopiesManagedFilesAndRewritesCorrespondencePath()

        Dim legacyFile As String = WriteLegacyLibrary()

        StorageMigrationService.EnsureCurrentStorage(
            _currentData,
            _legacyData,
            _currentLibrary,
            _legacyLibrary
        )

        Assert.IsTrue(File.Exists(Path.Combine(_currentLibrary, "decision-letter.txt")))
        Assert.IsTrue(File.Exists(legacyFile))

        Dim repository As New ManuscriptRepository(
            Path.Combine(_currentData, "data"),
            _currentLibrary
        )

        Dim migrated As List(Of Manuscript) = repository.Load()
        Dim migratedPath As String = migrated(0).Submissions(0).Correspondence(0).LocalFilePath

        Assert.IsTrue(
            Path.GetFullPath(migratedPath).StartsWith(
                Path.GetFullPath(_currentLibrary),
                StringComparison.OrdinalIgnoreCase
            )
        )

    End Sub


    <TestMethod>
    Public Sub Migration_InvalidLegacyJsonDoesNotDestroyLegacySource()

        Dim legacyDataDirectory As String = Path.Combine(_legacyData, "data")
        Directory.CreateDirectory(legacyDataDirectory)
        File.WriteAllText(Path.Combine(legacyDataDirectory, "manuscripts.json"), "{ definitely not json")

        Dim threw As Boolean = False

        Try

            StorageMigrationService.EnsureCurrentStorage(
                _currentData,
                _legacyData,
                _currentLibrary,
                _legacyLibrary
            )

        Catch ex As InvalidOperationException
            threw = True
        End Try

        Assert.IsTrue(threw, "An invalid legacy library should fail migration.")
        Assert.IsTrue(File.Exists(Path.Combine(legacyDataDirectory, "manuscripts.json")))
        Assert.IsFalse(File.Exists(Path.Combine(_currentData, "data", "manuscripts.json")))

    End Sub


    <TestMethod>
    Public Sub Migration_RejectsFutureSchema()

        Dim dataDirectory As String = Path.Combine(_currentData, "data")
        Directory.CreateDirectory(dataDirectory)
        File.WriteAllText(
            Path.Combine(dataDirectory, "schema.json"),
            "{""SchemaVersion"":999}"
        )

        Dim threw As Boolean = False

        Try

            StorageMigrationService.EnsureCurrentStorage(
                _currentData,
                _legacyData,
                _currentLibrary,
                _legacyLibrary
            )

        Catch ex As InvalidOperationException
            threw = True
        End Try

        Assert.IsTrue(threw, "A newer storage schema must be rejected.")

    End Sub


    Private Function WriteLegacyLibrary() As String

        Dim legacyDataDirectory As String = Path.Combine(_legacyData, "data")
        Directory.CreateDirectory(legacyDataDirectory)
        Directory.CreateDirectory(_legacyLibrary)

        Dim legacyFile As String = Path.Combine(_legacyLibrary, "decision-letter.txt")
        File.WriteAllText(legacyFile, "Synthetic reviewer/editor correspondence fixture.")

        Dim manuscripts As List(Of Manuscript) = CreateRepresentativeLibrary()

        manuscripts(0).Submissions(0).Correspondence.Add(
            New CorrespondenceItem With {
                .ItemDate = New DateTime(2026, 7, 10),
                .Type = CorrespondenceType.DecisionLetter,
                .Title = "Synthetic decision letter",
                .LocalFilePath = legacyFile,
                .IsManagedCopy = True
            }
        )

        File.WriteAllText(
            Path.Combine(legacyDataDirectory, "manuscripts.json"),
            JsonSerializer.Serialize(manuscripts, CreateJsonOptions())
        )

        Return legacyFile

    End Function

End Class
