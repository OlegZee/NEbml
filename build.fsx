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

let nuget_exe = "packages/NuGet.CommandLine/tools/NuGet.exe"
let DEF_VER = "0.0.1"

do xake {ExecOptions.Default with FileLog = "build.log"; ConLogLevel = Verbosity.Loud } {

    rules [
        "main"  <== ["build"; "test"; "nuget-pack"]

        "clean" => recipe {
            do! rm { dir "Src/**/bin" }
            do! rm { dir "Src/**/obj" }
        }

        "build" ..> recipe {
            let! ver = getEnv("VER")
            do! "Src/Core" |> shellx $"dotnet build -c Release -p:Version={ver =? DEF_VER}"
        }

        "test" ..> recipe {
            let! testFiles = !! "Src/Core.Tests/**/*.cs" |> getFiles
            do! needFiles testFiles

            do! shellx "dotnet test" "Src/Core.Tests"
        }

        "pack" ..> recipe {
            let! ver = getEnv("VER")
            do! "Src/Core" |> shellx $"dotnet pack -c Release -p:Version={ver =? DEF_VER}"
        }

        "nuget-push" => action {
            do! need ["nuget-pack"]

            let! ver = getEnv("VER")
            let package_name = sprintf "NEbml.Core.%s.nupkg" (ver =? DEF_VER)

            let! nuget_key = getEnv("NUGET_KEY")
            // let! exec_code = systemClr nuget_exe ["push"; "nupkg" </> package_name; nuget_key =? ""]
            // if exec_code <> 0 then failwith ""
            return ()
        }
    ]

}
