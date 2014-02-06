using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.AccessControl;

namespace dg.Utilities
{
    public static class Files
    {
        public static readonly string[] DANGEROUS_EXTENSIONS = new string[] 
            { 
                // Possibly malicious files, they can execute server side.
                ".aspx", ".asp", ".ascx", // ASP/ASP.NET
                ".php", ".php3", ".php4", ".phtml", // PHP
                ".jsp", // Java
                ".py", ".pyc", ".pyd", ".pyw", // Python
                ".pl", ".cgi", // Perl

                // Possibly malicious files, they can be viruses.
                ".exe", ".vbs", ".js", ".com", ".bat" 
            };

        /// <summary>
        /// Cleanup filenames from invalid characters and unauthorized characters
        /// </summary>
        /// <param name="fileName">Raw file name</param>
        /// <returns>Clean and secure file name!</returns>
        internal static string CleanupFileName(string fileName)
        {
            // Replace dots with underscores, except the extension
            fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

            // Remove \ / : * ? " < > |
            return Regex.Replace(fileName, @"[\\/:*?""<>|\p{C}]", "_", RegexOptions.None);
        }

        static public string AquireUploadFileName(
            string fileName,
            string folder, string subFolder,
            bool bAppendDateTime,
            bool StripUnicodeFileNames,
            bool RenameDangerousExtensions
            )
        {
            fileName = CleanupFileName(System.IO.Path.GetFileName(fileName));
            if (RenameDangerousExtensions)
            {
                foreach (string ext in DANGEROUS_EXTENSIONS)
                {
                    if (fileName.EndsWith(ext.StartsWith(".") ? ext : ('.' + ext), StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += @".unsafe";
                        break;
                    }
                }
            }

            if (subFolder != null)
            {
                if (subFolder.Length == 0) subFolder = null;
                else subFolder = subFolder.Trim('/', '\\');
            }
            folder = folder.Trim('/', '\\');

            if (folder.Contains(@"/"))
            {
                folder += @"/";
                if (subFolder != null) subFolder += @"/";
            }
            else
            {
                folder += @"\";
                if (subFolder != null) subFolder += @"\";
            }

            if (subFolder != null) folder += subFolder;

            if (HttpContext.Current != null)
            {
                if (folder.Length > 0&&(folder[0] == '~' || folder[0] == '/'))
                {
                    folder = HttpContext.Current.Server.MapPath(folder);
                }
            }
            else
            {
                folder = Files.MapPath(folder);
            }

            if (StripUnicodeFileNames)
            {
                char[] chars = fileName.ToCharArray();
                fileName = string.Empty;
                foreach (char c in chars)
                {
                    if (c <= 127 && c != 37/*%*/ && c != 38/*&*/) fileName += ((c == 32/* */ || c == 39/*'*/) ? ((char)95/*_*/) : c);
                }
            }
            else fileName = fileName.Replace("'", "_").Replace("%", "").Replace("&", "");
            fileName = fileName.Trim();

            if (fileName.Length == 0 || fileName.StartsWith(".")) bAppendDateTime = true;

            if (bAppendDateTime)
            {
                fileName = DateTime.UtcNow.ToString(@"yyyy_MM_dd_hh_mm_ss", DateTimeFormatInfo.InvariantInfo) + ((fileName.Length > 0 && !fileName.StartsWith(@".")) ? @"_" + fileName : fileName);
            }

            int iTries = 0;
            string strFile = Path.GetFileNameWithoutExtension(fileName);
            string strFileExt = Path.GetExtension(fileName);
            fileName = Path.GetFileName(fileName);
            string strFilePath = folder + fileName;
            while (System.IO.File.Exists(strFilePath))
            {
                if (!strFile.EndsWith(@"_")) strFile += @"_";
                fileName = strFile + iTries.ToString() + strFileExt;
                strFilePath = folder + fileName;
                iTries++;
            }
            return strFilePath;
        }

        /// <summary>
        /// Saves an uploaded file to the server.
        /// Takes care of dangerous extensions, and unicode filenames.
        /// Takes care of creating sub folders.
        /// </summary>
        /// <param name="postedFile">uploaded file</param>
        /// <param name="folder">target folder</param>
        /// <returns>file name without path, or null</returns>
        static public string SaveFile(HttpPostedFile postedFile, string folder)
        {
            return SaveFile(postedFile, folder, null, false);
        }

        /// <summary>
        /// Saves an uploaded file to the server.
        /// Takes care of dangerous extensions, and unicode filenames.
        /// Takes care of creating sub folders.
        /// </summary>
        /// <param name="postedFile">uploaded file</param>
        /// <param name="folder">target folder</param>
        /// <param name="appendDateTime">append date/time signature to file</param>
        /// <returns>file name without path, or null</returns>
        static public string SaveFile(HttpPostedFile postedFile, string folder, bool appendDateTime)
        {
            return SaveFile(postedFile, folder, null, appendDateTime);
        }

        /// <summary>
        /// Saves an uploaded file to the server.
        /// Takes care of dangerous extensions, and unicode filenames.
        /// Takes care of creating sub folders.
        /// </summary>
        /// <param name="postedFile">uploaded file</param>
        /// <param name="strFolder">target folder</param>
        /// <param name="subFolder">sub folder to append to target folder</param>
        /// <returns>file name without path, or null</returns>
        static public string SaveFile(HttpPostedFile postedFile, string folder, string subFolder)
        {
            return SaveFile(postedFile, folder, subFolder, false);
        }

        /// <summary>
        /// Saves an uploaded file to the server.
        /// Takes care of dangerous extensions, and unicode filenames.
        /// Takes care of creating sub folders.
        /// </summary>
        /// <param name="postedFile">uploaded file</param>
        /// <param name="folder">target folder</param>
        /// <param name="subFolder">sub folder to append to target folder</param>
        /// <param name="appendDateTime">append date/time signature to file</param>
        /// <returns>file name without path, or null</returns>
        static public string SaveFile(HttpPostedFile postedFile, string folder, string subFolder, bool appendDateTime)
        {
            string strFilePath = AquireUploadFileName(postedFile.FileName, folder, subFolder, appendDateTime, true, true);

            string dirPath = System.IO.Path.GetDirectoryName(strFilePath);
            if (dirPath != null)
            {
                if (!System.IO.Directory.Exists(dirPath))
                {
                    Folders.CreateDirectory(dirPath, false);
                }
            }

            try
            {
                postedFile.SaveAs(strFilePath);
                return Path.GetFileName(strFilePath);
            }
            catch { }
            return null;
        }

        public static string MapPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else if (HostingEnvironment.IsHosted)
            {
                return HostingEnvironment.MapPath(path);
            }
            else if (VirtualPathUtility.IsAppRelative(path))
            {
                string physicalPath = VirtualPathUtility.ToAbsolute(path, "/");
                physicalPath = physicalPath.Replace('/', '\\');
                physicalPath = physicalPath.Substring(1);
                physicalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, physicalPath);

                return physicalPath;
            }
            else
            {
                throw new Exception("Could not resolve non-rooted path.");
            }
        }

        /// <summary>
        /// Creates an empty file in the TEMP folder.
        /// Note that you might want to reset the file's permissions after moving, because it has the permissions of the TEMP folder.
        /// </summary>
        /// <returns>Path to the temp file that was created</returns>
        public static string CreateEmptyTempFile()
        {
            string tempFilePath = Folders.GetTempDir() + Guid.NewGuid().ToString() + @".tmp";
            FileStream fs = null;
            while (true)
            {
                try
                {
                    fs = new FileStream(tempFilePath, FileMode.CreateNew);
                    break;
                }
                catch (IOException ioex)
                {
                    Console.WriteLine(@"Utility.File.CreateEmptyTempFile - Error: {0}", ioex.ToString());
                    if (System.IO.File.Exists(tempFilePath))
                    { // File exists, make up another name
                        tempFilePath = Folders.GetTempDir() + Guid.NewGuid().ToString() + @".tmp";
                    }
                    else
                    { // Another error, throw it back up
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Utility.File.CreateEmptyTempFile - Error: {0}", ex.ToString());
                    break;
                }
            }
            if (fs != null)
            {
                fs.Dispose();
                fs = null;
                return tempFilePath;
            }
            return null;
        }

        /// <summary>
        /// Reset the file's permissions to it's parent folder's permissions
        /// </summary>
        /// <param name="filePath">Path to the target file</param>
        public static void ResetFilePermissionsToInherited(string filePath)
        {
            FileSecurity fileSecurity = File.GetAccessControl(filePath);
            fileSecurity.SetAccessRuleProtection(false, true);
            foreach (FileSystemAccessRule rule in fileSecurity.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount)))
            {
                fileSecurity.RemoveAccessRule(rule);
            }
            File.SetAccessControl(filePath, fileSecurity);
        }

        public class TemporaryFileDeleter : IDisposable
        {
            private string _LocalFilePath;

            public string LocalFilePath
            {
                get { return _LocalFilePath; }
            }
            public TemporaryFileDeleter(string localFilePath)
            {
                _LocalFilePath = localFilePath;
            }

            ~TemporaryFileDeleter()
            {
                Dispose(false);
            }

            public void DeleteFile()
            {
                string path = _LocalFilePath;
                _LocalFilePath = null;
                if (path == null) return;
                System.IO.File.Delete(path);
            }
            public void DoNotDelete()
            {
                _LocalFilePath = null;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                }
                // Now clean up Native Resources (Pointers)
                try
                {
                    DeleteFile();
                }
                catch { }
            }
        }
    }
}
