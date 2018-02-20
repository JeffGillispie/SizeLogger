using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks.Dataflow;
using Alphaleonis.Win32.Filesystem;
using NLog;

namespace SizeLogger
{
    public class Logger
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public void Log(string path)
        {
            string[] locations = System.IO.File.ReadAllLines(path);
            Console.WriteLine(String.Format("Initiating processing of {0} locations.", locations.Length));
            int counter = 0;

            foreach (string location in locations)
            {
                counter++;
                ProcessPath(location);
                Console.WriteLine(String.Format(
                    "Location {0} or {1} has been processed.{2}", 
                    counter, locations.Length, Environment.NewLine));
                logger.Info(location);
            }

            Console.WriteLine();
            Console.WriteLine("Processing Completed!");
        }

        private void ProcessPath(string path)
        {
            ConsoleSpinner spinner = new ConsoleSpinner();
            Console.WriteLine("Scanning Items: " + path);
            int batchSize = Properties.Settings.Default.SqlInsertBufferSize;
            var producer = new BatchBlock<ScanInfo>(batchSize);
            var consumer = new ActionBlock<ScanInfo[]>(items => {
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

        private void PostItems(ITargetBlock<ScanInfo> target, string path)
        {
            Scanner scanner = new Scanner();

            scanner.ItemScanned += ((sender, info) => {
                target.Post(info);
            });

            DirectoryInfo dir = new DirectoryInfo(path);
            long size = scanner.GetFolderSize(dir);
            //long size = scanner.GetFolderSize(path);
        }

        private void LogItems(IEnumerable<ScanInfo> scannedItems)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder() {
                IntegratedSecurity = true,
                DataSource = Properties.Settings.Default.SqlServerName,
                InitialCatalog = Properties.Settings.Default.SqlDatabaseName,
                ConnectTimeout = Properties.Settings.Default.SqlConnectionTimeout
            };

            string connectionSettings = builder.ConnectionString;
            string query = Properties.Resources.InsertScanInfo;

            using (SqlConnection connection = new SqlConnection(connectionSettings))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();

                    foreach (ScanInfo scannedItem in scannedItems)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@path", scannedItem.Path);
                        command.Parameters.AddWithValue("@size", scannedItem.Size);
                        command.Parameters.AddWithValue("@isFolder", scannedItem.IsFolder);
                        command.Parameters.AddWithValue("@errorMessage", scannedItem.ErrorMessage);
                        command.Parameters.AddWithValue("@dateRecorded", DateTime.Now);                        
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }                
    }        
}
