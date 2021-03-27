// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Feather.Build

[<EntryPoint>]
let main _argv =
    Template.init "templates"
    let html = Template.dotLiquid "Hello world" null
    printfn $"{html}"
    0 // return an integer exit code