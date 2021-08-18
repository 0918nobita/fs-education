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

let commonMetaTags =
    [ meta [ _charset "utf-8" ]
      meta [ _name "viewport"
             _content "width=device-width,initial-scale=1" ] ]

let genPageInfo () : PageInfo seq =
    Directory.EnumerateFiles "pages"
    |> Seq.map
        (fun markdownPath ->
            let baseName =
                Path.GetFileNameWithoutExtension markdownPath

            let groups =
                (Regex("^([0-9]+)-").Match baseName).Groups

            if Seq.length groups <> 2 then
                failwith $"%s{markdownPath}: The file name format is invalid"

            let pageNumber = int groups.[1].Value

            let htmlFileName = $"%s{baseName}.html"
            let htmlPath = Path.Combine("build", htmlFileName)
            let markdownText = File.ReadAllText markdownPath
            let markdownAst = Markdown.Parse markdownText

            if Seq.isEmpty markdownAst then
                failwith $"%s{markdownPath}: No block found"

            let pageTitle =
                match markdownAst.[0] with
                | :? Syntax.HeadingBlock as headingBlock ->
                    match headingBlock.Inline with
                    | null -> failwith $"%s{markdownPath}: Failed to get ContainerInline"
                    | containerInline ->
                        match containerInline.FirstChild with
                        | null -> failwith $"%s{markdownPath}: Failed to get ContainerInline.FirstChild"
                        | firstChild -> string firstChild
                | _ -> failwith $"%s{markdownPath}: Page title is not set"

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

            { PageNumber = pageNumber
              MarkdownPath = markdownPath
              HtmlPath = htmlPath
              PublicPath = htmlFileName
              Title = pageTitle
              HtmlContent = htmlContent })

let writePageAsync (page: PageInfo) =
    File.WriteAllTextAsync(page.HtmlPath, RenderView.AsString.htmlDocument page.HtmlContent)
    |> Async.AwaitTask

let writePagesAsync (pages: PageInfo seq) =
    pages
    |> Seq.map writePageAsync
    |> Async.Parallel
    |> Async.map ignore

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

        File.WriteAllText("build/index.html", RenderView.AsString.htmlDocument content)
    }

let () =
    Directory.CreateDirectory "build" |> ignore

    let pages = genPageInfo ()

    [ writePagesAsync pages
      writeIndexPageAsync pages ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
