module Shell

open System.Diagnostics

let exec (cmd: string) (args: string list) (workingDirectory: string) : Result<string, string> =
    use p = new Process()
    p.StartInfo.FileName <- cmd
    p.StartInfo.Arguments <- String.concat " " args
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.WorkingDirectory <- workingDirectory

    try
        p.Start() |> ignore
        p.StandardOutput.ReadToEnd() |> Ok
    with
    | e -> Error e.Message
