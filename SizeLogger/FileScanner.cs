using System;
using System.Linq;
using Alphaleonis.Win32.Filesystem;

namespace SizeLogger
{
    public class FileScanner
    {
        public long GetDirectorySize(DirectoryInfo dir)
        {
            ScanInfo folderInfo = new ScanInfo(dir);

            try
            {
                folderInfo.AddSize(dir.EnumerateFiles().Sum(file => {
                    OnScan(file);
                    return file.Length;
                }));

                folderInfo.AddSize(dir.EnumerateDirectories().Sum(folder => {
                    return GetDirectorySize(folder);
                }));
            }
            catch (Exception ex)
            {
                folderInfo.SetErrorMessage(ex.Message);
            }

            OnScan(folderInfo);
            return folderInfo.Size;
        }

        public event EventHandler<ScanInfo> ItemScanned;
        public event EventHandler<FileInfo> FileScanned;

        protected virtual void OnScan(ScanInfo info)
        {
            ItemScanned?.Invoke(this, info);
        }

        protected virtual void OnScan(FileInfo info)
        {
            FileScanned?.Invoke(this, info);
        }
    }
}
