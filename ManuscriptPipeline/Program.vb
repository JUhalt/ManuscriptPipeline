Imports System
Imports Velopack
Imports ManuscriptPipeline.Services

Friend Module Program

    <STAThread>
    Public Sub Main(
        args As String()
    )

        ' Velopack must be the first application startup work performed.
        ' In a normal F5/developer launch this simply returns and the
        ' existing VB application framework continues as usual.
        VelopackApp.Build().Run()

        ' Safely copy any pre-rebrand ManuscriptPipeline storage into
        ' PaperRoute storage before settings or repositories are constructed.
        StorageMigrationService.EnsureCurrentStorage()

        Dim application As New My.MyApplication()

        application.Run(
            args
        )

    End Sub

End Module
