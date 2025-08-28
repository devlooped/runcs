using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Devlooped;

class DownloadManager
{
    /// <summary>
    /// Obtains the temporary directory root, e.g., <c>/tmp/dotnet/runcs/</c>.
    /// </summary>
    public static string GetTempRoot()
    {
        // We want a location where permissions are expected to be restricted to the current user.
        string directory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.GetTempPath()
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return CreateTempPath(Path.Join(directory, "dotnet", "runcs"));
    }

    /// <summary>
    /// Obtains a specific temporary path in a subdirectory of the temp root, e.g., <c>/tmp/dotnet/runcs/{name}</c>.
    /// </summary>
    public static string GetTempSubpath(params string[] name) => CreateTempPath(Path.Join([GetTempRoot(), .. name]));

    /// <summary>
    /// Creates a temporary subdirectory for file-based apps.
    /// Use <see cref="GetTempSubpath"/> to obtain the path.
    /// </summary>
    static string CreateTempPath(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            // Ensure only the current user has access to the directory to avoid leaking the program to other users.
            // We don't mind that permissions might be different if the directory already exists,
            // since it's under user's local directory and its path should be unique.
            Directory.CreateDirectory(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return path;
    }
}
