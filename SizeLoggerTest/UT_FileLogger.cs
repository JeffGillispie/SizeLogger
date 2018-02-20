using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
            logger.ProcessFolder(path, 1000);
            timer.Stop();
            Debug.WriteLine($"File Logger Seconds - Queue 1000: {timer.Elapsed.TotalSeconds:0.000}");

            timer.Reset();                        
            timer.Start();
            logger.ProcessFolder(path, 100);
            timer.Stop();
            Debug.WriteLine($"File Logger Seconds - Queue 100: {timer.Elapsed.TotalSeconds:0.000}");

            timer.Reset();
            timer.Start();
            FileLogger.ProcessTest(path);
            timer.Stop();
            Debug.WriteLine($"Process Test Seconds: {timer.Elapsed.TotalSeconds:0.000}");
        }

        [TestMethod]
        public async Task TestProcessor()
        {
            string path = @"x:\bak";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var queue = new BufferBlock<object>();
            FileLogger.PostItems(queue, path);
            var consumer = FileLogger.Consume(queue);
            await Task.WhenAll(consumer, queue.Completion);
            timer.Stop();
            Debug.WriteLine($"Test Processor Seconds: {timer.Elapsed.TotalSeconds:0.000}");
        }                
    }
}
