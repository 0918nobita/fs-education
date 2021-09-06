module Prgoram

open System.IO
open FsToolkit.ErrorHandling
open Giraffe.ViewEngine

open SiteInfo

let siteInfo =
    { Name = "プログラミングをはじめよう"
      Description = "これから趣味でプログラミングを始めようとしている人のためのテキスト"
      BaseUrl = "https://fs-education.vercel.app"
      OutDir = "build" }

let baseMetadataTags =
    [ meta [ _charset "utf-8" ]
      meta [ _name "description"
             _content siteInfo.Description ]
      meta [ _property "og:locale"
             _content "ja_JP" ]
      meta [ _property "og:description"
             _content siteInfo.Description ]
      meta [ _name "twitter:card"
             _content "summary" ]
      meta [ _name "viewport"
             _content "width=device-width,initial-scale=1" ]
      link [ _rel "stylesheet"
             _href "style.css" ]
      link [ _rel "manifest"
             _href "manifest.json" ]
      link [ _rel "icon"
             _href "icon.svg"
             _type "image/svg+xml" ] ]

[<EntryPoint>]
let main _ =
    if not (Directory.Exists siteInfo.OutDir) then
        Directory.CreateDirectory siteInfo.OutDir
        |> ignore

    Directory.GetFiles "assets"
    |> Array.map
        (fun assetPath ->
            async { File.Copy(assetPath, Path.Combine(siteInfo.OutDir, Path.GetFileName assetPath), true) })
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    result {
        let! installOutput = Shell.exec "pnpm" [ "install" ] "../frontend"
        printfn "%s" installOutput
        let! buildOutput = Shell.exec "pnpm" [ "run"; "build" ] "../frontend"
        printfn "%s" buildOutput
    }
    |> Result.defaultWith (fun () -> failwith "Failed to build frontend")

    File.Copy("../frontend/dist/bundle.js", "./build/bundle.js", true)

    let res =
        Directory.EnumerateFiles "pages"
        |> Seq.map (PageInfo.fromMdAsync siteInfo baseMetadataTags)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ResultSeq.resultSequence2
        |> Result.map (Seq.sortBy (fun pageInfo -> pageInfo.PageNumber))

    match res with
    | Ok pages ->
        pages
        |> Seq.map Page.writeAsync
        |> Seq.toList
        |> List.append [ Page.writeIndexAsync siteInfo baseMetadataTags pages ]
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

        0
    | Error msgs ->
        msgs |> Seq.iter (eprintfn "%s")
        1
