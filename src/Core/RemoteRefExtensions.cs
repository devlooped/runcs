using System.Runtime.InteropServices;

namespace Devlooped;

public static class RemoteRefExtensions
{
    extension(RemoteRef location)
    {
        public string TempPath => Path.Join(GetTempRoot(), location.Host ?? "github.com", location.Owner, location.Project ?? "", location.Repo, location.Ref ?? "main");

        public string EnsureTempPath() => Directory.CreateUserDirectory(location.TempPath);
    }

    /// <summary>Obtains the temporary directory root, e.g., <c>/tmp/dotnet/runcs/</c>.</summary>
    static string GetTempRoot()
    {
        // We want a location where permissions are expected to be restricted to the current user.
        string directory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.GetTempPath()
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Directory.CreateUserDirectory(Path.Join(directory, "dotnet", "runcs"));
    }

    /// <summary>Obtains a specific temporary path in a subdirectory of the temp root, e.g., <c>/tmp/dotnet/runcs/{name}</c>.</summary>
    public static string GetTempSubpath(params string[] name) => Directory.CreateUserDirectory(Path.Join([GetTempRoot(), .. name]));
}
