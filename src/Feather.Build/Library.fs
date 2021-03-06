namespace Feather.Build

open System.IO
open Microsoft.Extensions.FileProviders
open FSharp.Control.Reactive

/// A simple live reloading server
module LiveReload =
    open Westwind.AspNetCore.LiveReload
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Logging

    let private configureServices (fp: PhysicalFileProvider) (services: IServiceCollection) =
        services
            .AddLiveReload(System.Action<LiveReloadConfiguration>(fun cfg ->
                cfg.FolderToMonitor <- fp.Root
            ))
            .AddDirectoryBrowser()
    let private mkStaticFileOptions(fp) =
        let opts = StaticFileOptions()
        opts.FileProvider <- fp
        opts
    let private mkDirectoryBrowserOptions(fp) =
        let opts = DirectoryBrowserOptions()
        opts.FileProvider <- fp 
        opts
    let private configureApp (fp: IFileProvider) (app : IApplicationBuilder) =
        app
            .UseLiveReload()
            .UseStaticFiles(mkStaticFileOptions fp)
            .UseDirectoryBrowser(mkDirectoryBrowserOptions fp)
    let private configureLogging (builder: ILoggingBuilder) =
        let filter (_provider: string) (category: string) (l : LogLevel) = 
            not (
                // Suppress verbose requesting logging from asp.net
                category.Contains "Diagnostics"
                || l < LogLevel.Information
            )
        builder
            .AddFilter(filter) 
            .AddConsole()
    let private configureBuilder 
        (fp: PhysicalFileProvider) 
        (builder: IWebHostBuilder) : IWebHostBuilder =
        builder
            .UseWebRoot(fp.Root)
            .Configure(configureApp fp >> ignore)
            .ConfigureServices(configureServices fp >> ignore)
            .ConfigureLogging(System.Action<ILoggingBuilder>(configureLogging >> ignore))
            // This selects a free port.
            .UseUrls("http://127.0.0.1:0")
    let run (fp: PhysicalFileProvider) = 
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                System.Action<IWebHostBuilder>(configureBuilder fp >> ignore))
            .Build()
            .RunAsync()

module Liquid = 
    open Fluid
    open Fluid.ViewEngine

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
    open CommandLine
    open SJP.FsNotify
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
                        return! LiveReload.run outputFp  |> Async.AwaitTask
                    } |> Async.RunSynchronously
                    obs.Dispose()
                0 // return an integer exit code
        | _ ->
            1