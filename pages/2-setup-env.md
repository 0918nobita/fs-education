---
title: 準備
---

# 準備

## <span class="word">.NET 5.0 SDK</span> の<span class="word">インストール</span>

<!-- .NET 5.0 SDK の導入手順をもっと丁寧に説明する -->

[こちら](https://dotnet.microsoft.com/download/dotnet/5.0) から .NET 5.0 SDK のインストーラをダウンロードしてください。

![.NET 5.0 SDK インストーラのダウンロードページ](https://user-images.githubusercontent.com/8453302/131018013-36a85a5a-9240-46a4-99ed-e0e1ce03c1d7.png)

Windows の場合は「PowerShell」を、macOS の場合は「ターミナル」を起動して、

```bash
dotnet --version
```

を入力し、エンターキーを押すことでこのコマンドを実行してください。その後

```bash
5.0.103
```

のように `.` (ピリオド) で区切られた数が表示されたら、 .NET 5.0 SDK のインストールに成功しています！

## <span class="word">VS Code</span> の<span class="word">インストール</span>

[こちら](https://code.visualstudio.com/download) から Visual Studio Code (以下、VS Code) のインストーラをダウンロードしてください。

<!-- スクリーンショットを貼る -->

## <span class="word">VS Code</span> を<span class="word">起動してみる</span>

<!-- スクリーンショットを貼る -->

## <span class="word">サンプルプログラム</span>の<span class="word">入手</span>

[こちら](https://github.com/0918nobita/fs-edu-materials/archive/refs/heads/main.zip) からサンプルプログラムを入手してください。zip 形式の圧縮ファイルでダウンロードされるので、適当な場所に解凍して使ってください。

## <span class="word">VS Code</span> <span class="word">拡張機能</span>の<span class="word">導入</span>

1. ionide-fsharp
2. .NET Interactive Notebooks
3. EditorConfig for VS Code

上記の VS Code 拡張機能をインストールしてください。環境によっては、サンプルプログラムを VS Code で開いた際に、これらをインストールするように勧める文章と、まとめてインストールするためのボタンが表示されるかもしれません。それを選択して自動でこれらをインストールしても大丈夫です。
