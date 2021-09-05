module Program

open Fable.Import
open FsToolkit.ErrorHandling

option {
    let! serviceWorker = Browser.Navigator.navigator.serviceWorker
    serviceWorker.register "/service-worker.js" |> ignore
}
|> ignore
