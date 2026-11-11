// Xake build file
//
//   dotnet fsi build.fsx -- -- [targets]     (default target: main = build, test, pack)
//   dotnet fsi build.fsx -- -- push          (VERSION and NUGET_KEY from the environment)
//
// A failing command fails the build: Xake prints the error and exits with code 2.

#r "nuget: Xake, 4.0.0"

open Xake
open Xake.Tasks

// Every `sh` below says `failonerror` explicitly. In Xake 4.0.0 only the empty form
// `sh "..." { () }` fails on a non-zero exit code by default; a block with settings
// (`workdir`, `args`) starts from options that ignore the exit code. That is how a
// rejected `dotnet nuget push` (403, expired key) ended in "Build completed" for v1.1.1.

let packageDir = "out/nupkg"

let getVersion = recipe {
    let! ver = getVar "VERSION"
    return ver |> Option.defaultValue "0.0.1"
}

do xakeScript {
    filelog "build.log" Chatty
    consolelog Chatty       // Info level: the console (and CI log) shows the tools' output, errors included

    var "VERSION" [Env "VERSION"; Default "0.0.1"]
    var "NUGET_KEY" [Env "NUGET_KEY"; Secret]   // never logged; masked as *** in every log
    var "NUGET_SOURCE" [Env "NUGET_SOURCE"; Default "https://api.nuget.org/v3/index.json"]

    "main" <<< ["build"; "test"; "pack"]

    command "clean" {
        do! rm { dir "Src/**/bin" }
        do! rm { dir "Src/**/obj" }
        do! rm { dir "out" }
    }

    command "build" {
        let! ver = getVersion
        do! sh "dotnet build Src/Core/Core.csproj -c Release" {
            arg $"-p:Version={ver}"
            failonerror
        }
    }

    command "test" {
        do! sh "dotnet test Src/Core.Tests/Core.Tests.csproj" {
            failonerror
        }
    }

    command "pack" {
        let! ver = getVersion
        do! rm { dir packageDir }
        do! sh "dotnet pack Src/Core/Core.csproj -c Release" {
            args [$"-p:Version={ver}"; "-o"; packageDir]
            failonerror
        }
    }

    command "push" {
        let! key = getVar "NUGET_KEY"
        let key =
            match key with
            | Some k when k.Trim() <> "" -> k
            | _ -> failwith "NUGET_KEY is not set: the package cannot be pushed"

        do! need ["pack"]
        let! ver = getVersion
        let! source = getVar "NUGET_SOURCE"

        do! sh "dotnet nuget push" {
            args [
                $"{packageDir}/NEbml.{ver}.nupkg"
                "--api-key"; key
                "--source"; Option.get source
            ]
            failonerror
        }
    }
}
