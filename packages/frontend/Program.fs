module Program

open Fable.Core
open Fable.Import
open FsToolkit.ErrorHandling

option {
    let! serviceWorker = Browser.Navigator.navigator.serviceWorker
    JS.console.log serviceWorker
}
|> ignore
