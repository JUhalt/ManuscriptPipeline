Imports System
Imports System.Collections.Generic

Namespace Models

    Public Class Manuscript

        Public Property Id As Guid = Guid.NewGuid()

        Public Property Title As String = String.Empty

        Public Property CoAuthors As String = String.Empty

        Public Property TargetJournal As String = String.Empty

        Public Property CurrentStage As PaperStage = PaperStage.Idea

        Public Property Location As ManuscriptLocation = ManuscriptLocation.Pipeline

        Public Property StageEnteredDate As DateTime = DateTime.Now

        Public Property RevisionDeadline As DateTime? = Nothing

        Public Property FiledDate As DateTime? = Nothing

        Public Property FiledReason As String = String.Empty

        Public Property History As List(Of HistoryEvent) =
            New List(Of HistoryEvent)()

    End Class

End Namespace