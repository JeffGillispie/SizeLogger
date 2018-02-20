using System;
using System.Collections.Generic;
using System.Linq;

namespace SizeLogger
{
    class Program
    {
        static void Main(string[] args)
        {            
            string target = args.First();            
            FileLogger logger = new FileLogger();
            logger.Log(target);

            /*
            if (args.Length > 1 && args.First().ToUpper().Equals("TEST"))
            {
                var dir = new Alphaleonis.Win32.Filesystem.DirectoryInfo(args.Last());
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                Scanner scanner = new Scanner();
                Console.WriteLine(String.Format("Starting test scan at {0} for {1}.", DateTime.Now, args.Last()));
                timer.Start();                
                long size = scanner.GetFolderSizeTest(dir);
                timer.Stop();
                Console.WriteLine("Scan Complete");
                Console.WriteLine("Size = " + size);
                Console.WriteLine("Duration = " + timer.Elapsed.TotalMinutes + " minutes");
            }
            else
            {
                string target = args.First();
                Logger logger = new Logger();
                logger.Log(target);
            }
            */
        }        
    }
}
