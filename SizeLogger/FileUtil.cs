using System;
using Alphaleonis.Win32.Filesystem;

namespace SizeLogger
{
    public static class FileUtil
    {
        public static bool IsFolder(string path)
        {
            bool isFolder = false;

            if (File.Exists(path) || Directory.Exists(path))
            {
                var attributes = File.GetAttributes(path);

                if ((attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    isFolder = true;
                }
            }
            else
            {
                throw new Exception($"The target item ({path}) does not exist.");
            }

            return isFolder;
        }
    }
}
