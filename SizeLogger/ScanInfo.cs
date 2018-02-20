using System;
using Alphaleonis.Win32.Filesystem;

namespace SizeLogger
{
    public class ScanInfo
    {
        private string errorMessage = string.Empty;
        private long size = 0;
        private string path = null;
        private bool hasError = false;
        private bool isFolder = false;

        public ScanInfo(string path, bool isFolder)
        {
            this.path = path;
            this.isFolder = isFolder;
        }

        public ScanInfo(FileInfo file)
        {
            this.isFolder = false;
            this.path = file.FullName;            
        }

        public ScanInfo(DirectoryInfo dir)
        {
            this.isFolder = true;
            this.path = dir.FullName;
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }            
        }

        public long Size
        {
            get
            {
                return size;
            }            
        }

        public string Path
        {
            get
            {
                return path;
            }
        }

        public bool HasError
        {
            get
            {
                return hasError;
            }
        }

        public bool IsFolder
        {
            get
            {
                return isFolder;
            }
        }

        public void SetErrorMessage(string message)
        {
            hasError = true;
            errorMessage = message;
        }

        public void SetSize(long size)
        {
            this.size = size;
        }

        public long AddSize(long size)
        {
            this.size += size;
            return this.size;
        }
    }
}
