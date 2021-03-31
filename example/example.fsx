#!/usr/bin/env -S dotnet fsi
#r "nuget: Feather.Build, 0.1.0-alpha-2"
// Workaround for https://github.com/dotnet/fsharp/issues/3178#issuecomment-811174453
#r "nuget: Microsoft.Extensions.FileProviders.Physical, 6.0.0-preview.2.21154.6"

type AppData =
    { siteTitle: string 
      siteAuthor: string
    }
let appData = { siteTitle = "Feather Example"; siteAuthor = "Srid" }
let userData = {| Name = "Srid"; Age = 36 |}

Feather.Build.CLI.run fsi.CommandLineArgs {| appData with UserData = userData |}