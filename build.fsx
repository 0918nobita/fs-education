#r "nuget: FsToolkit.ErrorHandling, 2.7.0"
#r "nuget: Markdig, 0.25.0"

open System.IO
open FsToolkit.ErrorHandling
open Markdig

let compileAsync ((inputPath, outputPath) : string * string) : Async<Result<unit, string>> =
    asyncResult {
        let text = File.ReadAllText inputPath

        let doc = Markdown.Parse text
        if Seq.isEmpty doc then return! Error $"{inputPath}: No block found"

        let! (mdAst, title) =
            match doc.[0] with
            | :? Syntax.HeadingBlock as headingBlock ->
                match headingBlock.Inline with
                | null -> Error $"{inputPath}: Failed to get ContainerInline"
                | containerInline ->
                    match containerInline.FirstChild with
                    | null -> Error $"{inputPath}: Failed to get ContainerInline.FirstChild"
                    | firstChild -> Ok (doc, string firstChild)
            | _ -> Error $"{inputPath}: Page title is not set"

        let content = Markdown.ToHtml mdAst
        let html = $"""<!DOCTYPE html>
<html lang="ja">
<head>
<meta charset="utf-8">
<title>{title} | プログラミングをはじめよう</title>
</head>
<body>
{content}</body>
</html>
"""
        File.WriteAllText (outputPath, html)
    }

let sumResultSeq (resSeq: seq<Result<'a, 'b>>) : Result<seq<'a>, seq<'b>> =
    resSeq
    |> Seq.fold
        (fun s a ->
            match s, a with
            | Ok arr,   Ok v ->    Ok (Seq.append arr (Seq.singleton v))
            | Ok _,     Error e -> Error (Seq.singleton e)
            | Error es, Ok _ ->    Error es
            | Error es, Error e -> Error (Seq.append es (Seq.singleton e)))
        (Ok Seq.empty)

Directory.CreateDirectory "build"

let res =
    Directory.EnumerateFiles "pages"
    |> Seq.map (
        (fun inputPath ->
            let outputPath =
                inputPath
                |> Path.GetFileNameWithoutExtension
                |> sprintf "%s.html"
                |> fun filename -> Path.Combine ("build", filename)
            (inputPath, outputPath))
        >> compileAsync)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> sumResultSeq

exit
    (match res with
    | Ok(_) -> 0
    | Error(msgs) ->
        msgs |> Seq.iter (eprintfn "%s")
        1)
