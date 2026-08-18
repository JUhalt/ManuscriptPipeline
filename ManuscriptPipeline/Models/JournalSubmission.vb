Imports System
Imports System.Collections.Generic

Namespace Models

    Public Class JournalSubmission

        Public Property Id As Guid = Guid.NewGuid()

        Public Property JournalName As String = String.Empty

        Public Property ManuscriptNumber As String = String.Empty

        Public Property SubmittedDate As DateTime = DateTime.Now

        Public Property Notes As String = String.Empty

        Public Property Decisions As List(Of EditorialDecisionEvent) =
            New List(Of EditorialDecisionEvent)()

    End Class

End Namespace