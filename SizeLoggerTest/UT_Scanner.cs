using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Alphaleonis.Win32.Filesystem;

namespace SizeLoggerTest
{
    [TestClass]
    public class UT_Scanner
    {
        private const string TEST_FOLDER_PATH = @"X:\bak";
        private const long TEST_FOLDER_SIZE = 12028992792;

        [TestMethod]
        public void GetFolderSize()
        {            
            List<string> results = new List<string>();            
            results.Add(GetSizeTestResult(1, (() => { return GetDirSizeMethod1(TEST_FOLDER_PATH); })));
            results.Add(GetSizeTestResult(2, (() => { return GetDirSizeMethod2(TEST_FOLDER_PATH); })));
            results.Add(GetSizeTestResult(3, (() => { return GetDirSizeMethod3(TEST_FOLDER_PATH); })));
            results.Add(GetSizeTestResult(4, (() => { return GetDirSizeMethod4(TEST_FOLDER_PATH); })));
            results.Add(GetSizeTestResult(5, (() => { return GetDirSizeMethod5(TEST_FOLDER_PATH); })));            
            results.ForEach(r => Debug.WriteLine(r));                
        }

        public string GetSizeTestResult(int id, Func<long> method)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            long size = method.Invoke();
            timer.Stop();
            return String.Format(
                "Method {0} = {1} in {2} seconds.", 
                id, 
                size, 
                timer.Elapsed.TotalSeconds.ToString("0.000"));
        }

        public long GetDirSizeMethod1(string path)
        {
            var dir = new DirectoryInfo(path);
            return dir
                .EnumerateFiles(DirectoryEnumerationOptions.Recursive)
                .Sum(f => f.Length);
        }

        public long GetDirSizeMethod2(string path)
        {
            long size = 0;
            Stack<DirectoryInfo> folders = new Stack<DirectoryInfo>();
            DirectoryInfo root = new DirectoryInfo(path);
            folders.Push(root);

            while (folders.Count > 0)
            {
                DirectoryInfo folder = folders.Pop();

                foreach (FileInfo file in folder.EnumerateFiles())
                {
                    size += file.Length;
                }

                foreach (DirectoryInfo dir in folder.EnumerateDirectories())
                {
                    folders.Push(dir);
                }
            }

            return size;
        }

        public long GetDirSizeMethod3(string path)
        {
            long size = 0;
            Stack<string> folders = new Stack<string>();
            folders.Push(path);

            while (folders.Count > 0)
            {
                string folder = folders.Pop();

                foreach (string file in Directory.EnumerateFiles(folder))
                {
                    size += File.GetSize(file);
                }

                foreach (string dir in Directory.EnumerateDirectories(folder))
                {
                    folders.Push(dir);
                }
            }

            return size;
        }

        public long GetDirSizeMethod4(string path)
        {
            long size = 0;
            Stack<DirectoryInfo> folders = new Stack<DirectoryInfo>();
            DirectoryInfo root = new DirectoryInfo(path);
            folders.Push(root);

            while (folders.Count > 0)
            {
                DirectoryInfo folder = folders.Pop();

                Parallel.ForEach(folder.EnumerateFiles(), file => {
                    Interlocked.Add(ref size, file.Length);
                });
                
                foreach (DirectoryInfo dir in folder.EnumerateDirectories())
                {
                    folders.Push(dir);
                }
            }

            return size;
        }

        public long GetDirSizeMethod5(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            long size = 0;            
            size += dir.EnumerateFiles().Sum(file => file.Length);
            size += dir.EnumerateDirectories().Sum(folder => GetDirSizeMethod5(folder.FullName));
            return size;
        }
    }
}
