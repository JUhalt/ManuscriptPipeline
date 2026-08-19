Namespace Services

    Public NotInheritable Class ProductInfo

        Private Sub New()
        End Sub

        Public Const DisplayName As String = "PaperRoute"
        Public Const ProductName As String = "PaperRoute Tracker"
        Public Const Tagline As String = "Track • Submit • Publish"
        Public Const Description As String = "Local-first academic manuscript tracking for researchers."

        ' Current PaperRoute storage names.
        Public Const DataFolderName As String = "PaperRoute"
        Public Const ManagedLibraryFolderName As String = "PaperRoute Library"

        ' Legacy names are retained so the one-time migration can copy from
        ' pre-rebrand builds without deleting the user's rollback copy.
        Public Const LegacyDataFolderName As String = "ManuscriptPipeline"
        Public Const LegacyManagedLibraryFolderName As String = "ManuscriptPipeline Library"

    End Class

End Namespace
