# renameproject
[![NuGet](https://img.shields.io/nuget/v/ModernRonin.ProjectRenamer.svg)](https://www.nuget.org/packages/ModernRonin.ProjectRenamer/)
[![NuGet](https://img.shields.io/nuget/dt/ModernRonin.ProjectRenamer.svg)](https://www.nuget.org/packages/ModernRonin.ProjectRenamer)
## Summary
How often have you felt the need to rename a C# project? If you have come here, then you know that the most important existing IDE for C#, Visual Studio, does not really support this scenario very well.

This tool takes care of this for you, provided your use-case follows a set of fairly common practices:
* you use **git** as a repository and have `git`(the executable) on your PATH
* your `csproj` files have the same name as the folder in which they reside together with accompanying source code
* you don't have more than one solution file (`.sln`) in one directory
* you have **dotnetcore 3.1** or above


## Get it
*renameproject* is intended to be used as a global dotnet tool. 
(You could install it as a local tool, too, but given what it does this does not really make a lot of sense.)

You install it by executing:

```shell
dotnet tool install -g ModernRonin.ProjectRenamer
```

## Update it
If there is a new version out, you can update *renameproject* with

```shell
dotnet tool update --global ModernRonin.ProjectRenamer
```

### Release History
1.0.1: 
* fixed problem when target project was not linked into a solution folder
* when required tools like `git` or `dotnet` are not found, a properly informative error message is displayed

1.0.0: initial release



## Use it
You use it from the command line, in the directory of your solution:

```shell
renameproject <oldProjectName> <newProjectName>
```

The project names include neither path nor extension (.csproj). *renameproject* will find your project just by the name, no matter how deeply it might be hidden in your directory structure.
It must be linked into the solution, though.

Example usage:
```shell
renameproject ModernRonin.ProjectRenamer ModernRonin.RenameProject
```

What will happen:
* the project file will be renamed
* the folder of the project file will be renamed
* renaming is done with `git mv` so it keeps your history intact
* all `<ProjectReference>` tags in other projects in your solution referencing the project will be adjusted
* if you use [paket](https://github.com/fsprojects/Paket) **as a local dotnet tool** (see [Soft Limitations](#soft-limitations)) and you agree to a prompt, `paket install` will be run
* all changes will be staged in git
* if you agree to the corresponding prompt, a `dotnet build` will be run just to be totally safe that everything worked well, for very cautious/diligent people :-)
* if you agree to another prompt, a commit of the form `Renamed <oldProjectName> to <newProjectName>` will be created automatically

If anything goes wrong, all changes will be discarded.

## Limitations
*renameproject* has a few limitations. Some of them are *hard limitations*, meaning they are unlikely to go away, others are *soft limitations*, meaning they exist only because I simply have not gotten round to fix them yet. I  
do not really have a lot of free time to spend on this, but am **totally open to PRs (hint hint)**. 

### Hard Limitations
* Your local repository copy must be clean. This is to ensure that in case we have to discard changes, we don't discard anything you wouldn't want discarded, by accident.
If *renameproject* detects uncommitted changes, added files or the like, it will abort its operation.
* the tool won't adjust your namespaces - just use R# for this.

### Soft Limitations
* the prompts (build, paket and commit) cannot be avoided using command-line flags
* you cannot have more than one solution file or the solution file in another location than the current directory - could be turned into an optional command-line argument in the future
* you cannot use this without git - the git-aspects could be made optional via a command-line flag in the future
* you cannot use this with projects of other types than `csproj`, for example `fsproj`
* I have not tested this with old-style, pre-SDK `csproj` projects
* the detection of whether the local repo is clean might throw false positives in some cases
* you cannot use wildcards, like `renameproject ModernRonin.CommonServices.* ModernRonin.Common.Services.* ` - this would be very handy for the wide-spread convention to have an accompanying ` *.Tests ` project
* you need to manually update the tool with `dotnet tool update -g ModernRonin.ProjectRenamer` and you need to come here to check whether there is a new version (or check nuget)
* if you use paket as a global tool instead of as a local tool, paket support will fail currently - you should really switch to using paket as a local tool, if you can. but on the other hand, in the future *renameproject* 
might just become smarter about this using a combination of checking whether there is `paket` in the `PATH` and the presence of `dotnet-tools.json` and whether it contains an entry for paket


## License
The [license](./LICENSE) is [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). In essence this means you are free to use and distribute and change this tool however you see fit, as long as you provide a link to the license
and share any customizations/changes you might perform under the same license. 


