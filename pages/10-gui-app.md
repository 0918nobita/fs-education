---
title: GUI アプリケーションを作ろう
---

# <span class="word">GUI</span> <span class="word">アプリケーション</span>を<span class="word">作ろう</span>

<!-- ホントは以下のように書いて優先される改行位置を設定したいけど、ページタイトルがおかしくなるのでまだ使えない -->
<!-- # <span class="word">GUI</span> <span class="word">アプリケーション</span><span class="word">を</span><span class="word">作ろう</span> -->

いよいよ今回からはウィンドウの表示される本格的なプログラムを作っていきます

```bash
dotnet new -i GtkSharp.Template.FSharp::3.24.24.34
dotnet new gtkapp -lang="F#" -o gui
```
