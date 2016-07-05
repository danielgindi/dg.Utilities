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
        [Obsolete]
        public static string[] DANGEROUS_EXTENSIONS
        {
            get
            {
                return FileHelper.DANGEROUS_EXTENSIONS;
            }
        }

        [Obsolete]
        public static string AquireUploadFileName(
            string fileName,
            string folder, string subFolder,
            bool appendDateTime,
            bool stripUnicodeFileNames,
            bool renameDangerousExtensions
            )
        {
            return FileHelper.AquireUploadFileName(
                fileName,
                folder, subFolder,
                appendDateTime,
                stripUnicodeFileNames,
                renameDangerousExtensions);
        }

        [Obsolete]
        public static string SaveFile(HttpPostedFile postedFile, string folder)
        {
            return FileHelper.SaveFile(postedFile, folder);
        }

        [Obsolete]
        public static string SaveFile(HttpPostedFile postedFile, string folder, bool appendDateTime)
        {
            return FileHelper.SaveFile(postedFile, folder, appendDateTime);
        }

        [Obsolete]
        public static string SaveFile(HttpPostedFile postedFile, string folder, string subFolder)
        {
            return FileHelper.SaveFile(postedFile, folder, subFolder);
        }

        [Obsolete]
        public static string SaveFile(HttpPostedFile postedFile, string folder, string subFolder, bool appendDateTime)
        {
            return FileHelper.SaveFile(postedFile, folder, subFolder, appendDateTime);
        }

        [Obsolete]
        public static string MapPath(string path)
        {
            return FileHelper.MapPath(path);
        }

        [Obsolete]
        public static string CreateEmptyTempFile()
        {
            return FileHelper.CreateEmptyTempFile();
        }

        [Obsolete]
        public static void ResetFilePermissionsToInherited(string filePath)
        {
            FileHelper.ResetFilePermissionsToInherited(filePath);
        }

        [Obsolete]
        public class TemporaryFileDeleter : FileHelper.TemporaryFileDeleter
        {
            public TemporaryFileDeleter(string localFilePath) : base(localFilePath)
            {
            }
        }
    }
}
