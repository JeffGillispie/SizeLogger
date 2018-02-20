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
            
            if (FileUtil.IsFolder(target))
            {
                logger.ProcessFolder(target);
            }
            else
            {
                logger.ProcessFolderList(target);
            }
        }        
    }
}
