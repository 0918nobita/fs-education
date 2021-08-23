module HtmlGen

open FSharp.Formatting.Markdown

let rec private mdSpanToHtml (span: MarkdownSpan) : string =
    match span with
    | Literal (text = text) -> text
    | Strong (body = body) ->
        body
        |> List.map mdSpanToHtml
        |> String.concat ""
        |> sprintf "<b>%s</b>"
    | _ -> ""

let rec private mdParagraphToHtml (paragraph: MarkdownParagraph) : string =
    match paragraph with
    | Heading (size = size; body = [ Literal (text = text) ]) -> $"<h%i{size}>%s{text}</h%i{size}>"
    | Paragraph (body = body) ->
        body
        |> List.map mdSpanToHtml
        |> String.concat ""
        |> sprintf "<p>%s</p>"
    | CodeBlock (code = code) ->
        $"<div class=\"code-outer\"><div class=\"code-inner\"><pre><code>%s{code}</code></pre><div class=\"code-padding-right\"></div></div></div>"
    | ListBlock (kind = Ordered; items = items) ->
        items
        |> List.map (
            List.map mdParagraphToHtml
            >> String.concat ""
            >> sprintf "<li>%s</li>"
        )
        |> String.concat ""
        |> sprintf "<ol>%s</ol>"
    | Span (body = spans) -> spans |> List.map mdSpanToHtml |> String.concat ""
    | _ -> ""

let mdDocToHtml (doc: MarkdownDocument) : string =
    doc.Paragraphs
    |> List.map mdParagraphToHtml
    |> String.concat "\n"
