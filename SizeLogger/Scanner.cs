using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;

namespace SizeLogger
{
    public class Scanner
    {   
        public long GetFileSize(string path)
        {
            long size = 0;
            ScanInfo info = new ScanInfo(path, false);
                        
            try
            {
                size = File.GetSize(path);                            
            }
            catch (Exception ex)
            {
                info.SetErrorMessage(ex.Message);                
            }

            info.SetSize(size);            
            OnScan(info);
            return size;
        }

        public long GetFolderSizeTest(DirectoryInfo dir)
        {
            long size = 0;

            try
            {
                size += dir.EnumerateFiles().Sum(file => file.Length);
                size += dir.EnumerateDirectories().Sum(folder => GetFolderSizeTest(folder));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + dir.FullName);
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                Console.WriteLine("continuing scan...");
            }

            return size;
        }
                
        public long GetFolderSize(DirectoryInfo dir)
        {            
            ScanInfo folderInfo = new ScanInfo(dir.FullName, true);

            try
            {
                folderInfo.AddSize(dir.EnumerateFiles().Sum(file =>
                {
                    ScanInfo fileInfo = new ScanInfo(file);

                    try
                    {
                        fileInfo.SetSize(file.Length);
                    }
                    catch (Exception ex)
                    {
                        fileInfo.SetErrorMessage(ex.Message);
                    }

                    OnScan(fileInfo);
                    return fileInfo.Size;
                }));

                folderInfo.AddSize(dir.EnumerateDirectories().Sum(folder =>
                {
                    return GetFolderSize(folder);
                }));
            }
            catch (Exception ex)
            {
                folderInfo.SetErrorMessage(ex.Message);
            }

            OnScan(folderInfo);
            return folderInfo.Size;
        }
                                
        public long GetFolderSize(string path)
        {
            long size = 0;
            ScanInfo info = new ScanInfo(path, true);
            IEnumerable<string> files = new List<string>();
            IEnumerable<string> folders = new List<string>();

            try
            {
                files = Directory.GetFiles(path);
                folders = Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                info.SetErrorMessage(ex.Message);                
            }
                        
            Parallel.ForEach(files, file => {
                long fileSize = GetFileSize(file);
                Interlocked.Add(ref size, fileSize);
            });

            Parallel.ForEach(folders, folder => {
                long folderSize = GetFolderSize(folder);
                Interlocked.Add(ref size, folderSize);
            });

            info.SetSize(size);                        
            OnScan(info);
            return size;
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
