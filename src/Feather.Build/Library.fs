namespace Feather.Build

open System.IO
open Fluid
open Fluid.ViewEngine
open Microsoft.Extensions.FileProviders

module Liquid = 
    type Engine(path: string) =
        // Build opts
        let physicalFs = new PhysicalFileProvider(path)
        let opts = FluidViewEngineOptions()
        do 
            opts.Parser <- FluidViewParser()
            opts.ViewsFileProvider <- physicalFs
            opts.IncludesFileProvider <- physicalFs
            opts.TemplateOptions.MemberAccessStrategy <- UnsafeMemberAccessStrategy.Instance
            opts.ViewLocationFormats.Add($"{{0}}{Constants.ViewExtension}")
        let renderer = FluidViewRenderer(opts)

        member this.Render(name: string, value: obj) : string =
            use x = new StringWriter()
            renderer.RenderViewAsync(x, name, value) 
            |> Async.AwaitTask |> Async.RunSynchronously
            let out = x.ToString()
            printfn $"{out}"
            out
