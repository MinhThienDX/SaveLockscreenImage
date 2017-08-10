using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualBasic.FileIO;

namespace SaveLockscreenImage
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                string source = appSettings.Get("sourceFolder");

                // If source folder is empty, guess it then save it
                if (string.IsNullOrEmpty(source))
                {
                    string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string assetPath = appSettings.Get("assetPath");
                    source = Path.Combine(appDataLocal, assetPath);

                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings.Remove("sourceFolder");
                    config.AppSettings.Settings.Add("sourceFolder", source);
                    config.Save(ConfigurationSaveMode.Modified);
                }

                string dest = appSettings.Get("destFolder");
                bool isErr = !Directory.Exists(source);

                if (isErr)
                {
                    Console.WriteLine("Source folder does not exist");
                }
                else
                {
                    isErr = !Directory.Exists(dest);

                    if (isErr)
                    {
                        Console.WriteLine("Destination folder does not exist");
                    }
                    else
                    {
                        long minFileSize;
                        long.TryParse(appSettings.Get("minFileSizeInByte"), out minFileSize);
                        // Copy files
                        string tempFolderPath = CopyAsset(source, dest, minFileSize);

                        int minImageWidth;
                        int.TryParse(appSettings.Get("minImageWidth"), out minImageWidth);
                        // Move copied files
                        SaveImage(tempFolderPath, minImageWidth);

                        // Delete duplicate files
                        string deleteDuplicate = appSettings.Get("deleteDuplicate");
                        if (deleteDuplicate.Equals("true"))
                        {
                            DeleteDuplicateImage(dest);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("BUG happened");
                Console.WriteLine("Start of exception");
                Console.WriteLine("=================================================================================================");
                Console.WriteLine(e);
                Console.WriteLine("=================================================================================================");
                Console.WriteLine("End of exception");
            }
            finally
            {
                Console.WriteLine("Would you kindly press Enter to exit");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Delete duplicate images in destination folder
        /// </summary>
        /// <param name="dest">Destination folder's path</param>
        private static void DeleteDuplicateImage(string dest)
        {
            MD5 md5 = MD5.Create();
            IEnumerable<FileInfo> orderedFiles = Directory.EnumerateFiles(dest).Select(path => new FileInfo(path)).OrderBy(file => file.Length);
            IList<FileInfo> fileList = orderedFiles as IList<FileInfo> ?? orderedFiles.ToList();
            int startIndex = fileList.Count - 1;
            int fileCount = 0;

            for (int i = startIndex; i > 0; i--)
            {
                FileInfo prevFile = fileList.ElementAt(i - 1);
                FileInfo nextFile = fileList.ElementAt(i);

                // When 2 files 1 size
                if (prevFile.Length == nextFile.Length)
                {
                    string deletePath = null;

                    // Get 2 hash
                    FileStream prevStream = File.OpenRead(prevFile.FullName);
                    FileStream nextStream = File.OpenRead(nextFile.FullName);
                    byte[] prevHash = md5.ComputeHash(prevStream);
                    byte[] nextHash = md5.ComputeHash(nextStream);
                    prevStream.Dispose();
                    nextStream.Dispose();

                    // When 2 files 1 hash
                    if (prevHash.SequenceEqual(nextHash))
                    {
                        // Keep the new file, delete older file
                        if (prevFile.CreationTime > nextFile.CreationTime)
                        {
                            deletePath = nextFile.FullName;
                        }
                        else
                        {
                            deletePath = prevFile.FullName;
                        }
                    }

                    if (!string.IsNullOrEmpty(deletePath))
                    {
                        FileSystem.DeleteFile(deletePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                        fileCount++;
                    }
                }
            }

            Console.WriteLine("Deleted " + fileCount + " duplicate image(s) in destination folder");
        }

        /// <summary>
        /// Save images to destination folder
        /// </summary>
        /// <param name="tempFolderPath">Temp folder's path</param>
        /// <param name="minImageWidth">Minimum image's width to process</param>
        private static void SaveImage(string tempFolderPath, int minImageWidth)
        {
            string[] fileList = Directory.GetFiles(tempFolderPath);
            string parentFolderPath = new DirectoryInfo(tempFolderPath).Parent.FullName;
            int fileCount = 0;

            foreach (string s in fileList)
            {
                Image img = Image.FromFile(s);

                // Check image's width
                if (img.Width >= minImageWidth)
                {
                    img.Dispose();
                    string newFilePath = Path.Combine(parentFolderPath, s.Substring(s.LastIndexOf("\\", StringComparison.Ordinal) + 1));

                    if (!File.Exists(newFilePath))
                    {
                        File.Move(s, newFilePath);
                        fileCount++;
                    }
                }
                else
                {
                    img.Dispose();
                }
            }

            // Delete temp folder
            Directory.Delete(tempFolderPath, true);
            Console.WriteLine("Finish save " + fileCount + " image(s) to destination folder");
        }

        /// <summary>
        /// Copy files from asset folder to temp folder
        /// </summary>
        /// <param name="source">Source folder's path</param>
        /// <param name="dest">Destination folder's path</param>
        /// <param name="minFileSize">Minimum filesize to copy</param>
        /// <returns>Temp folder's path</returns>
        private static string CopyAsset(string source, string dest, long minFileSize)
        {
            DirectoryInfo destFolder = new DirectoryInfo(dest);

            // Create temp folder inside destination folder
            DirectoryInfo tempFolder = destFolder.CreateSubdirectory(DateTime.Now.Ticks.ToString());
            string tempFolderPath = tempFolder.FullName;
            string[] fileList = Directory.GetFiles(source);
            int fileCount = 0;

            // Copy files to temp folder
            foreach (string s in fileList)
            {
                FileInfo file = new FileInfo(s);

                if (file.Length > minFileSize)
                {
                    string tmp = Path.Combine(tempFolderPath, file.Name + ".jpg");
                    File.Copy(s, tmp);
                    fileCount++;
                }
            }

            Console.WriteLine("Finish copy " + fileCount + " file(s) from asset folder");
            return tempFolderPath;
        }
    }
}
