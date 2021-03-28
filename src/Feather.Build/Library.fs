namespace Feather.Build

open DotLiquid
open DotLiquid.FileSystems
open System.IO
open Fluid
open System.Text.Json
open Fluid.Values
open Newtonsoft.Json.Linq

module Play = 
    // TODO: 
    // - Layout demo
    // - Replace DotLiquid
    type R = JToken
    let demo = 
        // Build opts
        let opts = TemplateOptions()
        let lookup (src: JObject) (name: string) : R = src.GetValue(name)
        let getIt : System.Func<JObject, string, R> = 
            System.Func<JObject, string, R>(lookup)
        opts.MemberAccessStrategy.Register<JObject, R>(getIt)
        opts.ValueConverters.Add(fun x -> 
            match x with 
            | :? JObject as o ->
                x
            | _ -> null)
        opts.ValueConverters.Add(fun x -> 
            match x with 
            | :? JValue as v ->
                v.Value
            | _ -> null)
 
        let json = "{\"Name\": \"Srid\", \"Age\": 36, \"Favs\": [7, 4, 42]}"
        let template = "{{ Name }} is {{ Age }} years old. His favs are {% for fav in Favs %} {{fav}}, {% else %} none! {% endfor %}"

        let parser = FluidParser()
        let success, tmpl = parser.TryParse(template)
        match success with 
        | false -> printfn "fail"
        | true -> 
            let value = JObject.Parse(json)
            let ctx = TemplateContext(value, opts)
            let out = tmpl.Render(ctx)
            printfn $"{out}"
        printfn "Done."
        0

module Template =

    /// HTML templates presume a "filesystem" on which template files are stored.
    /// This module provides an implementation that abstracts away the
    /// storage-details by providing **pure tree of files** (as Map).
    module FileSystem =
        /// <summary>A `Mount` is an in-memory map of a filesystem folder
        /// 
        /// To create a Mount, use <see cref="readFolderOfMount"/>.
        /// </summary>
        type Mount = Map<
                        string, // Relative path to a file under the folder
                        string  // Contents of the file
                        >

        let lookupMount(mount: Mount, path: string): string option =
            match mount.TryFind(path) with 
            | None -> None 
            | Some content -> Some content

        /// Read the folder contents once as `Mount`
        let readFolderOfMount(folderPath: string) : Mount =
            // This doesn't recurse, but it should.
            let contents = 
                Directory.GetFiles(folderPath, "*.liquid")
                |> Array.map (fun path -> Path.GetRelativePath(folderPath, path), File.ReadAllText(path))
            let map = contents |> Map.ofArray
            map

    let private liquidFile(name) =
        sprintf "%s.liquid" name

    let private liquidFileUnexposed(name) =
        sprintf "_%s" (liquidFile name)

    /// <summary>Like `LocaFileSystem`, but looks up the files from an in-memory map (`Mount`)</summary>
    let PureFileSystem(mount: FileSystem.Mount) : IFileSystem =
        { new IFileSystem with 
            member self.ReadTemplateFile(_ctx, name) =
                let fn = liquidFileUnexposed name
                match FileSystem.lookupMount(mount, fn) with
                | None -> $"Not Found ({name})"
                | Some content -> content
        }

    /// Initialize the template engine.
    ///
    /// Setting the DotLiquid's filesystem is a static assignment, so we are forced to do it ths way, instead of the functional way.
    let init mount = 
        Template.FileSystem <- PureFileSystem mount

    /// Render the template using the given model, and return the final HTML string
    /// TODO: Domain types?
    let dotLiquid (mount: FileSystem.Mount) (templateName: string) (model: Hash) : string =
        match FileSystem.lookupMount(mount, liquidFile templateName) with 
        | None -> "Not Found ({templateName})"
        | Some content -> 
            let tmpl = Template.Parse content 
            let html = 
                model 
                // |> Hash.FromAnonymousObject
                |> tmpl.Render 
            html
