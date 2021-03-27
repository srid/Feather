// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System.IO
open Feather.Build
open System.Threading
open Fake.IO.Globbing.Operators
open CommandLine

type Options = {
    [<Option('w', "watch", Default = false, HelpText = "Watch for template changes")>]
    watch : bool

    [<Option('C', "path", Default = ".", HelpText = "Source path")>]
    path : string
}

let generateOnce path =
    let mount = Template.FileSystem.readFolderOfMount $"{path}/templates"
    Template.init mount
    let html = Template.dotLiquid mount "index" null
    let htmlPath = $"{path}/output/index.html"
    printfn $"W {htmlPath}"
    File.WriteAllText(htmlPath,html)

[<EntryPoint>]
let main argv =
    let result = Parser.Default.ParseArguments<Options>(argv)
    match result with 
    | :? Parsed<Options> as parsed -> 
        let options = parsed.Value
        generateOnce options.path
        if options.watch then
            printfn "Watching for template changes"
            use _watcher = !! $"**/*.*" |> Fake.IO.ChangeWatcher.run (fun changes -> 
                for change in changes do 
                    printfn $"! {change}"
                generateOnce options.path
            )
            Thread.Sleep(Timeout.Infinite)
        0 // return an integer exit code
    | _ ->
        1