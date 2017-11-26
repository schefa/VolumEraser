using System;
using System.IO;
using System.Windows;
using VolumEraser.Models;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace VolumEraser.Controller
{
    public class VolumeController
    {
        #region Attributes
        private const int MAX_BUFFER_SIZE = 10 * 1024 * 1024;
        protected static Action EmptyDelegate = delegate () { };
        #endregion

        /// <summary>
        /// Deletes a volume
        /// </summary>
        /// <param name="selectedVolume"></param>
        public static void deleteVolume(Models.Volume selectedVolume)
        {
            try
            {
                // Get the root directory and print out some information about it.
                DirectoryInfo dirInfo = selectedVolume.RootDirectory;

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
        /// Method to purge a volume
        /// Based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (DoD 5220.22-M ECE).
        /// </summary>
        /// <param name="selectedVolume"></param> 
        public static async Task eraseVolume( CancellationToken ct, Volume selectedVolume)
        {
            // Variables
            var dummyFilesCount = Math.Floor((double)selectedVolume.AvailableFreeSpace / MAX_BUFFER_SIZE);
            byte[] pattern = new byte[] { 0x00, 0xFF, 0x72, 0x96, 0x00, 0xFF, 0x72 };
            ThreadSafeRandom.Shuffle<byte>(pattern); 
            Random random = ThreadSafeRandom.Random;

            // Set progressbar
            MainWindow.PGBar.Maximum = selectedVolume.AvailableFreeSpace;
            MainWindow.PGBar.Value = 0;

            // available free space will be filled with random files 
            for (int i = 1; i <= dummyFilesCount; i++)
            {
                // Write asynchronous to implement cancel button
                using (FileStream fs = File.Create(selectedVolume.Name + i, MAX_BUFFER_SIZE, FileOptions.Asynchronous))
                {
                    // Loop 7 times (DoD 5220.22-M ECE)
                    for (int pass = 0; pass <= 6; ++pass)
                    {
                        fs.Position = 0;

                        if (ct.IsCancellationRequested) {
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

                        // Create file
                        await fs.WriteAsync(buffer, 0, buffer.Length, ct);

                        // delegate event for progressbar
                        MainWindow.PGBar.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Background);
                        MainWindow.LabelProgress.Dispatcher.Invoke(EmptyDelegate, DispatcherPriority.Background);
                        fs.Flush(true);
                    }

                    // Increase progressbar
                    MainWindow.LabelProgress.Content = Math.Round (i / dummyFilesCount, 3) * 100 + " %";
                    MainWindow.PGBar.Value += MAX_BUFFER_SIZE; 
                } 
            }

            // After everything is done, delete everything again
            VolumeController.deleteVolume(selectedVolume);
        }

    }
}
