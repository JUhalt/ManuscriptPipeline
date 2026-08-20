Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

<TestClass>
Public Class PersistenceTests

    Private _root As String = String.Empty
    Private _dataDirectory As String = String.Empty
    Private _managedLibrary As String = String.Empty


    <TestInitialize>
    Public Sub Initialize()

        _root = CreateTemporaryRoot()
        _dataDirectory = Path.Combine(_root, "data")
        _managedLibrary = Path.Combine(_root, "managed")

    End Sub


    <TestCleanup>
    Public Sub Cleanup()
        DeleteTemporaryRoot(_root)
    End Sub


    <TestMethod>
    Public Sub SaveAndLoad_RoundTripsRepresentativeLibrary()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim expected As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        repository.Save(expected)

        Dim actual As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(3, actual.Count)

        Assert.AreEqual(
            "Active Study",
            actual(0).Title
        )

        Assert.AreEqual(
            PaperStage.UnderReview,
            actual(0).CurrentStage
        )

        Assert.AreEqual(
            1,
            actual(0).Submissions.Count
        )

        Assert.AreEqual(
            1,
            actual(0).Submissions(0).Decisions.Count
        )

        Assert.AreEqual(
            EditorialDecision.MajorRevision,
            actual(0).Submissions(0).Decisions(0).Decision
        )

        Assert.AreEqual(
            ManuscriptLocation.Published,
            actual(1).Location
        )

        Assert.AreEqual(
            ManuscriptLocation.FileDrawer,
            actual(2).Location
        )

        Assert.AreEqual(
            1,
            actual(2).RejectionCount
        )

    End Sub


    <TestMethod>
    Public Sub SecondSave_CreatesBackupOfPreviousData()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim manuscripts As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        repository.Save(manuscripts)

        manuscripts(0).Title =
            "Active Study Revised"

        repository.Save(manuscripts)

        Assert.IsTrue(
            File.Exists(repository.BackupFilePath)
        )

        Dim backupJson As String =
            File.ReadAllText(repository.BackupFilePath)

        Assert.IsTrue(
            backupJson.Contains("Active Study")
        )

        Assert.IsFalse(
            backupJson.Contains("Active Study Revised")
        )

    End Sub


    <TestMethod>
    Public Sub Load_NormalizesPublishedStageLocation()

        Directory.CreateDirectory(
            _dataDirectory
        )

        Dim manuscript As New Manuscript With {
            .Title = "Legacy Published Location",
            .CurrentStage = PaperStage.Published,
            .Location = ManuscriptLocation.Pipeline
        }

        Dim json As String =
            JsonSerializer.Serialize(
                New List(Of Manuscript) From {
                    manuscript
                },
                CreateJsonOptions()
            )

        File.WriteAllText(
            Path.Combine(
                _dataDirectory,
                "manuscripts.json"
            ),
            json
        )

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            ManuscriptLocation.Published,
            loaded(0).Location
        )

    End Sub


    <TestMethod>
    Public Sub Load_NormalizesNullCollections()

        Directory.CreateDirectory(
            _dataDirectory
        )

        Dim id As Guid =
            Guid.NewGuid()

        Dim json As String =
            "[{" &
            """Id"":""" & id.ToString() & """," &
            """Title"":""Null Collections""," &
            """CurrentStage"":""Draft""," &
            """Location"":""Pipeline""," &
            """History"":null," &
            """Submissions"":null" &
            "}]"

        File.WriteAllText(
            Path.Combine(
                _dataDirectory,
                "manuscripts.json"
            ),
            json
        )

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.IsNotNull(
            loaded(0).History
        )

        Assert.IsNotNull(
            loaded(0).Submissions
        )

        Assert.AreEqual(
            0,
            loaded(0).History.Count
        )

        Assert.AreEqual(
            0,
            loaded(0).Submissions.Count
        )

    End Sub


    <TestMethod>
    Public Sub Load_CorruptPrimaryRecoversFromValidBackup()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim manuscripts As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        repository.Save(manuscripts)

        manuscripts(0).Title =
            "Second Version"

        repository.Save(manuscripts)

        File.WriteAllText(
            repository.DataFilePath,
            "{ definitely not valid json"
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            3,
            loaded.Count
        )

        Assert.AreEqual(
            "Active Study",
            loaded(0).Title
        )

        Assert.IsTrue(
            repository.LastLoadRecoveredFromBackup
        )

        Assert.IsFalse(
            String.IsNullOrWhiteSpace(
                repository.LastRecoveryPreservedFilePath
            )
        )

        Assert.IsTrue(
            File.Exists(
                repository.LastRecoveryPreservedFilePath
            )
        )

        Dim reloaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            "Active Study",
            reloaded(0).Title
        )

    End Sub


    <TestMethod>
    Public Sub Load_BlankPrimaryRecoversFromValidBackup()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim manuscripts As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        repository.Save(manuscripts)

        manuscripts(0).Title =
            "Second Version"

        repository.Save(manuscripts)

        File.WriteAllText(
            repository.DataFilePath,
            String.Empty
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            "Active Study",
            loaded(0).Title
        )

        Assert.IsTrue(
            repository.LastLoadRecoveredFromBackup
        )

    End Sub


    <TestMethod>
    Public Sub Load_MissingPrimaryRecoversFromValidBackup()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim manuscripts As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        repository.Save(manuscripts)

        manuscripts(0).Title =
            "Second Version"

        repository.Save(manuscripts)

        File.Delete(
            repository.DataFilePath
        )

        Assert.IsTrue(
            File.Exists(repository.BackupFilePath)
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            "Active Study",
            loaded(0).Title
        )

        Assert.IsTrue(
            repository.LastLoadRecoveredFromBackup
        )

        Assert.IsTrue(
            File.Exists(repository.DataFilePath)
        )

    End Sub


    <TestMethod>
    Public Sub Load_CorruptPrimaryAndBackupThrowsSafely()

        Directory.CreateDirectory(
            _dataDirectory
        )

        Dim dataPath As String =
            Path.Combine(
                _dataDirectory,
                "manuscripts.json"
            )

        Dim backupPath As String =
            Path.Combine(
                _dataDirectory,
                "manuscripts.bak"
            )

        Const corruptPrimary As String =
            "{ corrupt primary"

        Const corruptBackup As String =
            "{ corrupt backup"

        File.WriteAllText(
            dataPath,
            corruptPrimary
        )

        File.WriteAllText(
            backupPath,
            corruptBackup
        )

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Assert.ThrowsExactly(Of InvalidDataException)(
            Sub()
                Dim ignored As List(Of Manuscript) =
                    repository.Load()
            End Sub
        )

        Assert.AreEqual(
            corruptPrimary,
            File.ReadAllText(dataPath)
        )

        Assert.AreEqual(
            corruptBackup,
            File.ReadAllText(backupPath)
        )

    End Sub


    <TestMethod>
    Public Sub Load_ValidEmptyLibraryRemainsValid()

        Directory.CreateDirectory(
            _dataDirectory
        )

        File.WriteAllText(
            Path.Combine(
                _dataDirectory,
                "manuscripts.json"
            ),
            "[]"
        )

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            0,
            loaded.Count
        )

        Assert.IsFalse(
            repository.LastLoadRecoveredFromBackup
        )

    End Sub


    <TestMethod>
    Public Sub SaveAndLoad_RoundTripsCorrespondenceMetadata()

        Dim repository As New ManuscriptRepository(
            _dataDirectory,
            _managedLibrary
        )

        Dim manuscripts As List(Of Manuscript) =
            CreateRepresentativeLibrary()

        Dim linkedFilePath As String =
            Path.Combine(
                _root,
                "linked-decision-letter.txt"
            )

        File.WriteAllText(
            linkedFilePath,
            "Synthetic linked correspondence."
        )

        manuscripts(0).Submissions(0).Correspondence.Add(
            New CorrespondenceItem With {
                .ItemDate = New DateTime(2026, 8, 19),
                .Type = CorrespondenceType.DecisionLetter,
                .Title = "Round-trip decision letter",
                .LocalFilePath = linkedFilePath,
                .IsManagedCopy = False
            }
        )

        repository.Save(
            manuscripts
        )

        Dim loaded As List(Of Manuscript) =
            repository.Load()

        Assert.AreEqual(
            1,
            loaded(0).Submissions(0).Correspondence.Count
        )

        Dim correspondence As CorrespondenceItem =
            loaded(0).Submissions(0).Correspondence(0)

        Assert.AreEqual(
            "Round-trip decision letter",
            correspondence.Title
        )

        Assert.AreEqual(
            CorrespondenceType.DecisionLetter,
            correspondence.Type
        )

        Assert.AreEqual(
            New DateTime(2026, 8, 19),
            correspondence.ItemDate
        )

        Assert.AreEqual(
            linkedFilePath,
            correspondence.LocalFilePath
        )

        Assert.IsFalse(
            correspondence.IsManagedCopy
        )

    End Sub

End Class