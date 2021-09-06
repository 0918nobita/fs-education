module Page

open System.IO
open FsToolkit.ErrorHandling
open Giraffe.ViewEngine

open PageInfo
open SiteInfo

let writeAsync (page: PageInfo) =
    File.WriteAllTextAsync(page.HtmlPath, RenderView.AsString.htmlDocument page.HtmlContent)
    |> Async.AwaitTask

let writeIndexAsync (siteInfo: SiteInfo) baseMetadataTags (pages: PageInfo seq) =
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
                    (baseMetadataTags
                     @ [ title [] [ str siteInfo.Name ]
                         meta [ _property "og:title"
                                _content siteInfo.Name ]
                         meta [ _property "og:type"
                                _content "website" ]
                         meta [ _property "org:url"
                                _content siteInfo.BaseUrl ] ])
                body [] [
                    div [ _id "container" ] [
                        h1 [] [
                            span [ _class "word" ] [ str "プログラミング" ]
                            str "を"
                            span [ _class "word" ] [ str "はじめよう" ]
                        ]
                        ul [] liNodes
                    ]
                    script [ _src "bundle.js" ] []
                ]
            ]

        File.WriteAllText(Path.Combine(siteInfo.OutDir, "index.html"), RenderView.AsString.htmlDocument content)
    }
