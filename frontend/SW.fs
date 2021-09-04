module SW

open Fable.Import

Browser.Navigator.navigator.serviceWorker
|> printfn "%A"
