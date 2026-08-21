[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](osmfeula.txt)
[![OSS](https://img.shields.io/github/license/devlooped/oss.svg?color=blue)](license.txt) 
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/runfile)

<!-- include ../../readme.md#gist -->
<!-- #gist -->
Run C# code programs from GitHub gists.

```
Usage: [dnx] gist [OPTIONS] <gistRef> [<appArgs>...]

Arguments:
    <GIST_REF>  Reference to gist file to run, with format owner/gist[@commit][:path]
                @commit optional gist commit (default: latest)
                :path optional path to file in gist (default: program.cs or first .cs file)

                Examples:
                * kzu/0ac826dc7de666546aaedd38e5965381                 (tip commit and program.cs or first .cs file)
                * kzu/0ac826dc7de666546aaedd38e5965381@d8079cf:run.cs  (explicit commit and file path)

                Can be an alias previously set with --dnx-alias.

    <appArgs>   Arguments passed to the C# program that is being run.

Options:
    --dnx-aot         Enable dotnet AOT defaults for run file.cs. Defaults to false.
    --dnx-alias ALIAS Assign an alias on first usage which can be used instead of the full ref.
    --dnx-debug       Launch the debugger before running.
    --dnx-force       Force download, skipping ETag checking.
```

> [!TIP]
> The gist does not need to be public. Private repos will leverage the same authentication 
> used by your local `git` to access the file, via the Git Credential Manager.

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
<!-- ../../readme.md#gist -->

## Aliases

<!-- include ../../readme.md#aliases -->
<!-- #aliases -->

`runfile` and `gist` share the same alias store, so a name created with one tool works with the other.
Aliases are stored in your global `.netconfig` file (`~/.netconfig` on Linux/macOS, `%USERPROFILE%\.netconfig` on Windows).

### Creating an alias

Pass `--dnx-alias` (or `--alias`) when running a ref:

```
dnx TOOL REF --dnx-alias NAME
```

Examples:

```
dnx runfile kzu/sandbox@main:run.cs --dnx-alias sandbox
dnx gist kzu/0ac826dc7de666546aaedd38e5965381 --dnx-alias demo
```

After that, use the alias instead of the full ref:

```
dnx TOOL NAME
```

Re-running with the same alias name updates the ref it points to.

### Managing aliases

```
[dnx] TOOL alias
[dnx] TOOL alias delete NAME
[dnx] TOOL alias rename OLD NEW
```

`TOOL` is `runfile` or `gist`.

| Command | Description |
| --- | --- |
| `alias` | List all configured aliases and the refs they point to. Refs link to the source on the web; `github.com/` is omitted when it is the default host. |
| `alias delete NAME` | Remove an alias. |
| `alias rename OLD NEW` | Rename an alias, keeping the same ref. Fails if `NEW` already exists. |

Running a tool with no arguments shows the usage help followed by the alias table when any aliases are configured.

<!-- #aliases -->
<!-- ../../readme.md#aliases -->
<!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->
## Open Source Maintenance Fee

To ensure the long-term sustainability of this project, users of this package who generate 
revenue must pay an [Open Source Maintenance Fee](https://opensourcemaintenancefee.org). 
While the source code is freely available under the terms of the [License](license.txt), 
this package and other aspects of the project require [adherence to the Maintenance Fee](osmfeula.txt).

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/devlooped) at the proper 
OSMF tier. A single fee covers all of [Devlooped packages](https://www.nuget.org/profiles/Devlooped).

<!-- https://github.com/devlooped/.github/raw/main/osmf.md -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![Ryan McCaffery](https://avatars.githubusercontent.com/u/16667079?u=c0daa64bb5c1b572130e05ae2b6f609ecc912d4d&v=4&s=39 "Ryan McCaffery")](https://github.com/mccaffers)
[![Seika Logiciel](https://avatars.githubusercontent.com/u/2564602?v=4&s=39 "Seika Logiciel")](https://github.com/SeikaLogiciel)
[![Andrew Grant](https://avatars.githubusercontent.com/devlooped-user?s=39 "Andrew Grant")](https://github.com/wizardness)
[![eska-gmbh](https://avatars.githubusercontent.com/devlooped-team?s=39 "eska-gmbh")](https://github.com/eska-gmbh)
[![Geodata AS](https://avatars.githubusercontent.com/u/5946299?v=4&s=39 "Geodata AS")](https://github.com/geodata-no)
[![Jiri Slachta](https://avatars.githubusercontent.com/u/6891947?u=802cfeb13b070d04c53269fc662b0d58963480dd&v=4&s=39 "Jiri Slachta")](https://github.com/jslachta)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
