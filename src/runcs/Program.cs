using Devlooped;
using Spectre.Console;

if (args.Length == 0 || !FileRef.TryParse(args[0], out var location))
{
    AnsiConsole.MarkupLine($"Usage: [grey][[dnx]][/] [lime]{ThisAssembly.Project.ToolCommandName}[/] [italic]FILE_REF[/] [grey][[args]][/]");
    AnsiConsole.MarkupLine("""
            [bold]FILE_REF[/]  Reference to remote file to run, with format [yellow][[host/]]owner/repo[[@ref]][[:path]][/]
                      [italic][yellow]host[/][/] optional host name (default: github.com)
                      [italic][yellow]@ref[/][/] optional branch, tag, or commit (default: default branch)
                      [italic][yellow]:path[/][/] optional path to file in repo (default: program.cs at repo root)

                      Examples: 
                      * kzu/sandbox@v1.0.0:run.cs           (implied host github.com, explicit tag and file path)
                      * gitlab.com/kzu/sandbox@main:run.cs  (all explicit parts)
                      * bitbucket.org/kzu/sandbox           (implied ref as default branch and path as program.cs)
                      * kzu/sandbox                         (implied host github.com, ref and path defaults)
        """);
    return;
}


AnsiConsole.WriteLine(location.ToString() ?? "");
