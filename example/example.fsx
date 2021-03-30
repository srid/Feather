#!/usr/bin/env -S dotnet fsi 
#r "nuget: Feather.Build, 0.1.0-alpha"

open Feather.Build

CLI.run fsi.CommandLineArgs