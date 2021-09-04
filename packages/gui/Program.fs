module Program

open Gtk

[<EntryPoint>]
let main argv =
    Application.Init()

    let app =
        new Application("org.gui.gui", GLib.ApplicationFlags.None)

    app.Register(GLib.Cancellable.Current) |> ignore

    let win = new Window.MainWindow()
    app.AddWindow(win)

    win.Show()

    Application.Run()
    0
