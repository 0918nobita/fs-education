module Metadata

open FSharp.Formatting.Markdown
open YamlDotNet.Serialization

let fromMdDoc (doc: MarkdownDocument) : Map<string, string> =
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
