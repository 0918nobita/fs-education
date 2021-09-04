module Prgoram

open System.IO
open System.Text.RegularExpressions
open FSharp.Formatting.Markdown
open FsToolkit.ErrorHandling
open YamlDotNet.Serialization

let getMetaDataFromMdDoc (doc: MarkdownDocument) : Map<string, string> =
    let desrializer =
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.CamelCaseNamingConvention.Instance)
            .Build()

    doc.Paragraphs
    |> List.tryPick
        (function
        | YamlFrontmatter (yaml = yaml) ->
            yaml
            |> String.concat "\n"
            |> desrializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>
            :> seq<_>
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq
            |> Some
        | _ -> None)
    |> Option.defaultValue Map.empty

open Giraffe.ViewEngine

type PageInfo =
    { PageNumber: int
      MarkdownPath: string
      HtmlPath: string
      PublicPath: string
      Title: string
      HtmlContent: XmlNode }

let baseUrl = "https://fs-education.vercel.app"
let siteName = "プログラミングをはじめよう"
let siteDescription = "これから趣味でプログラミングを始めようとしている人のためのテキスト"
let outDir = "build"

let commonTags =
    [ meta [ _charset "utf-8" ]
      meta [ _name "description"
             _content siteDescription ]
      meta [ _property "og:locale"
             _content "ja_JP" ]
      meta [ _property "og:description"
             _content siteDescription ]
      meta [ _name "twitter:card"
             _content "summary" ]
      meta [ _name "viewport"
             _content "width=device-width,initial-scale=1" ]
      link [ _rel "stylesheet"
             _href "style.css" ]
      link [ _rel "manifest"
             _href "manifest.json" ]
      link [ _rel "icon"
             _href
                 "data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22><text x=%2250%%22 y=%2250%%22 style=%22dominant-baseline:central;text-anchor:middle;font-size:90px;%22>🔷</text></svg>" ] ]

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

        let markdownDoc =
            Markdown.Parse(File.ReadAllText markdownPath, "\n", MarkdownParseOptions.AllowYamlFrontMatter)

        let pageContent = HtmlGen.mdDocToHtml markdownDoc

        let pageTitle =
            match markdownDoc
                  |> getMetaDataFromMdDoc
                  |> Map.tryFind "title" with
            | Some (title) -> title
            | None -> failwith "Failed to get title"

        let detailedPageTitle = $"%s{pageTitle} | %s{siteName}"

        let htmlContent =
            html [ _lang "ja" ] [
                head
                    []
                    (commonTags
                     @ [ title [] [ str detailedPageTitle ]
                         meta [ _property "og:title"
                                _content detailedPageTitle ]
                         meta [ _property "og:type"
                                _content "article" ]
                         meta [ _property "og:url"
                                _content $"%s{baseUrl}/%s{htmlFileName}" ] ])
                body [] [
                    div [ _id "container" ] pageContent
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
            html [ _lang "ja" ] [
                head
                    []
                    (commonTags
                     @ [ title [] [ str siteName ]
                         meta [ _property "og:title"
                                _content siteName ]
                         meta [ _property "og:type"
                                _content "website" ]
                         meta [ _property "org:url"
                                _content baseUrl ] ])
                body [] [
                    div [ _id "container" ] [
                        h1 [] [
                            span [ _class "word" ] [ str "プログラミング" ]
                            str "を"
                            span [ _class "word" ] [ str "はじめよう" ]
                        ]
                        ul [] liNodes
                    ]
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

[<EntryPoint>]
let main argv =
    let res =
        Directory.EnumerateFiles "pages"
        |> Seq.map genSinglePageInfoAsync
        |> Async.Parallel
        |> Async.RunSynchronously
        |> resultSequence2
        |> Result.map (Seq.sortBy (fun pageInfo -> pageInfo.PageNumber))

    match res with
    | Ok pages ->
        pages
        |> Seq.map writePageAsync
        |> Seq.toList
        |> List.append [ writeIndexPageAsync pages ]
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        0
    | Error msgs ->
        msgs |> Seq.iter (eprintfn "%s")
        1
