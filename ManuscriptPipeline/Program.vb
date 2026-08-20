Imports System
Imports System.Windows.Forms
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

        Try

            ' Validate and migrate PaperRoute storage before settings,
            ' repositories, or the main application are constructed.
            StorageMigrationService.EnsureCurrentStorage()

        Catch ex As Exception

            MessageBox.Show(
                "PaperRoute could not safely prepare its local storage." &
                Environment.NewLine &
                Environment.NewLine &
                "The storage format could not be verified, so PaperRoute will close rather than risk opening or rewriting data with an unknown format." &
                Environment.NewLine &
                Environment.NewLine &
                "Existing source data has been preserved wherever possible." &
                Environment.NewLine &
                Environment.NewLine &
                "Error details:" &
                Environment.NewLine &
                ex.Message,
                "PaperRoute Storage Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            )

            Return

        End Try

        Dim application As New My.MyApplication()

        application.Run(
            args
        )

    End Sub

End Module