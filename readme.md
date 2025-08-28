![Icon](https://raw.githubusercontent.com/devlooped/runcs/main/assets/img/icon-32.png) dnx gist
============

[![Version](https://img.shields.io/nuget/vpre/gist.svg?color=royalblue)](https://www.nuget.org/packages/gist)
[![Downloads](https://img.shields.io/nuget/dt/gist.svg?color=green)](https://www.nuget.org/packages/gist)
[![License](https://img.shields.io/github/license/devlooped/runcs.svg?color=blue)](https://github.com/devlooped/runcs/blob/main/license.txt)
[![Build](https://github.com/devlooped/runcs/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/runcs/actions/workflows/build.yml)

## dnx gist
<!-- #gist -->
Run C# code programs from GitHub gists.

```
Usage: [dnx] gist GIST_REF [args]
    GIST_REF  Reference to gist file to run, with format owner/gist[@commit][:path]
              @commit optional gist commit (default: default branch)
              :path optional path to file in gist (default: program.cs or first .cs file)

              Examples:
              * kzu/0ac826dc7de666546aaedd38e5965381                 (tip commit and program.cs or first .cs file)
              * kzu/0ac826dc7de666546aaedd38e5965381@d8079cf:run.cs  (explicit commit and file path)

    args      Arguments to pass to the C# gist program
```

> [!TIP]
> The gist does not need to be public. In that case, the same authentication 
> used by your local `git` will be used to access the gist, via the Git Credential Manager.

<!-- #gist -->

## dnx runcs
<!-- #runcs -->
Run C# code programs from git repos on GitHub, GitLab, Bitbucket and Azure DevOps.

```
Usage: [dnx] runcs REPO_REF [args]
    REPO_REF  Reference to remote file to run, with format [host/]owner/repo[@ref][:path]
              host optional host name (default: github.com)
              @ref optional branch, tag, or commit (default: default branch)
              :path optional path to file in repo (default: program.cs at repo root)

              Examples:
              * kzu/sandbox@v1.0.0:run.cs           (implied host github.com, explicit tag and file path)
              * gitlab.com/kzu/sandbox@main:run.cs  (all explicit parts)
              * bitbucket.org/kzu/sandbox           (implied ref as default branch and path as program.cs)
              * kzu/sandbox                         (implied host github.com, ref and path defaults)

    args      Arguments to pass to the C# program
```

<!-- #runcs -->

# Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/gist/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.[run]-pr*`[NUMBER]`
- Branch builds: *42.42.[run]-*`[BRANCH]`

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->