using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;

namespace dg.Utilities
{
    public static class Folders
    {
        public static DirectoryInfo CreateDirectory(string path, bool failOnExists)
        {
            return FolderHelper.CreateDirectory(path, failOnExists);
        }
        
        public static bool VerifyDirectoryExists(string path)
        {
            return FolderHelper.VerifyDirectoryExists(path);
        }

        public static string GetTempDir()
        {
            return FolderHelper.GetTempDir();
        }
    }
}
