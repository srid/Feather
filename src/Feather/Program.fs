// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System.IO
open Feather.Build
open System.Threading
open CommandLine
open Microsoft.Extensions.FileProviders
open SJP.FsNotify
open FSharp.Control.Reactive

type Options = {
    [<Option('w', "watch", Default = false, HelpText = "Watch for template changes")>]
    watch : bool

    [<Option('C', "path", Default = ".", HelpText = "Source path")>]
    path : string
}

/// Static-site's user specific data.
/// 
/// TODO: For generic user sites, we may want to allow arbitrary JSON, rather 
/// than defining a F# type (whose type-safety is good for custom-built sites).
/// But the story of HTML template engines in dotnet is lacking in regards to
/// support aritrary JSON well *in addition to* the other features (virtual fs,
/// etc.) we rely. So may very well end up compromising on the JSON support for
/// now, thus requiring custom sites to use Feature as a library to begin with.
/// Things can always change in the future.
type AppData =
    { siteTitle: string 
      siteAuthor: string
    }

let generateOnce (engine: Liquid.Engine, output: string) =
    let appData = { siteTitle = "Feather Example"; siteAuthor = "Srid" }
    let userData = {| Name = "Srid"; Age = 36 |}
    let html = engine.Render("index.liquid", {| appData with UserData = userData |})
    let htmlPath = Path.Join(output, "index.html")
    printfn $"W {htmlPath}"
    File.WriteAllText(htmlPath,html)

[<EntryPoint>]
let main argv =
    let result = Parser.Default.ParseArguments<Options>(argv)
    match result with 
    | :? Parsed<Options> as parsed -> 
        let options = parsed.Value
        let tmplPath = Path.Join(Path.GetFullPath options.path, "templates")
        match Directory.Exists(tmplPath) with 
        | false ->
            printfn $"Error: {tmplPath} does not exist"
            2
        | true ->
            let outputPath = Path.Join(Path.GetFullPath options.path, "output")
            Directory.CreateDirectory(outputPath) |> ignore
            let fp = new PhysicalFileProvider(tmplPath)
            generateOnce(Liquid.Engine fp, outputPath)
            if options.watch then
                use watcher = new ObservableFileSystemWatcher(tmplPath)
                let obs = 
                    watcher.Changed 
                    |> Observable.filter(fun evt -> evt.ChangeType = WatcherChangeTypes.Changed)
                    |> Observable.subscribe (fun evt -> 
                        printfn "! %s" evt.Name
                        generateOnce(Liquid.Engine fp, outputPath))
                watcher.Start()
                Thread.Sleep(Timeout.Infinite)
                obs.Dispose()
            0 // return an integer exit code
    | _ ->
        1