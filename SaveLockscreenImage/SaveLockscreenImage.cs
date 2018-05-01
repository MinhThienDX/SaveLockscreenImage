using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace SaveLockscreenImage
{
    /// <summary>
    /// This program save Windows 10's lockscreen image
    /// </summary>
    public class SaveLockscreenImage
    {
        private const string ASSET_PATH = @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
        private const string DEST_FOLDER = "Lockscreen";

        /// <summary>
        /// Temp folder's path
        /// </summary>
        private static string tempFolderPath;
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        public static void Main()
        {
            try
            {
                int imageCount = 0;
                var source = GetSourcePath();
                var dest = GetDestinationPath();

                if (!Directory.Exists(source))
                {
                    Console.WriteLine("Source folder does not exist");
                }
                else
                {
                    imageCount = SaveNow(source, dest);
                }

                // Delete temp folder
                Directory.Delete(tempFolderPath, true);

                if (imageCount == 0)
                {
                    return;
                }

                Console.Write("Exit sequence commencing");
                var tmp = AppSettings.Get("exitTimeout");
                long.TryParse(tmp, out long timeout);

                if (timeout > 1)
                {
                    ProcessExit(timeout);
                }
            }
            catch (Exception e)
            {
                // Delete temp folder
                Directory.Delete(tempFolderPath, true);

                Console.WriteLine("BUG happened");
                Console.Write("Start of exception");
                Console.WriteLine("=================================================================================================");
                Console.WriteLine(e);
                Console.Write("End of exception");
                Console.WriteLine("=================================================================================================");
                Console.Write("Press anykey to exit...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Save those lockscreen NOW
        /// </summary>
        /// <param name="source">Source folder path</param>
        /// <param name="dest">Destination folder path</param>
        /// <returns>Numnber of images saved</returns>
        public static int SaveNow(string source, string dest)
        {
            long.TryParse(AppSettings.Get("minFileSizeInByte"), out long minFileSize);
            // Copy files
            CopyAsset(source, dest, minFileSize);

            int.TryParse(AppSettings.Get("minImageWidth"), out int minImageWidth);
            // Move copied files
            int imageCount = SaveImage(minImageWidth);

            if (imageCount == 0)
            {
                return imageCount;
            }

            // Delete duplicate files
            var deleteDuplicate = AppSettings.Get("deleteDuplicate");

            if (deleteDuplicate.Equals("true"))
            {
                DeleteDuplicateImage(dest);
            }

            return imageCount;
        }

        /// <summary>
        /// Get source folder path
        /// </summary>
        /// <returns>Source folder path</returns>
        public static string GetSourcePath()
        {
            var source = AppSettings.Get("sourceFolder");

            // If source path is empty, guess it
            if (string.IsNullOrEmpty(source))
            {
                var appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var assetPath = AppSettings.Get("assetPath");

                // Setting source path
                if (string.IsNullOrEmpty(assetPath))
                {
                    source = Path.Combine(appDataLocal, ASSET_PATH);
                }
                else
                {
                    source = Path.Combine(appDataLocal, assetPath);
                }

                // Then save source folder to config
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // Save asset folder too
                if (string.IsNullOrEmpty(assetPath))
                {
                    config.AppSettings.Settings.Remove("assetPath");
                    config.AppSettings.Settings.Add("assetPath", ASSET_PATH);
                }

                config.AppSettings.Settings.Remove("sourceFolder");
                config.AppSettings.Settings.Add("sourceFolder", source);
                config.Save(ConfigurationSaveMode.Modified);
            }

            return source;
        }

        /// <summary>
        /// Get destination folder path
        /// </summary>
        /// <returns>Destination folder path</returns>
        public static string GetDestinationPath()
        {
            var dest = AppSettings.Get("destFolder");

            // If destination path is empty, place it on desktop
            if (string.IsNullOrEmpty(dest))
            {
                dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), DEST_FOLDER);
            }

            // If destination folder doesn't exist, make it exist
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            return dest;
        }

        /// <summary>
        /// Delete duplicate images in destination folder
        /// </summary>
        /// <param name="dest">Destination folder's path</param>
        public static void DeleteDuplicateImage(string dest)
        {
            var md5 = MD5.Create();
            IEnumerable<FileInfo> orderedFiles = Directory.EnumerateFiles(dest).Select(path => new FileInfo(path)).OrderBy(file => file.Length);
            IList<FileInfo> fileList = orderedFiles as IList<FileInfo> ?? orderedFiles.ToList();
            var startIndex = fileList.Count - 1;
            var fileCount = 0;

            for (var i = startIndex; i > 0; i--)
            {
                var prevFile = fileList.ElementAt(i - 1);
                var nextFile = fileList.ElementAt(i);

                // When 2 files 1 size
                if (prevFile.Length == nextFile.Length)
                {
                    string deletePath = null;

                    // Get 2 hash
                    var prevStream = File.OpenRead(prevFile.FullName);
                    var nextStream = File.OpenRead(nextFile.FullName);
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
        /// <param name="minImageWidth">Minimum image's width to process</param>
        /// <returns>Numnber of images saved</returns>
        public static int SaveImage(int minImageWidth)
        {
            string[] fileList = Directory.GetFiles(tempFolderPath);
            var parentFolderPath = new DirectoryInfo(tempFolderPath).Parent.FullName;
            var fileCount = 0;

            foreach (var s in fileList)
            {
                Image img = null;

                try
                {
                    img = Image.FromFile(s);
                }
                catch (Exception)
                {
                    // Non-image file
                    continue;
                }

                // Check image's width
                if (img.Width >= minImageWidth)
                {
                    img.Dispose();
                    var newFilePath = Path.Combine(parentFolderPath, s.Substring(s.LastIndexOf("\\", StringComparison.Ordinal) + 1));

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

            Console.WriteLine("Finish save " + fileCount + " image(s) to destination folder");
            return fileCount;
        }

        /// <summary>
        /// Copy files from asset folder to temp folder
        /// </summary>
        /// <param name="source">Source folder's path</param>
        /// <param name="dest">Destination folder's path</param>
        /// <param name="minFileSize">Minimum filesize to copy</param>
        /// <returns>Temp folder's path</returns>
        public static void CopyAsset(string source, string dest, long minFileSize)
        {
            var destFolder = new DirectoryInfo(dest);

            // Create temp folder inside destination folder
            var tempFolder = destFolder.CreateSubdirectory(DateTime.Now.Ticks.ToString());
            tempFolderPath = tempFolder.FullName;
            string[] fileList = Directory.GetFiles(source);
            var fileCount = 0;

            // Copy files to temp folder
            foreach (var s in fileList)
            {
                var file = new FileInfo(s);

                if (file.Length > minFileSize)
                {
                    var tmp = Path.Combine(tempFolderPath, file.Name + ".jpg");
                    File.Copy(s, tmp);
                    fileCount++;
                }
            }

            Console.WriteLine("Finish copy " + fileCount + " file(s) from asset folder");
        }

        /// <summary>
        /// Commence exit sequence
        /// </summary>
        /// <param name="exitTimeout">Exit timeout in seconds</param>
        public static void ProcessExit(long exitTimeout)
        {
            var count = exitTimeout - 1;

            for (var i = count; i > 0; i--)
            {
                Console.Write("\n" + i);
                Thread.Sleep(1000);
            }
        }
    }
}
