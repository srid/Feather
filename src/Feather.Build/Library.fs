namespace Feather.Build

open System.IO
open Fluid
open Fluid.ViewEngine
open Microsoft.Extensions.FileProviders
open CommandLine
open SJP.FsNotify
open FSharp.Control.Reactive
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Westwind.AspNetCore.LiveReload

module Web =
    // TODO: do it functional way
    let configureServices (services: IServiceCollection) (fp: PhysicalFileProvider) =
        services
            .AddLiveReload(System.Action<LiveReloadConfiguration>(fun cfg ->
                cfg.FolderToMonitor <- fp.Root
            ))
            |> ignore
    let configureApp (app : IApplicationBuilder) (fp: IFileProvider) =
        let opts = StaticFileOptions()
        opts.FileProvider <-fp
        app
            .UseLiveReload()
            .UseStaticFiles(opts)
            |> ignore

    let run (fp: PhysicalFileProvider) = 
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                fun webHostBuilder ->
                    webHostBuilder
                        .UseWebRoot(fp.Root)
                        .Configure(fun app -> configureApp app fp)
                        .ConfigureServices(fun srv -> configureServices srv fp)
                        |> ignore)
            .Build()
            .RunAsync()

module Liquid = 
    let private mkEngineOpts(fp: IFileProvider) =
        let opts = FluidViewEngineOptions()
        do 
            opts.Parser <- FluidViewParser()
            opts.ViewsFileProvider <- fp
            opts.IncludesFileProvider <- fp
            opts.TemplateOptions.MemberAccessStrategy <- UnsafeMemberAccessStrategy.Instance
            opts.ViewLocationFormats.Add($"{{0}}{Constants.ViewExtension}")
        opts
    type Engine(fp: IFileProvider) =
        let renderer = FluidViewRenderer <| mkEngineOpts fp

        /// Render the given template, passing the given vars. Return the HTML rendered.
        member this.Render(name: string, value: obj) : string =
            use x = new StringWriter()
            renderer.RenderViewAsync(x, name, value) 
            |> Async.AwaitTask |> Async.RunSynchronously
            x.ToString()

module CLI =
    type Options = {
        [<Option('w', "watch", Default = false, HelpText = "Watch for template changes")>]
        watch : bool

        [<Option('C', "path", Default = ".", HelpText = "Source path")>]
        path : string
    }

    let generateOnce (engine: Liquid.Engine, x: obj, output: string) =
        let html = engine.Render("index.liquid", x)
        let htmlPath = Path.Join(output, "index.html")
        printfn $"W {htmlPath}"
        File.WriteAllText(htmlPath,html)

    let run argv x =
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
                generateOnce(Liquid.Engine fp, x, outputPath)
                if options.watch then
                    use watcher = new ObservableFileSystemWatcher(tmplPath)
                    let obs = 
                        watcher.Changed 
                        |> Observable.filter(fun evt -> evt.ChangeType = WatcherChangeTypes.Changed)
                        |> Observable.subscribe (fun evt -> 
                            printfn "! %s" evt.Name
                            generateOnce(Liquid.Engine fp, x, outputPath))
                    watcher.Start()
                    use outputFp = new PhysicalFileProvider(outputPath)
                    async {
                        return! Web.run outputFp  |> Async.AwaitTask
                    } |> Async.RunSynchronously
                    obs.Dispose()
                0 // return an integer exit code
        | _ ->
            1