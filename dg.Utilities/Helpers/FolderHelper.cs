using System;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;

namespace dg.Utilities
{
    public static class FolderHelper
    {
        // Do a cleanup of the folder name to avoid possible problems
        /// <summary>
        /// Cleanup the folder name from invalid characters
        /// </summary>
        /// <param name="folderName">Raw folder name</param>
        /// <returns>Clean folder name</returns>
        internal static string CleanupFolderName(string folderName)
        {
            // Remove \ / : * ? " < > |
            return Regex.Replace(folderName, @"[\\/:*?""<>|\p{C}]", "_", RegexOptions.None);
        }

        // The "_mkdir" function is used by the "CreateDirectory" method.
        [DllImport("msvcrt.dll", SetLastError = true)]
        private static extern int _mkdir(string path);

        /// <summary>
        /// This method should provide safe substitute for Directory.CreateDirectory().
        /// </summary>
        /// <param name="path">The directory path to be created.</param>
        /// <returns>A <see cref="System.IO.DirectoryInfo"/> object for the created directory.</returns>
        /// <remarks>
        ///		<para>
        ///		This method creates all the directory structure if needed.
        ///		</para>
        ///		<para>
        ///		The System.IO.Directory.CreateDirectory() method has a bug that gives an
        ///		error when trying to create a directory and the user has no rights defined
        ///		in one of its parent directories. The CreateDirectory() should be a good 
        ///		replacement to solve this problem.
        ///		</para>
        /// </remarks>
        public static DirectoryInfo CreateDirectory(string path, bool failOnExists)
        {
            // Create the directory info object for that dirInfo (normalized to its absolute representation).
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(path));

            try
            {
                if (dirInfo.Exists)
                {
                    if (failOnExists) throw new IOException(@"Folder already exists: " + dirInfo.FullName);
                }
                else dirInfo.Create();
                return dirInfo;
            }
            catch
            {
                CreateDirectoryUsingDll(dirInfo, failOnExists);
                return new DirectoryInfo(path);
            }
        }

        private static void CreateDirectoryUsingDll(DirectoryInfo dirInfo, bool failOnExists)
        {
            // On some occasion, the DirectoryInfo.Create() function will 
            // throw an error due to a bug in the .Net Framework design. For
            // example, it may happen that the user has no permissions to
            // list entries in a lower level in the directory path, and the
            // Create() call will simply fail.
            // To workaround it, we use _mkdir directly.

            ArrayList oDirsToCreate = new ArrayList();

            // Check the entire path structure to find directories that must be created.
            while (dirInfo != null && !dirInfo.Exists)
            {
                oDirsToCreate.Add(dirInfo.FullName);
                dirInfo = dirInfo.Parent;
            }

            // "dirInfo == null" means that the check arrives in the root and it doesn't exist too.
            if (dirInfo == null)
                throw (new System.IO.DirectoryNotFoundException("Directory \"" + oDirsToCreate[oDirsToCreate.Count - 1] + "\" not found."));

            // Create all directories that must be created (from bottom to top).
            for (int i = oDirsToCreate.Count - 1; i >= 0; i--)
            {
                string sPath = (string)oDirsToCreate[i];
                int iReturn = _mkdir(sPath);

                if (iReturn != 0)
                    throw new ApplicationException("Error calling [msvcrt.dll]:_wmkdir(" + sPath + "), error code: " + iReturn);
            }

            if (oDirsToCreate.Count == 0 && failOnExists) throw new IOException(@"Folder already exists: " + dirInfo.FullName);
        }

        public static bool VerifyDirectoryExists(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                if (HttpContext.Current != null)
                {
                    path = HttpContext.Current.Server.MapPath(path);
                }
                else
                {
                    path = FileHelper.MapPath(path);
                }
            }
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    try
                    {
                        CreateDirectory(path, false);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string GetTempDir()
        {
            string path = null;
            try
            {
                path = System.IO.Path.GetTempPath();
            }
            catch
            {
                try
                {
                    // Fallback 1
                    path = Environment.GetEnvironmentVariable("TEMP");
                    if (path == null || path.Length == 0)
                    {
                        // Fallback 2
                        path = Environment.GetEnvironmentVariable("TMP");
                    }
                    if (path == null || path.Length == 0)
                    {
                        // Fallback 3
                        path = Environment.GetEnvironmentVariable("WINDIR");
                        if (path != null && path.Length > 0)
                        {
                            path = Path.Combine(path, @"TEMP");
                        }
                    }
                }
                catch
                {
                }
            }

            if (path == null || path.Length == 0)
            {
                // Fallback 4
                path = HttpContext.Current.Server.MapPath(@"~/temp/dg.Utilities");
            }

            if (!path.EndsWith(@"/") && !path.EndsWith(@"\"))
            {
                if (path.IndexOf('/') > -1) path += '/';
                else path += '\\';
            }

            if (VerifyDirectoryExists(path)) return path;

            throw new UnauthorizedAccessException(@"Cannot access TEMP folder!");
        }
    }
}
