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

        Dim repository As New ManuscriptRepository(_dataDirectory, _managedLibrary)
        Dim expected As List(Of Manuscript) = CreateRepresentativeLibrary()

        repository.Save(expected)

        Dim actual As List(Of Manuscript) = repository.Load()

        Assert.AreEqual(3, actual.Count)
        Assert.AreEqual("Active Study", actual(0).Title)
        Assert.AreEqual(PaperStage.UnderReview, actual(0).CurrentStage)
        Assert.AreEqual(1, actual(0).Submissions.Count)
        Assert.AreEqual(1, actual(0).Submissions(0).Decisions.Count)
        Assert.AreEqual(EditorialDecision.MajorRevision, actual(0).Submissions(0).Decisions(0).Decision)
        Assert.AreEqual(ManuscriptLocation.Published, actual(1).Location)
        Assert.AreEqual(ManuscriptLocation.FileDrawer, actual(2).Location)
        Assert.AreEqual(1, actual(2).RejectionCount)

    End Sub


    <TestMethod>
    Public Sub SecondSave_CreatesBackupOfPreviousData()

        Dim repository As New ManuscriptRepository(_dataDirectory, _managedLibrary)
        Dim manuscripts As List(Of Manuscript) = CreateRepresentativeLibrary()

        repository.Save(manuscripts)

        manuscripts(0).Title = "Active Study Revised"
        repository.Save(manuscripts)

        Assert.IsTrue(File.Exists(repository.BackupFilePath))

        Dim backupJson As String = File.ReadAllText(repository.BackupFilePath)
        Assert.IsTrue(backupJson.Contains("Active Study"))
        Assert.IsFalse(backupJson.Contains("Active Study Revised"))

    End Sub


    <TestMethod>
    Public Sub Load_NormalizesPublishedStageLocation()

        Directory.CreateDirectory(_dataDirectory)

        Dim manuscript As New Manuscript With {
            .Title = "Legacy Published Location",
            .CurrentStage = PaperStage.Published,
            .Location = ManuscriptLocation.Pipeline
        }

        Dim json As String =
            JsonSerializer.Serialize(
                New List(Of Manuscript) From {manuscript},
                CreateJsonOptions()
            )

        File.WriteAllText(Path.Combine(_dataDirectory, "manuscripts.json"), json)

        Dim repository As New ManuscriptRepository(_dataDirectory, _managedLibrary)
        Dim loaded As List(Of Manuscript) = repository.Load()

        Assert.AreEqual(ManuscriptLocation.Published, loaded(0).Location)

    End Sub


    <TestMethod>
    Public Sub Load_NormalizesNullCollections()

        Directory.CreateDirectory(_dataDirectory)

        Dim id As Guid = Guid.NewGuid()
        Dim json As String =
            "[{" &
            """Id"":""" & id.ToString() & """," &
            """Title"":""Null Collections""," &
            """CurrentStage"":""Draft""," &
            """Location"":""Pipeline""," &
            """History"":null," &
            """Submissions"":null" &
            "}]"

        File.WriteAllText(Path.Combine(_dataDirectory, "manuscripts.json"), json)

        Dim repository As New ManuscriptRepository(_dataDirectory, _managedLibrary)
        Dim loaded As List(Of Manuscript) = repository.Load()

        Assert.IsNotNull(loaded(0).History)
        Assert.IsNotNull(loaded(0).Submissions)
        Assert.AreEqual(0, loaded(0).History.Count)
        Assert.AreEqual(0, loaded(0).Submissions.Count)

    End Sub

End Class
