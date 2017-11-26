//
// SecureDeleteExtensions.cs
// This file is part of Microsoft.WinAny.Helper library
//
// Author: Josip Habjan (habjan@gmail.com, http://www.linkedin.com/in/habjan) 
// Copyright (c) 2013 Josip Habjan. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace VolumEraser
{

    #region OverwriteAlgorithm

    public enum OverwriteAlgorithm : int
    {
        /// <summary>
        /// 3 passes.
        /// This method is based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (DoD 5220.22-M E).  
        /// It will overwrite a file 3 times.  This method offers medium security, use it only on files that do not contain sensitive information.
        /// </summary>
        DoD_3 = 4,
        /// <summary>
        /// 7 passes.
        /// This method is based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (US DoD 5220.22-M ECE).  
        /// It will overwrite a file 7 times.  This method incorporates the DoD-3 method.  It is secure and should be used for general files.
        /// </summary>
        DoD_7 = 8,
    }

    #endregion

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random _random;

        /// <summary>
        /// Represents a thread safe pseudo-random number generator, a device that produces a sequence
        /// of numbers that meet certain statistical requirements for randomness.
        /// </summary>
        public static Random @Random
        {
            get { return _random ?? (_random = new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId)); }
        }

        /// <summary>
        /// Randomize list element order using Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">List to shuffle.</param>
        public static void Shuffle<T>(IList<T> list)
        {
            for (int pass = list.Count - 1; pass > 1; pass--)
            {
                int index = ThreadSafeRandom.Random.Next(pass + 1);
                T value = list[index];
                list[index] = list[pass];
                list[pass] = value;
            }
        }
    }

    public static class SecureDeleteExtensions
    {
        #region Private constants

        private const int MAX_BUFFER_SIZE = 67108864;

        #endregion

        #region Delete - DirectoryInfo

        /// <summary>
        /// Safe delete this directory, subdirectories and all the files under this directory.
        /// </summary>
        /// <param name="directory">The DirectoryInfo.</param>
        /// <param name="overwriteAlgorithm">Overwrite algorithm.</param>
        public static void Delete(this DirectoryInfo directory, OverwriteAlgorithm overwriteAlgorithm)
        {
            FileInfo[] files = directory.GetFiles();

            foreach (FileInfo file in files)
            {
                file.Delete(overwriteAlgorithm);
            }

            DirectoryInfo[] subDirectories = directory.GetDirectories();

            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                subDirectory.Delete(overwriteAlgorithm);
            }
             
            directory.Delete();
        }

        #endregion

        #region Delete - FileInfo

        public static void Delete(this FileInfo file, OverwriteAlgorithm overwriteAlgorithm)
        {

            if ((overwriteAlgorithm & OverwriteAlgorithm.DoD_7) == OverwriteAlgorithm.DoD_7)
            {
                OverwriteFile_DoD_7(file);
            }

            if ((overwriteAlgorithm & OverwriteAlgorithm.DoD_3) == OverwriteAlgorithm.DoD_3)
            {
                OverwriteFile_DoD_3(file);
            }

            file.Delete();
        }

        #endregion
 
        #region OverwriteFile_DoD_3

        /// <summary>
        /// Overwrite the file based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (DoD 5220.22-M E).
        /// </summary>
        /// <param name="file">The file.</param>
        internal static void OverwriteFile_DoD_3(FileInfo file)
        {
            byte[] pattern = new byte[] { 0x00, 0xFF, 0x72 };

            ThreadSafeRandom.Shuffle<byte>(pattern);

            Random random = ThreadSafeRandom.Random;

            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None);

            for (int pass = 0; pass <= 2; ++pass)
            {
                fs.Position = 0;

                for (long size = fs.Length; size > 0; size -= MAX_BUFFER_SIZE)
                {
                    long bufferSize = (size < MAX_BUFFER_SIZE) ? size : MAX_BUFFER_SIZE;

                    byte[] buffer = new byte[bufferSize];

                    if (pass != 2)
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

                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush(true);
                }
            }

            fs.Close(); fs.Dispose(); fs = null;
        }

        #endregion

        #region OverwriteFile_DoD_7

        /// <summary>
        /// Overwrite the file based on the U.S. Department of Defense's standard 'National Industrial Security Program Operating Manual' (US DoD 5220.22-M ECE).
        /// </summary>
        /// <param name="file">The file.</param>
        internal static void OverwriteFile_DoD_7(FileInfo file)
        {
            byte[] pattern = new byte[] { 0x00, 0xFF, 0x72, 0x96, 0x00, 0xFF, 0x72 };

            ThreadSafeRandom.Shuffle<byte>(pattern);

            Random random = ThreadSafeRandom.Random;

            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None);

            for (int pass = 1; pass <= 7; ++pass)
            {
                fs.Position = 0;

                for (long size = fs.Length; size > 0; size -= MAX_BUFFER_SIZE)
                {
                    long bufferSize = (size < MAX_BUFFER_SIZE) ? size : MAX_BUFFER_SIZE;

                    byte[] buffer = new byte[bufferSize];

                    if (pass != 2 && pass != 6)
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

                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush(true);
                }
            }

            fs.Close(); fs.Dispose(); fs = null;
        }

        #endregion
        
    }
}
