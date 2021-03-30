#!/usr/bin/env -S dotnet fsi
#r "nuget: Feather.Build, 0.1.0-alpha"

type AppData =
    { siteTitle: string 
      siteAuthor: string
    }
let appData = { siteTitle = "Feather Example"; siteAuthor = "Srid" }
let userData = {| Name = "Srid"; Age = 36 |}

Feather.Build.CLI.run fsi.CommandLineArgs {| appData with UserData = userData |}