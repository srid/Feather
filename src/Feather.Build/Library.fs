namespace Feather.Build

open DotLiquid
open DotLiquid.FileSystems

// Map from relative path to file contents
type FileMap = Map<string, string>

module Template =
    // Like `LocaFileSystem`, but looks up the files from an in-memory map (`FileMap`)
    let PureFileSystem(fileMap: FileMap) : IFileSystem =
        { new IFileSystem with 
            member self.ReadTemplateFile(ctx, name) =
                match fileMap.TryFind(name) with
                | None -> "Oops"
                | Some content -> content
        }

    // Initialize the template engine.
    //
    // Setting the DotLiquid's filesystem is a static assignment, so we are forced to do it ths way, instead of the functional way.
    let init templateDir = 
        // TODO: Use In-memory filesystem hash instead of LocalFileSystem
        // This would allow the pipeline to pass in file values
        Template.FileSystem <- LocalFileSystem templateDir

    // Render the template using the given model, and return the final HTML string
    // TODO: Domain types?
    let dotLiquid (template: string) (model: obj) : string =
        let tmpl = Template.Parse template 
        let html = 
            model 
            |> Hash.FromAnonymousObject
            |> tmpl.Render 
        html
