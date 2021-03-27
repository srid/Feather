// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System.IO
open Feather.Build

[<EntryPoint>]
let main _argv =
    let ex = "../../example"
    let mount = Template.FileSystem.readFolderOfMount $"{ex}/templates"
    Template.init mount
    let html = Template.dotLiquid mount "index" null
    printfn $"{html}"
    File.WriteAllText($"{ex}/output/index.html",html)
    0 // return an integer exit code