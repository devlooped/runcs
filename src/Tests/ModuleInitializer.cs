using System.Runtime.CompilerServices;
using GitCredentialManager.UI;

namespace Devlooped;

static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Dispatcher.Initialize();

        var file = "git";
        if (OperatingSystem.IsWindows())
            file += ".exe";
        var path = Path.Combine(AppContext.BaseDirectory, file);

        if (!File.Exists(path))
            File.WriteAllBytes(path, []);

        // Sets fake git path to ensure we're not inadvertently requiring a full git installation
        Environment.SetEnvironmentVariable("GIT_EXEC_PATH", AppContext.BaseDirectory);
    }
}
