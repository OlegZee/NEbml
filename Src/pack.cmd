rem msbuild NEbml.sln /p:Configuration=Release /t:Clean,Build

del /Q /F nupackage\lib\*.*
copy Core\bin\Release\NEbml.Core.* nupackage\lib

cd nupackage
..\.nuget\NuGet.exe pack Nebml.nuspec
cd ..