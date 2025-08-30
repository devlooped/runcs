namespace Devlooped;

static class DirectoryExtensions
{
    extension(Directory)
    {
        /// <summary>Creates a temporary user-owned subdirectory for file-based apps.</summary>
        public static string CreateUserDirectory(string path)
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
}
