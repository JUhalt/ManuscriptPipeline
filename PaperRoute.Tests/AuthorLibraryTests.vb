Imports System
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Models
Imports ManuscriptPipeline.Services

<TestClass>
Public Class AuthorLibraryTests

    Private _root As String = String.Empty
    Private _data As String = String.Empty


    <TestInitialize>
    Public Sub Initialize()

        _root =
            CreateTemporaryRoot()

        _data =
            Path.Combine(
                _root,
                "data"
            )

    End Sub


    <TestCleanup>
    Public Sub Cleanup()

        DeleteTemporaryRoot(
            _root
        )

    End Sub


    <TestMethod>
    Public Sub EmptyAuthorLibrary_LoadsAsEmpty()

        Dim repository As New AuthorLibraryRepository(
            _data
        )

        Dim library As AuthorLibraryData =
            repository.Load()

        Assert.AreEqual(
            0,
            library.Authors.Count
        )

        Assert.AreEqual(
            0,
            library.Affiliations.Count
        )

    End Sub


    <TestMethod>
    Public Sub SaveAndLoad_RoundTripsAuthorsAndAffiliations()

        Dim repository As New AuthorLibraryRepository(
            _data
        )

        Dim affiliation As New AffiliationRecord With {
            .Institution = "Example University",
            .Department = "Department of Psychology",
            .City = "Hartford",
            .Region = "CT",
            .Country = "USA"
        }

        Dim author As New AuthorRecord With {
            .GivenName = "Joshua",
            .FamilyName = "Uhalt",
            .Orcid = "0000-0000-0000-0000",
            .IsMe = True
        }

        Dim library As New AuthorLibraryData()

        library.Authors.Add(
            author
        )

        library.Affiliations.Add(
            affiliation
        )

        repository.Save(
            library
        )

        Dim loaded As AuthorLibraryData =
            repository.Load()

        Assert.AreEqual(
            1,
            loaded.Authors.Count
        )

        Assert.AreEqual(
            "Joshua Uhalt",
            loaded.Authors(0).DisplayName
        )

        Assert.IsTrue(
            loaded.Authors(0).IsMe
        )

        Assert.AreEqual(
            "0000-0000-0000-0000",
            loaded.Authors(0).Orcid
        )

        Assert.AreEqual(
            1,
            loaded.Affiliations.Count
        )

        Assert.AreEqual(
            "Example University",
            loaded.Affiliations(0).Institution
        )

    End Sub


    <TestMethod>
    Public Sub SecondSave_CreatesSafetyBackup()

        Dim repository As New AuthorLibraryRepository(
            _data
        )

        Dim library As New AuthorLibraryData()

        library.Authors.Add(
            New AuthorRecord With {
                .DisplayNameOverride = "First"
            }
        )

        repository.Save(
            library
        )

        library.Authors(0).DisplayNameOverride =
            "Second"

        repository.Save(
            library
        )

        Assert.IsTrue(
            File.Exists(
                repository.BackupFilePath
            )
        )

        Dim backupText As String =
            File.ReadAllText(
                repository.BackupFilePath
            )

        StringAssert.Contains(
            backupText,
            "First"
        )

    End Sub


    <TestMethod>
    Public Sub CorruptPrimary_WithValidBackup_RecoversAndPreservesCorruptFile()

        Dim repository As New AuthorLibraryRepository(
            _data
        )

        Dim library As New AuthorLibraryData()

        library.Authors.Add(
            New AuthorRecord With {
                .DisplayNameOverride = "Safe Author"
            }
        )

        repository.Save(
            library
        )

        repository.Save(
            library
        )

        File.WriteAllText(
            repository.DataFilePath,
            "{ deliberately broken json"
        )

        Dim loaded As AuthorLibraryData =
            repository.Load()

        Assert.IsTrue(
            repository.LastLoadRecoveredFromBackup
        )

        Assert.AreEqual(
            "Safe Author",
            loaded.Authors(0).DisplayName
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

    End Sub


    <TestMethod>
    Public Sub DuplicateAuthorIds_AreRejectedBeforeSave()

        Dim repository As New AuthorLibraryRepository(
            _data
        )

        Dim duplicateId As Guid =
            Guid.NewGuid()

        Dim library As New AuthorLibraryData()

        library.Authors.Add(
            New AuthorRecord With {
                .Id = duplicateId,
                .DisplayNameOverride = "One"
            }
        )

        library.Authors.Add(
            New AuthorRecord With {
                .Id = duplicateId,
                .DisplayNameOverride = "Two"
            }
        )

        Assert.ThrowsExactly(Of InvalidDataException)(
            Sub()
                repository.Save(
                    library
                )
            End Sub
        )

    End Sub

End Class
