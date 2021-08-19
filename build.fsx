#r "nuget: FsToolkit.ErrorHandling, 2.7.0"
#r "nuget: Giraffe.ViewEngine"
#r "nuget: Markdig, 0.25.0"

open System.IO
open System.Text.RegularExpressions
open FsToolkit.ErrorHandling
open Giraffe.ViewEngine
open Markdig

type PageInfo =
    { PageNumber: int
      MarkdownPath: string
      HtmlPath: string
      PublicPath: string
      Title: string
      HtmlContent: XmlNode }

let siteName = "プログラミングをはじめよう"

let outDir = "build"

let commonMetaTags =
    [ meta [ _charset "utf-8" ]
      meta [ _name "viewport"
             _content "width=device-width,initial-scale=1" ] ]

let genSinglePageInfoAsync (markdownPath: string) =
    asyncResult {
        let baseName =
            Path.GetFileNameWithoutExtension markdownPath

        let groups =
            (Regex("^([0-9]+)-").Match baseName).Groups

        if Seq.length groups <> 2 then
            return! Error $"%s{markdownPath}: The file name format is invalid"

        let pageNumber = int groups.[1].Value

        let htmlFileName = $"%s{baseName}.html"
        let htmlPath = Path.Combine(outDir, htmlFileName)
        let markdownText = File.ReadAllText markdownPath
        let markdownAst = Markdown.Parse markdownText

        if Seq.isEmpty markdownAst then
            return! Error $"%s{markdownPath}: No block found"

        let! pageTitle =
            match markdownAst.[0] with
            | :? Syntax.HeadingBlock as headingBlock ->
                match headingBlock.Inline with
                | null -> Error $"%s{markdownPath}: Failed to get ContainerInline"
                | containerInline ->
                    match containerInline.FirstChild with
                    | null -> Error $"%s{markdownPath}: Failed to get ContainerInline.FirstChild"
                    | firstChild -> Ok(string firstChild)
            | _ -> Error $"%s{markdownPath}: Page title is not set"

        let htmlContent =
            html [] [
                head
                    []
                    (commonMetaTags
                     @ [ title [] [
                             str $"%s{pageTitle} | %s{siteName}"
                         ] ])
                body [] [
                    rawText (Markdown.ToHtml markdownAst)
                ]
            ]

        return
            { PageNumber = pageNumber
              MarkdownPath = markdownPath
              HtmlPath = htmlPath
              PublicPath = htmlFileName
              Title = pageTitle
              HtmlContent = htmlContent }

    }

let writePageAsync (page: PageInfo) =
    File.WriteAllTextAsync(page.HtmlPath, RenderView.AsString.htmlDocument page.HtmlContent)
    |> Async.AwaitTask

let writeIndexPageAsync (pages: PageInfo seq) =
    let liNodes =
        pages
        |> Seq.toList
        |> List.map
            (fun page ->
                li [] [
                    a [ _href page.PublicPath ] [
                        str page.Title
                    ]
                ])

    async {
        let content =
            html [] [
                head [] (commonMetaTags @ [ title [] [ str siteName ] ])
                body [] [
                    h1 [] [ str siteName ]
                    ul [] liNodes
                ]
            ]

        File.WriteAllText(Path.Combine(outDir, "index.html"), RenderView.AsString.htmlDocument content)
    }

let resultSequence2 (resSeq: seq<Result<'a, 'b>>) : Result<seq<'a>, seq<'b>> =
    resSeq
    |> Seq.fold
        (fun s a ->
            match s, a with
            | Ok arr, Ok v -> Ok(Seq.append arr (Seq.singleton v))
            | Ok _, Error e -> Error(Seq.singleton e)
            | Error es, Ok _ -> Error es
            | Error es, Error e -> Error(Seq.append es (Seq.singleton e)))
        (Ok Seq.empty)

let () =
    Directory.CreateDirectory outDir |> ignore

    let res =
        Directory.EnumerateFiles "pages"
        |> Seq.map genSinglePageInfoAsync
        |> Async.Parallel
        |> Async.RunSynchronously
        |> resultSequence2

    match res with
    | Ok (pages) ->
        pages
        |> Seq.map writePageAsync
        |> Seq.toList
        |> List.append [ writeIndexPageAsync pages ]
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore
    | Error (msgs) ->
        msgs |> Seq.iter (eprintfn "%s")
        exit 1
