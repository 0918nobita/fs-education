module PageInfo

open System.IO
open System.Text.RegularExpressions
open FSharp.Formatting.Markdown
open FsToolkit.ErrorHandling
open Giraffe.ViewEngine

open SiteInfo

type PageInfo =
    { PageNumber: int
      MarkdownPath: string
      HtmlPath: string
      PublicPath: string
      Title: string
      HtmlContent: XmlNode }

let fromMdAsync (siteInfo: SiteInfo) baseMetadataTags (markdownPath: string) =
    asyncResult {
        let baseName =
            Path.GetFileNameWithoutExtension markdownPath

        let groups =
            (Regex("^([0-9]+)-").Match baseName).Groups

        if Seq.length groups <> 2 then
            return! Error $"%s{markdownPath}: The file name format is invalid"

        let pageNumber = int groups.[1].Value

        let htmlFileName = $"%s{baseName}.html"

        let htmlPath =
            Path.Combine(siteInfo.OutDir, htmlFileName)

        let markdownDoc =
            Markdown.Parse(File.ReadAllText markdownPath, "\n", MarkdownParseOptions.AllowYamlFrontMatter)

        let pageContent = HtmlGen.mdDocToHtml markdownDoc

        let pageTitle =
            match markdownDoc
                  |> Metadata.fromMdDoc
                  |> Map.tryFind "title" with
            | Some title -> title
            | None -> failwith "Failed to get title"

        let detailedPageTitle = $"%s{pageTitle} | %s{siteInfo.Name}"

        let htmlContent =
            html [ _lang "ja" ] [
                head
                    []
                    (baseMetadataTags
                     @ [ title [] [ str detailedPageTitle ]
                         meta [ _property "og:title"
                                _content detailedPageTitle ]
                         meta [ _property "og:type"
                                _content "article" ]
                         meta [ _property "og:url"
                                _content $"%s{siteInfo.BaseUrl}/%s{htmlFileName}" ] ])
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
