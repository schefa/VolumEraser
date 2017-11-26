using System;
using System.IO;
using System.Windows;
using VolumEraser.Models;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Text;

namespace VolumEraser.Controller
{
    public class HardDriveController
    {
        private const int MAX_BUFFER_SIZE = 10 * 1024 * 1024;
        protected static Action EmptyDelegate = delegate () { };

        /// <summary>
        /// Deletes all content from a drive
        /// </summary>
        /// <param name="selectedItem"></param>
        public static void deleteContent(Models.HardDrive selectedItem)
        {
            try
            {
                // Get the root directory and print out some information about it.
                DirectoryInfo dirInfo = selectedItem.RootDirectory;

                // Get the files in the directory and print out some information about them.
                FileInfo[] fileNames = dirInfo.GetFiles("*.*");
                
                foreach (var item in fileNames)
                {
                    File.Delete(item.FullName);
                }

                DirectoryInfo[] dirInfos = dirInfo.GetDirectories("*.*");

                foreach (DirectoryInfo item in dirInfos)
                {
                    Directory.Delete(item.FullName, true);
                }
                
            }
            catch (DirectoryNotFoundException dirNotFound)
            {
                MessageBox.Show(dirNotFound.Message);
            }
        }

        /// <summary>
        /// Method to create random dummy content
        /// Based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (DoD 5220.22-M ECE).
        /// </summary>
        /// <param name="selectedItem"></param> 
        public static async Task Delete( CancellationToken ct, HardDrive selectedItem)
        {
            var dummyFilesCount = Math.Floor((double)selectedItem.AvailableFreeSpace / MAX_BUFFER_SIZE);
            byte[] pattern = new byte[] { 0x00, 0xFF, 0x72, 0x96, 0x00, 0xFF, 0x72 };
             
            ThreadSafeRandom.Shuffle<byte>(pattern);

            Random random = ThreadSafeRandom.Random;
 
            MainWindow.PGBar.Maximum = selectedItem.AvailableFreeSpace;
            MainWindow.PGBar.Value = 0;
             
            for (int i = 1; i <= dummyFilesCount; i++)
            {
                using (FileStream fs = File.Create(selectedItem.Name + i, MAX_BUFFER_SIZE, FileOptions.Asynchronous))
                {

                    for (int pass = 0; pass <= 6; ++pass)
                    {
                        fs.Position = 0;

                        if (ct.IsCancellationRequested)
                        {
                            ct.ThrowIfCancellationRequested();
                            return;
                        }
                         
                        long bufferSize = MAX_BUFFER_SIZE;

                        byte[] buffer = new byte[bufferSize];

                        if (pass != 1 && pass != 5)
                        {
                            for (int bufferIndex = 0; bufferIndex < bufferSize; ++bufferIndex)
                            {
                                buffer[bufferIndex] = pattern[pass];
                            }
                        }
                        else
                        {
                            for (int bufferIndex = 0; bufferIndex < bufferSize; ++bufferIndex)
                            {
                                buffer[bufferIndex] = (byte)(random.Next() % 256);
                            }
                        }

                        await fs.WriteAsync(buffer, 0, buffer.Length, ct);
                        MainWindow.PGBar.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Background);
                        MainWindow.LabelProgress.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Background);
                        fs.Flush(true);

                    }

                    MainWindow.LabelProgress.Content = Math.Round (i / dummyFilesCount, 3) * 100 + " %";
                    MainWindow.PGBar.Value += MAX_BUFFER_SIZE;
                    
                }

            }

            // After everything is done, delete everything again
            HardDriveController.deleteContent(selectedItem);
        }

    }
}
