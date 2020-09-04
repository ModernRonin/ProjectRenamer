# projectrename

## Summary
How often have you felt the need to rename a C# project? If you have come here, then you know that the most important existing IDE for C#, Visual Studio, does not really support this scenario very well.

This tool takes care of this for you, provided your use-case follows a set of fairly common practices:
* you use **git** as a repository
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

## Use it
You use it from the command line, in the directory of your solution:

```shell
dotnet renameproject <oldProjectName> <newProjectName>
```

The project names include neither path nor extension (.csproj). *renameproject* will find your project just by the name, no matter how deeply it might be hidden in your directory structure.
It must be linked into the solution, though.

Example usage:
```shell
dotnet renameproject ModernRonin.ProjectRenamer ModernRonin.RenameProject
```

What will happen:
* the project file will be renamed
* the folder of the project file will be renamed
* the renames use `git mv` so they keep your history intact
* all `<ProjectReference>` tags in other projects in your solution referencing the project will be adjusted
* if you use [paket](https://github.com/fsprojects/Paket) and you agree to a prompt, a `paket install` will be run
* all changes will be staged in git
* if you agree to the corresponding prompt, a `dotnet build` will be run to see whether anything worked well
* if you agree to another prompt, a commit of the form `Renamed <oldProjectName> to <newProjectName>` will be created automatically

If anything goes wrong, all changes will be discarded.

## Limitations
*renameproject* has a few limitations. Some of them are *hard limitations*, meaning they are unlikely to go away, others are *soft limitations*, meaning they exist only because I simply have not gotten round to fix them. I also 
do not really have a lot of free time to spend on this, but am totally open to PRs. 

### Hard
Your local repository copy must be clean. This is to ensure that in case we have to discard changes, we don't discard anything you wouldn't want discarded, by accident.
If *renameproject* detects uncommitted changes, added files or the like, it will abort its operation.


### Soft
* the prompts (build, paket and commit) cannot be avoided using command-line flags
* you cannot have more than one solution file or the solution file in another location than the current directory - could be turned into an optional command-line argument in the future
* you cannot use this without git - the git-aspects could be made optional via a command-line flag in the future
* you cannot use this with projects of other types than `csproj`, for example `fsproj`
* I have not tested this with old-style, pre-SDK `csproj` projects
* the detection of whether the local repo is clean might throw some false positives in some cases
* you cannot supply wildcards, like `dotnet renameproject ModernRonin.CommonServices.*`ModernRonin.Common.Services.*`
* you need to manually update the tool with `dotnet tool update -g ModernRonin.ProjectRenamer`


## License
The [license](./LICENSE) is [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). In essence this means you are free to use and distribute and change this tool however you see fit, as long as you provide a link to the license
and share any customizations/changes you might perform under the same license. 


