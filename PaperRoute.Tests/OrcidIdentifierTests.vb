Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Services

<TestClass>
Public Class OrcidIdentifierTests

    <TestMethod>
    Public Sub Normalize_OrcidUrl_ReturnsCanonicalId()

        Assert.AreEqual(
            "0000-0002-1825-0097",
            OrcidIdentifierService.Normalize(
                "https://orcid.org/0000-0002-1825-0097"
            )
        )

    End Sub


    <TestMethod>
    Public Sub Normalize_OrcidPrefix_ReturnsCanonicalId()

        Assert.AreEqual(
            "0000-0001-5109-3700",
            OrcidIdentifierService.Normalize(
                "ORCID iD: 0000-0001-5109-3700"
            )
        )

    End Sub


    <TestMethod>
    Public Sub IsValid_KnownValidId_PassesChecksum()

        Assert.IsTrue(
            OrcidIdentifierService.IsValid(
                "0000-0002-1825-0097"
            )
        )

    End Sub


    <TestMethod>
    Public Sub IsValid_WrongChecksum_IsRejected()

        Assert.IsFalse(
            OrcidIdentifierService.IsValid(
                "0000-0002-1825-0098"
            )
        )

    End Sub

End Class
