namespace Feather.Build

open System.IO
open Fluid
open Fluid.ViewEngine
open Microsoft.Extensions.FileProviders

module Liquid = 
    let private mkEngineOpts(path) =
        let physicalFs = new PhysicalFileProvider(path)
        let opts = FluidViewEngineOptions()
        do 
            opts.Parser <- FluidViewParser()
            opts.ViewsFileProvider <- physicalFs
            opts.IncludesFileProvider <- physicalFs
            opts.TemplateOptions.MemberAccessStrategy <- UnsafeMemberAccessStrategy.Instance
            opts.ViewLocationFormats.Add($"{{0}}{Constants.ViewExtension}")
        opts
    type Engine(path: string) =
        let renderer = FluidViewRenderer <| mkEngineOpts path

        /// Render the given template, passing the given vars. Return the HTML rendered.
        member this.Render(name: string, value: obj) : string =
            use x = new StringWriter()
            renderer.RenderViewAsync(x, name, value) 
            |> Async.AwaitTask |> Async.RunSynchronously
            x.ToString()
