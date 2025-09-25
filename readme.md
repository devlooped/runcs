![Icon](https://raw.githubusercontent.com/devlooped/runcs/main/assets/img/icon-32.png) dnx runcs
============

[![Version](https://img.shields.io/nuget/vpre/runcs.svg?color=royalblue)](https://www.nuget.org/packages/runcs)
[![Downloads](https://img.shields.io/nuget/dt/runcs.svg?color=green)](https://www.nuget.org/packages/runcs)
[![License](https://img.shields.io/github/license/devlooped/runcs.svg?color=blue)](https://github.com/devlooped/runcs/blob/main/license.txt)
[![Build](https://github.com/devlooped/runcs/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/runcs/actions/workflows/build.yml)

## dnx runcs
<!-- #runcs -->
Run C# code programs from git repos on GitHub, GitLab and Azure DevOps.

```
Usage:
    [dnx] runcs [--aot] [--alias ALIAS] <repoRef> [<appArgs>...]

Arguments:
    <REPO_REF>  Reference to remote file to run, with format [host/]owner/repo[@ref][:path]
                host optional host name ([gist.]github.com|gitlab.com|dev.azure.com, default: github.com)
                @ref optional branch, tag, or commit (default: default branch)
                :path optional path to file in repo (default: program.cs at repo root)

                Examples:
                * kzu/sandbox@v1.0.0:run.cs           (implied host github.com, explicit tag and file path)
                * gitlab.com/kzu/sandbox@main:run.cs  (all explicit parts)
                * kzu/sandbox                         (implied host github.com, ref and path defaults)

                Can be an alias previously set with --alias.

    <appArgs>   Arguments passed to the C# program that is being run.

Options:
    --aot         (optional) Enable dotnet AOT defaults for run file.cs. Defaults to false.
    --alias ALIAS (optional) Assign an alias on first usage which can be used instead of the full ref.
```

Example:

```
dnx runcs kzu/runcs@v1:run.cs dotnet rocks
```

View [source](https://github.com/kzu/runcs/blob/v1/run.cs):

```csharp
#:package Spectre.Console@*

using Spectre.Console;

AnsiConsole.MarkupLine($"Hello world from [green]dnx[/] [yellow]gist[/] :rocket: [bold italic]{string.Join(' ', args)}[/]");
```

> [!TIP]
> The repo does not need to be public. In that case, the same authentication 
> used by your local `git` will be used to access the gist, via the Git Credential Manager.

When running different files from the same repo+ref, the download will be performed only once.
The last download etag is used to avoid downloading on each run.

<!-- #runcs -->

## dnx gist
<!-- #gist -->
Run C# code programs from GitHub gists.

```
Usage: [dnx] gist [--aot] [--alias ALIAS] <gistRef> [<appArgs>...]

Arguments:
    <GIST_REF>  Reference to gist file to run, with format owner/gist[@commit][:path]
                @commit optional gist commit (default: latest)
                :path optional path to file in gist (default: program.cs or first .cs file)

                Examples:
                * kzu/0ac826dc7de666546aaedd38e5965381                 (tip commit and program.cs or first .cs file)
                * kzu/0ac826dc7de666546aaedd38e5965381@d8079cf:run.cs  (explicit commit and file path)

                Can be an alias previously set with --alias.

    <appArgs>   Arguments passed to the C# program that is being run.

Options:
    --aot         (optional) Enable dotnet AOT defaults for run file.cs. Defaults to false.
    --alias ALIAS (optional) Assign an alias on first usage which can be used instead of the full ref.
```

> [!TIP]
> The gist does not need to be public. In that case, the same authentication 
> used by your local `git` will be used to access the gist, via the Git Credential Manager.

Example:

```
dnx gist kzu/52b115ce24c7978ddc33245d4ff840f5 dotnet rocks
```

View [source gist](https://gist.github.com/kzu/52b115ce24c7978ddc33245d4ff840f5):

```csharp
#:package Spectre.Console@*

using Spectre.Console;

AnsiConsole.MarkupLine($"Hello world from [green]dnx[/] [yellow]gist[/] :rocket: [bold italic]{string.Join(' ', args)}[/]");
```

When running different files from the same repo+ref, the download will be performed only once.
The last download etag is used to avoid downloading on each run.

<!-- #gist -->

# Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/gist/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.[run]-pr*`[NUMBER]`
- Branch builds: *42.42.[run]-*`[BRANCH]`

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jwendl.png "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/adalon.png "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Eule02.png "Michael Hagedorn")](https://github.com/Eule02)
[![torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek.png "torutek")](https://github.com/torutek)
[![Ryan McCaffery](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mccaffers.png "Ryan McCaffery")](https://github.com/mccaffers)
[![Alex Wiese](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/alexwiese.png "Alex Wiese")](https://github.com/alexwiese)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
