// xake build file

#r "nuget: Xake, 2"

open Xake
open Xake.Tasks

let sh folder cmd =
    let command::arglist | OtherwiseFail(command,arglist) = (cmd: string).Split(' ') |> List.ofArray
    shell {
        cmd command
        args arglist
        workdir folder
        failonerror
    } |> Ignore

let (=?) value deflt = match value with |Some v -> v |None -> deflt

let getVersion () = recipe {
    let! ver = getEnv "VER"
    return ver =? "0.0.1"
}

let dependsOn (fileset) = recipe {
    let! files = fileset |> getFiles
    do! needFiles files
}

do xake {ExecOptions.Default with FileLog = "build.log"; ConLogLevel = Loud } {

    rules [
        "main"  <== ["build"; "test"; "pack"]

        "clean" => recipe {
            do! rm { dir "Src/**/bin" }
            do! rm { dir "Src/**/obj" }
        }

        "build" ..> recipe {
            do! dependsOn !! "Src/Core/**/*.cs"
            let! ver = getVersion()
            do! sh "Src/Core" $"dotnet build -c Release -p:Version={ver}"
        }

        "test" ..> recipe {
            do! dependsOn !! "Src/Core.Tests/**/*.cs"
            do! sh "Src/Core.Tests" "dotnet test"
        }

        "pack" ..> recipe {
            let! ver = getVersion()
            do! sh "Src/Core" $"dotnet pack -c Release -p:Version={ver}"
        }

        "push" => action {
            do! need ["pack"]

            let! ver = getVersion()
            let! nuget_key_var = getEnv "NUGET_KEY"

            let package_name = $"NEbml.{ver}.nupkg"
            let nuget_key = nuget_key_var =? ""
            do! sh "Src/Core/bin/Release" $"dotnet nuget push {package_name} --api-key {nuget_key} --source https://api.nuget.org/v3/index.json"
        }
    ]

}
