namespace Feather.Build

open System.IO
open Fluid
open Fluid.ViewEngine
open Newtonsoft.Json.Linq
open Microsoft.Extensions.FileProviders

module Liquid = 
    // TODO: 
    // - Cleanup
    module Json =
        type R = JToken

        let enableJson(vopts: FluidViewEngineOptions) =
            let lookup (src: JObject) (name: string) : R = src.GetValue(name)
            let getIt : System.Func<JObject, string, R> = 
                System.Func<JObject, string, R>(lookup)
            vopts.TemplateOptions.MemberAccessStrategy.Register<JObject, R>(getIt)
            vopts.TemplateOptions.ValueConverters.Add(fun x -> 
                match x with 
                | :? JObject as o ->
                    x
                | _ -> null)
            vopts.TemplateOptions.ValueConverters.Add(fun x -> 
                match x with 
                | :? JValue as v ->
                    v.Value
                | _ -> null)

    type Engine(path: string) =
        // Build opts
        let physicalFs = new PhysicalFileProvider(path)
        let vopts = FluidViewEngineOptions()
        do 
            vopts.Parser <- FluidViewParser()
            vopts.ViewsFileProvider <- physicalFs
            vopts.IncludesFileProvider <- physicalFs
            vopts.TemplateOptions.MemberAccessStrategy <- UnsafeMemberAccessStrategy.Instance
            // Json.enableJson(vopts)
            vopts.ViewLocationFormats.Add($"{{0}}{Constants.ViewExtension}")
        let renderer = FluidViewRenderer(vopts)

        member this.Render(name: string, value: obj) : string =
            printfn $"{value}"
            use x = new StringWriter()
            async {
                renderer.RenderViewAsync(x, name, value) 
                |> Async.AwaitTask
                |> ignore
            } |> Async.RunSynchronously
            let out = x.ToString()
            printfn $"{out}"
            printfn "Done."
            out
