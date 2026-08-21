Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports ManuscriptPipeline.Models

Namespace Services

    Public Class CrossrefClient

        Private Shared ReadOnly SharedHttpClient As HttpClient =
            CreateHttpClient()


        Private Shared Function CreateHttpClient() As HttpClient

            Dim client As New HttpClient()

            client.Timeout =
                TimeSpan.FromSeconds(20)

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "PaperRouteTracker/0.2 (+https://github.com/JUhalt/PaperRoute-Tracker)"
            )

            Return client

        End Function


        Public Async Function LookupAsync(
            doi As String,
            Optional cancellationToken As CancellationToken = Nothing
        ) As Task(Of CrossrefMetadataSuggestion)

            Dim normalizedDoi As String =
                DoiNormalizer.Normalize(
                    doi
                )

            If Not DoiNormalizer.IsValid(
                normalizedDoi
            ) Then

                Throw New ArgumentException(
                    "Please enter a valid DOI.",
                    NameOf(doi)
                )

            End If

            Dim requestUri As String =
                "https://api.crossref.org/works/" &
                Uri.EscapeDataString(
                    normalizedDoi
                )

            Using response As HttpResponseMessage =
                Await SharedHttpClient.GetAsync(
                    requestUri,
                    cancellationToken
                )

                If response.StatusCode =
                   HttpStatusCode.NotFound Then

                    Throw New InvalidOperationException(
                        "Crossref did not find a work for that DOI."
                    )

                End If

                If CInt(response.StatusCode) = 429 Then

                    Throw New InvalidOperationException(
                        "Crossref is temporarily rate-limiting requests. Please wait a moment and try again."
                    )

                End If

                If Not response.IsSuccessStatusCode Then

                    Throw New InvalidOperationException(
                        "Crossref returned HTTP " &
                        CInt(response.StatusCode).ToString() &
                        ". Please try again later."
                    )

                End If

                Dim json As String =
                    Await response.Content.ReadAsStringAsync(
                        cancellationToken
                    )

                Return ParseWorkJson(
                    json
                )

            End Using

        End Function


        Friend Shared Function ParseWorkJson(
            json As String
        ) As CrossrefMetadataSuggestion

            If String.IsNullOrWhiteSpace(json) Then

                Throw New InvalidOperationException(
                    "Crossref returned an empty response."
                )

            End If

            Using document As JsonDocument =
                JsonDocument.Parse(
                    json
                )

                Dim root As JsonElement =
                    document.RootElement

                Dim message As JsonElement

                If Not root.TryGetProperty(
                    "message",
                    message
                ) OrElse
                   message.ValueKind <>
                    JsonValueKind.Object Then

                    Throw New InvalidOperationException(
                        "Crossref returned an unexpected response."
                    )

                End If

                Dim suggestion As New CrossrefMetadataSuggestion With {
                    .Doi = ReadString(message, "DOI"),
                    .Title = ReadFirstString(message, "title"),
                    .Journal = ReadFirstString(message, "container-title"),
                    .Publisher = ReadString(message, "publisher"),
                    .Volume = ReadString(message, "volume"),
                    .Issue = ReadString(message, "issue"),
                    .Pages = ReadString(message, "page"),
                    .Url = ReadString(message, "URL"),
                    .AbstractText = CleanAbstract(ReadString(message, "abstract"))
                }

                suggestion.PublishedDate =
                    ReadPublishedDate(
                        message
                    )

                suggestion.Keywords =
                    ReadStringArray(
                        message,
                        "subject"
                    )

                suggestion.Authors =
                    ReadAuthors(
                        message
                    )

                If String.IsNullOrWhiteSpace(
                    suggestion.Doi
                ) Then

                    Throw New InvalidOperationException(
                        "Crossref found the work, but the response did not contain a DOI."
                    )

                End If

                Return suggestion

            End Using

        End Function


        Private Shared Function ReadString(
            parent As JsonElement,
            propertyName As String
        ) As String

            Dim element As JsonElement

            If Not parent.TryGetProperty(
                propertyName,
                element
            ) Then

                Return String.Empty

            End If

            If element.ValueKind =
               JsonValueKind.String Then

                Return If(
                    element.GetString(),
                    String.Empty
                ).Trim()

            End If

            Return String.Empty

        End Function


        Private Shared Function ReadFirstString(
            parent As JsonElement,
            propertyName As String
        ) As String

            Dim element As JsonElement

            If Not parent.TryGetProperty(
                propertyName,
                element
            ) OrElse
               element.ValueKind <>
                JsonValueKind.Array Then

                Return String.Empty

            End If

            For Each item As JsonElement In
                element.EnumerateArray()

                If item.ValueKind =
                   JsonValueKind.String Then

                    Return If(
                        item.GetString(),
                        String.Empty
                    ).Trim()

                End If

            Next

            Return String.Empty

        End Function


        Private Shared Function ReadStringArray(
            parent As JsonElement,
            propertyName As String
        ) As List(Of String)

            Dim values As New List(Of String)()
            Dim element As JsonElement

            If Not parent.TryGetProperty(
                propertyName,
                element
            ) OrElse
               element.ValueKind <>
                JsonValueKind.Array Then

                Return values

            End If

            For Each item As JsonElement In
                element.EnumerateArray()

                If item.ValueKind <>
                   JsonValueKind.String Then

                    Continue For

                End If

                Dim value As String =
                    If(
                        item.GetString(),
                        String.Empty
                    ).Trim()

                If Not String.IsNullOrWhiteSpace(
                    value
                ) AndAlso
                   Not values.Contains(
                    value,
                    StringComparer.OrdinalIgnoreCase
                ) Then

                    values.Add(
                        value
                    )

                End If

            Next

            Return values

        End Function


        Private Shared Function ReadPublishedDate(
            message As JsonElement
        ) As DateTime?

            Dim propertyNames As String() = {
                "published-print",
                "published-online",
                "published",
                "issued"
            }

            For Each propertyName As String In
                propertyNames

                Dim container As JsonElement

                If Not message.TryGetProperty(
                    propertyName,
                    container
                ) OrElse
                   container.ValueKind <>
                    JsonValueKind.Object Then

                    Continue For

                End If

                Dim dateParts As JsonElement

                If Not container.TryGetProperty(
                    "date-parts",
                    dateParts
                ) OrElse
                   dateParts.ValueKind <>
                    JsonValueKind.Array Then

                    Continue For

                End If

                For Each dateArray As JsonElement In
                    dateParts.EnumerateArray()

                    If dateArray.ValueKind <>
                       JsonValueKind.Array Then

                        Continue For

                    End If

                    Dim parts As New List(Of Integer)()

                    For Each part As JsonElement In
                        dateArray.EnumerateArray()

                        If part.ValueKind =
                           JsonValueKind.Number Then

                            Dim parsed As Integer

                            If part.TryGetInt32(
                                parsed
                            ) Then

                                parts.Add(
                                    parsed
                                )

                            End If

                        End If

                    Next

                    If parts.Count = 0 Then
                        Continue For
                    End If

                    Dim year As Integer =
                        parts(0)

                    Dim month As Integer =
                        If(
                            parts.Count >= 2,
                            parts(1),
                            1
                        )

                    Dim day As Integer =
                        If(
                            parts.Count >= 3,
                            parts(2),
                            1
                        )

                    Try

                        Return New DateTime(
                            year,
                            month,
                            day
                        )

                    Catch
                    End Try

                Next

            Next

            Return Nothing

        End Function


        Private Shared Function ReadAuthors(
            message As JsonElement
        ) As List(Of CrossrefAuthorSuggestion)

            Dim authors As New List(Of CrossrefAuthorSuggestion)()
            Dim authorArray As JsonElement

            If Not message.TryGetProperty(
                "author",
                authorArray
            ) OrElse
               authorArray.ValueKind <>
                JsonValueKind.Array Then

                Return authors

            End If

            For Each item As JsonElement In
                authorArray.EnumerateArray()

                If item.ValueKind <>
                   JsonValueKind.Object Then

                    Continue For

                End If

                Dim author As New CrossrefAuthorSuggestion With {
                    .GivenName = ReadString(item, "given"),
                    .FamilyName = ReadString(item, "family"),
                    .Orcid = CrossrefApplyService.NormalizeOrcid(ReadString(item, "ORCID"))
                }

                Dim affiliationArray As JsonElement

                If item.TryGetProperty(
                    "affiliation",
                    affiliationArray
                ) AndAlso
                   affiliationArray.ValueKind =
                    JsonValueKind.Array Then

                    For Each affiliationItem As JsonElement In
                        affiliationArray.EnumerateArray()

                        If affiliationItem.ValueKind <>
                           JsonValueKind.Object Then

                            Continue For

                        End If

                        Dim name As String =
                            ReadString(
                                affiliationItem,
                                "name"
                            )

                        If Not String.IsNullOrWhiteSpace(
                            name
                        ) Then

                            author.Affiliations.Add(
                                name
                            )

                        End If

                    Next

                End If

                authors.Add(
                    author
                )

            Next

            Return authors

        End Function


        Private Shared Function CleanAbstract(
            rawAbstract As String
        ) As String

            If String.IsNullOrWhiteSpace(
                rawAbstract
            ) Then

                Return String.Empty

            End If

            Dim withoutTags As String =
                Regex.Replace(
                    rawAbstract,
                    "<[^>]+>",
                    " "
                )

            Return WebUtility.HtmlDecode(
                Regex.Replace(
                    withoutTags,
                    "\s+",
                    " "
                )
            ).Trim()

        End Function

    End Class

End Namespace
