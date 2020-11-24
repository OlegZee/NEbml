# README #

### What is this repository for? ###

NEbml provides facility to read/write EBML binary format. The idea of EBML is similar to XML, as it is:

 * made of tagged records
 * stores both atomic and compaund objects

Unlike XML the EBML is very efficient in space and performance terms.

### How do I get set up? ###

* reference `NEbml` package from nuget
* Package contains binaries for .NET 4.6.1 and NetStandard 2.0 platforms

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### Publishing new version (note to myself)

```cmd
REM build and test a package
dotnet fsi build.fsx -- clean build test

REM set env vars:
set VER=0.9.0
set NUGET_KEY=111112222233333

REM pack and push the package
dotnet fsi build.fsx -- push
```