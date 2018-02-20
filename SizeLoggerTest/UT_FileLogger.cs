using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SizeLogger;
using Alphaleonis.Win32.Filesystem;

namespace SizeLoggerTest
{
    [TestClass]
    public class UT_FileLogger
    {
        [TestMethod]
        public void Test_ProcessFolder()
        {
            string path = @"x:\bak";
            Stopwatch timer = new Stopwatch();
            
            timer.Reset();
            FileScanner scanner = new FileScanner();
            DirectoryInfo dir = new DirectoryInfo(path);
            timer.Start();
            scanner.GetDirectorySize(dir);
            timer.Stop();
            Debug.WriteLine($"File Scanner Seconds: {timer.Elapsed.TotalSeconds:0.000}");

            timer.Reset();
            FileLogger logger = new FileLogger();
            timer.Start();
            logger.ProcessFolder(path);
            timer.Stop();
            Debug.WriteLine($"File Logger Seconds: {timer.Elapsed.TotalSeconds:0.000}");
        }
    }
}
