// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System.IO
open Feather.Build
open System.Threading
open Fake.IO.Globbing.Operators
open CommandLine
open Newtonsoft.Json.Linq

type Options = {
    [<Option('w', "watch", Default = false, HelpText = "Watch for template changes")>]
    watch : bool

    [<Option('C', "path", Default = ".", HelpText = "Source path")>]
    path : string
}

/// Static-site's user specific data.
/// 
/// TODO: For generic user sites, we may want to allow arbitrary JSON, rather 
/// than defining a F# type (whose type-safety for fine for custom-built sites).
/// But the story of HTML template engines in dotnet is lacking in regards to
/// support aritrary JSON well *in addition to* the other features (virtual fs,
/// etc.) we rely. So may very well end up compromising on the JSON support for
/// now, thus requiring custom sites to use Feature as a library to begin with.
/// Things can always change in the future.
type AppData =
    { siteTitle: string 
      siteAuthor: string
    }

let generateOnce (engine: Play.LiquidEngine, output: string) =
    let appData = { siteTitle = "Feather Example"; siteAuthor = "Srid" }
    // JSON support broken, but I won't need it anyway?
    // let json : JObject = JObject.Parse "{\"Name\": \"Srid\", \"Age\": 36, \"Favs\": [7, 4, 42]}"
    let userData = {| Name = "Srid"; Age = 36 |}
    // let value = {| appData with Foo = "bar"; UserData = userData |}
    let html = engine.Render("index.liquid", {| appData with UserData = userData; Extra = {| More = "more..!" |} |})
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
        let outputPath = Path.Join(Path.GetFullPath options.path, "output")
        generateOnce(Play.LiquidEngine tmplPath , outputPath)
        if options.watch then
            printfn "Watching for template changes"
            // Limit what we want to watch (.liquid files), because we don't
            // want to accidentally 'watching' output files.
            let filesToWatch = Path.Join(options.path, "templates", "*.liquid")
            use _watcher = !! filesToWatch |> Fake.IO.ChangeWatcher.run (fun changes -> 
                for change in changes do 
                    printfn $"! {change.Name}"
                // FIXME: This delay exists to workaround an IOException during
                // reading of a .liquid (because Fake watcher presumably locks it)
                Thread.Sleep(100)
                generateOnce(Play.LiquidEngine tmplPath, outputPath)
            )
            Thread.Sleep(Timeout.Infinite)
        0 // return an integer exit code
    | _ ->
        1