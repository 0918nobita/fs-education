module HtmlGen

open Giraffe.ViewEngine
open FSharp.Formatting.Markdown

val mdDocToHtml : MarkdownDocument -> XmlNode list
