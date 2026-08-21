Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ManuscriptPipeline.Services

<TestClass>
Public Class DoiNormalizerTests

    <TestMethod>
    Public Sub Normalize_AcceptsDoiPrefix()

        Assert.AreEqual(
            "10.1234/example.1",
            DoiNormalizer.Normalize(
                "doi: 10.1234/example.1"
            )
        )

    End Sub


    <TestMethod>
    Public Sub Normalize_AcceptsDoiOrgUrl()

        Assert.AreEqual(
            "10.1234/example.2",
            DoiNormalizer.Normalize(
                "https://doi.org/10.1234/example.2"
            )
        )

    End Sub


    <TestMethod>
    Public Sub Normalize_RemovesTrailingCitationPunctuation()

        Assert.AreEqual(
            "10.1234/example.3",
            DoiNormalizer.Normalize(
                "10.1234/example.3."
            )
        )

    End Sub


    <TestMethod>
    Public Sub IsValid_RejectsNonDoi()

        Assert.IsFalse(
            DoiNormalizer.IsValid(
                "not-a-doi"
            )
        )

    End Sub

End Class
