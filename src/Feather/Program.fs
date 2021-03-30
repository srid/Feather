open Feather.Build

type AppData =
    { siteTitle: string 
      siteAuthor: string
    }
let appData = { siteTitle = "Feather Example"; siteAuthor = "Srid" }
let userData = {| Name = "Srid"; Age = 36 |}

[<EntryPoint>]
let main argv =
    CLI.run argv {| appData with UserData = userData |}
