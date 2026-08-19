Namespace Services

    Public NotInheritable Class ProductInfo

        Private Sub New()
        End Sub

        Public Const DisplayName As String = "PaperRoute"
        Public Const ProductName As String = "PaperRoute Tracker"
        Public Const Tagline As String = "Track • Submit • Publish"
        Public Const Description As String = "Local-first academic manuscript tracking for researchers."

        ' Keep these legacy storage names stable for v0.1 so existing users
        ' keep seeing the same local data and managed files after the rebrand.
        Public Const LegacyDataFolderName As String = "ManuscriptPipeline"
        Public Const LegacyManagedLibraryFolderName As String = "ManuscriptPipeline Library"

    End Class

End Namespace
