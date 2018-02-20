using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Alphaleonis.Win32.Filesystem;
using NLog;

namespace SizeLogger
{
    public class FileLogger
    {
        private static Logger fileLogger = LogManager.GetLogger("fileLogger");
        private static Logger folderLogger = LogManager.GetLogger("folderLogger");
        private Dictionary<Type, int> lookup = new Dictionary<Type, int>() {
            { typeof(FileInfo), 0 },
            { typeof(ScanInfo), 1 }
        };

        public void ProcessFolderList(string path)
        {
            string[] locations = System.IO.File.ReadAllLines(path);
            Console.WriteLine(String.Format("Initiating processing of {0} locations.", locations.Length));
            int counter = 0;

            foreach (string location in locations)
            {
                counter++;
                ProcessFolder(location);
                Console.WriteLine(String.Format(
                    "Location {0} or {1} has been processed.{2}",
                    counter, locations.Length, Environment.NewLine));                
            }

            Console.WriteLine();
            Console.WriteLine("Processing Completed!");
        }

        public void ProcessFolder(string path)
        {
            ProcessFolder(path, Properties.Settings.Default.SqlInsertBufferSize);            
        }

        public void ProcessFolder(string path, int batchSize)
        {
            ConsoleSpinner spinner = new ConsoleSpinner();
            Console.WriteLine("Scanning Items: " + path);            
            var producer = new BatchBlock<object>(batchSize);
            var consumer = new ActionBlock<object[]>(items => {
                LogItems(items);
                spinner.Turn();
            });
            producer.LinkTo(consumer);
            producer.Completion.ContinueWith(delegate { consumer.Complete(); });
            PostItems(producer, path);
            producer.Complete();
            consumer.Completion.Wait();
            Console.WriteLine("Completed: All scanned items have been logged.");
        }

        public static void ProcessTest(string path)
        {
            FileScanner scanner = new FileScanner();

            scanner.ItemScanned += ((sender, info) => {
                folderLogger.Info($"{info.Path}|{info.Size}|1|{info.ErrorMessage}|{DateTime.Now}");
            });

            scanner.FileScanned += ((sender, info) => {
                long size = 0;
                string err = String.Empty;

                try
                {
                    size = info.Length;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }

                fileLogger.Info($"{info.FullName}|{size}|0|{err}|{DateTime.Now}");
            });

            DirectoryInfo dir = new DirectoryInfo(path);
            scanner.GetDirectorySize(dir);
        }

        public static void PostItems(ITargetBlock<object> target, string path)
        {
            FileScanner scanner = new FileScanner();

            scanner.ItemScanned += ((sender, info) => {
                target.Post(info);
            });

            scanner.FileScanned += ((sender, info) => {
                target.Post(info);
            });

            DirectoryInfo dir = new DirectoryInfo(path);
            scanner.GetDirectorySize(dir);            
        }

        public static async Task Consume(BufferBlock<object> queue)
        {
            while (await queue.OutputAvailableAsync())
            {
                object item = await queue.ReceiveAsync();

                if (item.GetType().Equals(typeof(ScanInfo)))
                {
                    ScanInfo info = item as ScanInfo;
                    folderLogger.Info($"{info.Path}|{info.Size}|1|{info.ErrorMessage}|{DateTime.Now}");
                }
                else
                {
                    FileInfo file = item as FileInfo;
                    long size = 0;
                    string err = String.Empty;

                    try
                    {
                        size = file.Length;
                    }
                    catch (Exception ex)
                    {
                        err = ex.Message;
                    }

                    fileLogger.Info($"{file.FullName}|{size}|0|{err}|{DateTime.Now}");
                }
            }                        
        }

        private void LogItems(IEnumerable<object> scannedItems)
        {
            var typeGroups = scannedItems.ToLookup(i => i.GetType());

            foreach (ScanInfo info in typeGroups[typeof(ScanInfo)])
            {
                folderLogger.Info($"{info.Path}|{info.Size}|1|{info.ErrorMessage}|{DateTime.Now}");
            }
            
            foreach (FileInfo file in typeGroups[typeof(FileInfo)])
            {
                long size = 0;
                string err = String.Empty;

                try
                {
                    size = file.Length;
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }

                fileLogger.Info($"{file.FullName}|{size}|0|{err}|{DateTime.Now}");
            }
        }
    }
}
