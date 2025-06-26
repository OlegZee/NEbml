# README #

### What is this repository for? ###

NEbml provides facility to read/write EBML binary format. The idea of EBML is similar to XML, as it is:

 * made of tagged records
 * stores both atomic and compaund objects

Unlike XML the EBML is very efficient in space and performance terms.
Library tends to be compliant to Ebml specs, as they described in https://github.com/ietf-wg-cellar/ebml-specification.

### How do I get set up? ###

* reference `NEbml` package from nuget
* Package contains binaries for .NET 4.6.1 and NetStandard 2.0 platforms

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### Prerequisites

 * dotnet 8 SDK

### Publishing new version (note to myself)

```bash
# build and test a package
dotnet fsi build.fsx -- -- clean build test

# set env vars:
export VER=0.9.0
export NUGET_KEY=111112222233333

# pack and push the package
dotnet fsi build.fsx -- -- push
```