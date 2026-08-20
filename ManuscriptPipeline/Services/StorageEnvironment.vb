Imports System
Imports System.Diagnostics

Namespace Services

    Public NotInheritable Class StorageEnvironment

        Private Const DevelopmentOverrideVariable As String =
            "PAPERROUTE_DEV_STORAGE"


        Private Sub New()
        End Sub


        Public Shared Function IsDevelopmentProfile() As Boolean

            Return ShouldUseDevelopmentProfile(
                Debugger.IsAttached,
                Environment.GetEnvironmentVariable(
                    DevelopmentOverrideVariable
                )
            )

        End Function


        Friend Shared Function ShouldUseDevelopmentProfile(
            debuggerAttached As Boolean,
            overrideValue As String
        ) As Boolean

            If debuggerAttached Then
                Return True
            End If

            If String.IsNullOrWhiteSpace(overrideValue) Then
                Return False
            End If

            Select Case overrideValue.Trim().ToLowerInvariant()

                Case "1",
                     "true",
                     "yes",
                     "on",
                     "dev",
                     "development"

                    Return True

                Case Else
                    Return False

            End Select

        End Function


        Public Shared Function DataFolderName() As String

            If IsDevelopmentProfile() Then
                Return ProductInfo.DevelopmentDataFolderName
            End If

            Return ProductInfo.DataFolderName

        End Function


        Public Shared Function ManagedLibraryFolderName() As String

            If IsDevelopmentProfile() Then
                Return ProductInfo.DevelopmentManagedLibraryFolderName
            End If

            Return ProductInfo.ManagedLibraryFolderName

        End Function


        Public Shared Function LegacyDataFolderName() As String

            If IsDevelopmentProfile() Then
                Return ProductInfo.DevelopmentLegacyDataFolderName
            End If

            Return ProductInfo.LegacyDataFolderName

        End Function


        Public Shared Function LegacyManagedLibraryFolderName() As String

            If IsDevelopmentProfile() Then
                Return ProductInfo.DevelopmentLegacyManagedLibraryFolderName
            End If

            Return ProductInfo.LegacyManagedLibraryFolderName

        End Function


        Public Shared Function ProfileDisplayName() As String

            If IsDevelopmentProfile() Then
                Return "Development (isolated)"
            End If

            Return "Standard"

        End Function

    End Class

End Namespace
