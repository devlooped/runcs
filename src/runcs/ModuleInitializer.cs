using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Devlooped;

static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        var file = "git";
        if (OperatingSystem.IsWindows())
            file += ".exe";
        var path = Path.Combine(AppContext.BaseDirectory, file);

        if (!File.Exists(path))
            File.WriteAllBytes(path, []);

        // Sets fake git path to avoid requiring a full git installation
        Environment.SetEnvironmentVariable(GitCredentialManager.Constants.EnvironmentVariables.GitExecutablePath, AppContext.BaseDirectory);
    }
}
