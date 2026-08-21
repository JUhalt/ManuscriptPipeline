Imports System.Collections.Generic

Namespace Models

    Public Class AuthorLibraryData

        Public Property Authors As List(Of AuthorRecord) =
            New List(Of AuthorRecord)()

        Public Property Affiliations As List(Of AffiliationRecord) =
            New List(Of AffiliationRecord)()

    End Class

End Namespace
