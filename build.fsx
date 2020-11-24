// xake build file

#r "nuget: Xake, 1.1.4.427-beta"
#r "nuget: Xake.Dotnet, 1.1.4.7-beta"

open Xake
open Xake.Tasks

let shellx cmd folder =
    let command::arglist | OtherwiseFail(command,arglist) = (cmd: string).Split(' ') |> List.ofArray
    shell {
        cmd command
        args arglist
        workdir folder
        failonerror
    } |> Ignore

let (=?) value deflt = match value with |Some v -> v |None -> deflt

let getVersion () = recipe {
    let! ver = getEnv("VER")
    return ver =? "0.0.1"
}

do xake {ExecOptions.Default with FileLog = "build.log"; ConLogLevel = Loud } {

    rules [
        "main"  <== ["build"; "test"; "nuget-pack"]

        "clean" => recipe {
            do! rm { dir "Src/**/bin" }
            do! rm { dir "Src/**/obj" }
        }

        "build" ..> recipe {
            let! ver = getVersion()
            do! "Src/Core" |> shellx $"dotnet build -c Release -p:Version={ver}"
        }

        "test" ..> recipe {
            let! testFiles = !! "Src/Core.Tests/**/*.cs" |> getFiles
            do! needFiles testFiles

            do! shellx "dotnet test" "Src/Core.Tests"
        }

        "pack" ..> recipe {
            let! ver = getVersion()
            do! "Src/Core" |> shellx $"dotnet pack -c Release -p:Version={ver}"
        }

        "push" => action {
            do! need ["pack"]

            let! ver = getVersion()
            let! nuget_key_var = getEnv("NUGET_KEY")

            let package_name = $"NEbml.{ver}.nupkg"
            let nuget_key = nuget_key_var =? ""
            do! "Src/Core/bin/Release" |> shellx $"dotnet nuget push {package_name} --api-key {nuget_key} --source https://api.nuget.org/v3/index.json"
        }
    ]

}
