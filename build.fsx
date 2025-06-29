// xake build file

#r "nuget: Xake, 2.3.0"

open Xake
open Xake.Tasks


let (=?) value deflt = value |> Option.defaultValue deflt

let getVersion () = recipe {
    let! ver = getEnv "VERSION"
    return ver =? "0.0.1"
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
            do! sh $"dotnet build -c Release -p:Version={ver}" { workdir "Src/Core" }
        }

        "test" ..> recipe {
            do! dependsOn !! "Src/Core.Tests/**/*.cs"
            do! sh "dotnet test" { workdir "Src/Core.Tests" }
        }

        "pack" ..> recipe {
            let! ver = getVersion()
            do! sh $"dotnet pack -c Release -p:Version={ver}" { workdir "Src/Core" }
        }

        "push" => action {
            do! need ["pack"]

            let! ver = getVersion()
            let! nuget_key_var = getEnv "NUGET_KEY"

            let package_name = $"NEbml.{ver}.nupkg"
            let nuget_key = nuget_key_var =? ""
            do! sh "dotnet nuget push" {
                workdir "Src/Core/bin/Release"
                args [
                    package_name
                    "--api-key"; nuget_key
                    "--source"; "https://api.nuget.org/v3/index.json"
                ]
            }
        }
    ]
}
