module HtmlGen

open Giraffe.ViewEngine
open FSharp.Formatting.Markdown

let rec mdSpanToHtml (span: MarkdownSpan) : XmlNode option =
    match span with
    | Literal (text = text) -> Some(rawText text)
    | Strong (body = body) -> body |> List.choose mdSpanToHtml |> b [] |> Some
    | DirectImage (body = body; link = link) -> Some(img [ _src link; _alt body ])
    | DirectLink (body = body; link = link) ->
        body
        |> List.choose mdSpanToHtml
        |> a [ _href link ]
        |> Some
    | _ ->
        eprintfn "skipped:"
        eprintfn "%A" span
        None

let rec mdParagraphToHtml (paragraph: MarkdownParagraph) : option<list<XmlNode>> =
    match paragraph with
    | Heading (size = size; body = body) ->
        let tag =
            match size with
            | 1 -> h1
            | 2 -> h2
            | 3 -> h3
            | _ -> failwith "Unsupported heading"

        body
        |> List.choose mdSpanToHtml
        |> tag []
        |> List.singleton
        |> Some
    | Paragraph (body = body) ->
        body
        |> List.choose mdSpanToHtml
        |> p []
        |> List.singleton
        |> Some
    | CodeBlock (code = codeText) ->
        div [ _class "code-outer" ] [
            div [ _class "code-inner" ] [
                pre [] [ code [] [ str codeText ] ]
                div [ _class "code-padding-right" ] []
            ]
        ]
        |> List.singleton
        |> Some
    | ListBlock (kind = Ordered; items = items) ->
        items
        |> List.collect (List.choose mdParagraphToHtml)
        |> List.concat
        |> ol []
        |> List.singleton
        |> Some
    | Span (body = spans) -> spans |> List.choose mdSpanToHtml |> Some
    | _ -> None

let mdDocToHtml (doc: MarkdownDocument) : XmlNode list =
    doc.Paragraphs
    |> List.choose mdParagraphToHtml
    |> List.concat
