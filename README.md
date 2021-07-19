# renameproject
![CI Status](https://github.com/ModernRonin/ProjectRenamer/actions/workflows/dotnet.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/ModernRonin.ProjectRenamer.svg)](https://www.nuget.org/packages/ModernRonin.ProjectRenamer/)
[![NuGet](https://img.shields.io/nuget/dt/ModernRonin.ProjectRenamer.svg)](https://www.nuget.org/packages/ModernRonin.ProjectRenamer)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) 
## Summary
How often have you felt the need to rename or move a C# project? If you have come here, then you know that the most important existing IDE for C#, Visual Studio,
does not really support this scenario very well.

This tool takes care of this for you, provided your use-case follows a set of fairly common practices:
* you use **git** as a repository and have `git`(the executable) on your PATH
* your `csproj` files have the same name as the folder in which they reside together with accompanying source code
* you don't have more than one solution file (`.sln`) in one directory
* you have **dotnetcore 3.1** or above (note that once net6 is out, this tool will be switched to require net5; by that time, I assume widespread enough adoption)
* your solution does not contain nested solution folders - the tool currently has an issue with that and will fail; until I find the time to fix that, the 
workaround is simply to move the nested solution folder to top-level via VS, run the tool, and then move the solution folder back;seeing as this is two 
simple drag-and-drops that only change the solution file, I hope this is acceptable.


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

When I publish a new version, I always post at [my blog](https://modernronin.github.io/) under the [renameproject tag](https://modernronin.github.io/tags/renameproject/), aside from updating this readme here.

### Release History
2.1.5:
* bugfix: tool can be used on *nix platforms now without crashing; thanks to [@pranav-ninja](https://github.com/pranav-ninja) for the PR!

2.1.4:
* bugfix: fixed issue when paths contained whitespace; thanks to [@jakubmaguza](https://github.com/jakubmaguza) for the PR - the first contribution from anyone else :-)

2.1.3:
* bugfix: fixed a bug concerning nested solution folders; thanks to [@Mike-E-angelo](https://github.com/mike-e-angelo) for reporting the bug

2.1.2:
* bugfix: fixed another whitespace related scenario; thanks to [@sejohnson-at-griffis](https://github.com/sejohnson-at-griffis) for reporting the bug

2.1.1:
* bugfix: projects in paths containing whitespace no longer crash the dotnet commands; thanks to [@NicolasRiou](https://github.com/NicolasRiou) for reporting the bug

2.1.0:
* feature: you can move projects to different folders now instead of just renaming them
* feature: you can specify a directory to exclude from project reference updates
* feature: the detected VS solution folder is displayed in review
* feature: the detected git version is displayed in review
* bugfix: when called with unnamed arguments, old project name now is understood to come before new project name (before it was the wrong way round)
* bugfix: VS solution folders containing spaces don't crash the tool anymore

2.0.0: 
* breaking change: instead of asking the user interactively, behavior is now controlled via commandline switches

1.0.1: 
* bugfix: if a project is not in a solution folder, the tools works now, too
* bugfix: if a required tool like git cannot be found, give a proper error message

1.0.0: initial release

## Use it
You use it from the command line, in the directory of your solution:

```shell
renameproject <oldProjectName> <newProjectName>
```

The project names include neither path nor extension (.csproj). *renameproject* will find your project just by the name, no matter how deeply it might be hidden in your directory structure.
It must be linked into the solution, though.

### Simple rename
Example usage:
```shell
renameproject ModernRonin.ProjectRenamer ModernRonin.RenameProject
```

What will happen:
* the project file will be renamed
* the folder of the project file will be renamed
* renaming is done with `git mv` so it keeps your history intact
* all `<ProjectReference>` tags in other projects in your solution referencing the project will be adjusted
* if you use [paket](https://github.com/fsprojects/Paket) **as a local dotnet tool** (see [Soft Limitations](#soft-limitations)), `paket install` will be run, unless you specified the flag `--no-paket`
* all changes will be staged in git
* if you specified a flag `--build`, a `dotnet build` will be run just to be totally safe that everything worked well, for very cautious/diligent people :-)
* a commit of the form `Renamed <oldProjectName> to <newProjectName>` will be created automatically, unless you specified a flag `--no-commit`

If anything goes wrong, all changes will be discarded.

### Move
Since version 2.1.0, the tool also allows you to move projects. To do this, you prefix the new name with a relative folder. 

Here's an example:
```shell
renameproject ModernRonin.ProjectRenamer src/ModernRonin.ProjectRenamer
```

If you want to move a project from somewhere in a subfolder into the root of the solution, prefix the new name with `./`.

For example, to revert the change from the previous example you'd do:
```shell
renameproject ModernRonin.ProjectRenamer ./ModernRonin.ProjectRenamer
```

### Rename and Move combined
You can also move and rename in one operation like
```shell
renameproject ModernRonin.ProjectRenamer src/ModernRonin.RenameProject
```

>However, there is a **caveat**: git interprets this not as rename, but as delete and create and thus you will loose the history of your project file. Thus, I recommend to do such things in two passes.

### Exclude Directory
In some situations, for example if your repository contains a separate solution with separate projects in a subdirectory, you want to exclude a directory completely from being looked at by the project reference update mechanism. In that case, you can specify that directory with the optional `--exclude` argument.

### Help
For details about available arguments and flags/options and some example calls, you can also use 
```shell
renameproject help
```
to get help about the available flags.

## Limitations
*renameproject* has a few limitations. Some of them are *hard limitations*, meaning they are unlikely to go away, others are *soft limitations*, meaning they exist only because I simply have not gotten round to fix them yet. I  
do not really have a lot of free time to spend on this, but am **totally open to PRs (hint hint)**. 

### Hard Limitations
* Your local repository copy must be clean. This is to ensure that in case we have to discard changes, we don't discard anything you wouldn't want discarded, by accident.
If *renameproject* detects uncommitted changes, added files or the like, it will abort its operation.
* the tool won't adjust your namespaces - just use R# for this.
* I have not tested this with old-style, pre-SDK `csproj` projects and I very likely never will

### Soft Limitations
* you cannot have more than one solution file or the solution file in another location than the current directory - could be turned into an optional command-line argument in the future
* you cannot use this without git - the git-aspects could be made optional via a command-line flag in the future
* you cannot use this with projects of other types than `csproj`, for example `fsproj`
* the detection of whether the local repo is clean might throw false positives in some cases
* you cannot use wildcards, like `renameproject ModernRonin.CommonServices.* ModernRonin.Common.Services.* ` - this would be very handy for the wide-spread convention to have an accompanying ` *.Tests ` project
* you need to manually update the tool with `dotnet tool update -g ModernRonin.ProjectRenamer` and you need to come here to check whether there is a new version (or check nuget)
* if you use paket as a global tool instead of as a local tool, paket support will fail currently - you should really switch to using paket as a local tool, if you can. but on the other hand, in the future *renameproject* might just become smarter about this using a combination of checking whether there is `paket` in the `PATH` and the presence of `dotnet-tools.json` and whether it contains an entry for paket


## License
The [license](./LICENSE) is [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). In essence this means you are free to use and distribute and change this tool however you see fit, as long as you provide a link to the license
and share any customizations/changes you might perform under the same license. 


