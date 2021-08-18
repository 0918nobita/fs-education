#r "nuget: Markdig, 0.25.0"

open System.IO
open Markdig

let content =
    File.ReadAllText "intro.md"
    |> Markdown.Parse
    |> Markdown.ToHtml

let html = $"""<!DOCTYPE html>
<html lang="ja">
<head>
<meta charset="utf-8">
<title>プログラミングをはじめよう</title>
</head>
<body>
{content}</body>
</html>
"""

Directory.CreateDirectory "build"
File.WriteAllText ("build/index.html", html)
