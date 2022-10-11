using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Net;
using System.Security.Cryptography;

namespace SignedFilesContainer
{
    /// <summary>
    /// Helper methods to work with container: folders and zip files.
    /// </summary>
    public static class ContainerHelpers
    {
        public static readonly string MetaInfoFolderName = "META-INFO";
        public static readonly string ContentsFileName = "com.github.SignedFilesContainer.Contents.xml";
        public static readonly string ContentsFileLocalPath = $"{MetaInfoFolderName}/{ContentsFileName}";

        public static void CreateContainer(
            string inputFolder,
            string pathToCertificate,
            string certificatePassword,
            string outputFolder,
            bool overwrite)
        {
            if (!Directory.Exists(inputFolder))
            {
                throw new SignedFilesContainerException(
                    SignedFilesContainerError.InputFolderDoesntExist,
                    $"Input directory `{inputFolder}` doesn't exist.");
            }

            if (!File.Exists(pathToCertificate))
            {
                throw new SignedFilesContainerException(
                    SignedFilesContainerError.CertificateDoesntExist,
                    $"Certificate file `{pathToCertificate}` doesn't exist.");
            }

            if (Directory.Exists(outputFolder))
            {
                if (!overwrite)
                {
                    throw new SignedFilesContainerException(
                        SignedFilesContainerError.OutputFolderExists,
                        $"Output directory `{outputFolder}` exists and overwrite flag was not passed in the arguments.");
                }

                // AnsiConsole.MarkupLine($"Directory [yellow]{settings.OutputFolder}[/] will be overwritten.");

                Directory.Delete(outputFolder, recursive: true);
                Directory.CreateDirectory(outputFolder);
            }

            string metainfoFolder = Path.Combine(outputFolder, MetaInfoFolderName);
            if (!Directory.Exists(metainfoFolder))
                Directory.CreateDirectory(metainfoFolder);

            string contentsFile = Path.Combine(metainfoFolder, ContentsFileName);
            if (File.Exists(contentsFile))
                File.Delete(contentsFile);

            CopyDirectory(inputFolder, outputFolder, recursive: true);
            Directory.CreateDirectory(metainfoFolder);

            Contents contents = ContainerHelpers.GetDirectoryContents(outputFolder);

            var serializer = new XmlSerializer(typeof(Contents));

            using var memoryStream = new MemoryStream();
            var streamWriter = XmlWriter.Create(memoryStream, new()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            });

            serializer.Serialize(streamWriter, contents);
            string contentsXml = Encoding.UTF8.GetString(memoryStream.ToArray());

            var certificate = new X509Certificate2(File.ReadAllBytes(pathToCertificate), certificatePassword);
            string signedXml = CertificateHelpers.SignXml(contentsXml, certificate);
            File.WriteAllText(contentsFile, signedXml);
        }

        public static string GetRelativePathFrom(string rootDir, string fullName)
        {
            string fullRootDir = Path.GetFullPath(rootDir);
            string fullRootName = Path.GetFullPath(fullName);
            if (fullRootName.Equals(fullRootDir))
                return "";

            if (!fullRootName.StartsWith(fullRootDir))
                throw new InvalidOperationException($"File or directory '${fullName}' was expected to be under directory `{rootDir}`.");

            return fullRootName[(fullRootDir.Length + 1)..^0];
        }

        public static string ChangeToUnixPathSeparators(string localPath) =>
            localPath.Replace('\\', '/');

        public static Contents GetDirectoryContents(string directory)
        {
            return new Contents
            {
                Files = GetFileEntries(directory).ToList()
            };
        }

        private static IEnumerable<FileEntry> GetFileEntries(string rootDir) =>
            GetFileEntries(rootDir, rootDir);

        private static IEnumerable<FileEntry> GetFileEntries(string rootDir, string currentDir)
        {
            var result = new List<FileEntry>();
            GetFileHashes(result, rootDir, currentDir);
            return result;
        }

        private static void GetFileHashes(List<FileEntry> result, string rootDir, string currentDir)
        {
            var dir = new DirectoryInfo(currentDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Directory not found: {dir.FullName}");

            foreach (FileInfo fi in dir.GetFiles())
            {
                string hash = CertificateHelpers.GetSHA384FileHash(fi.FullName);
                string relativePath = GetRelativePathFrom(rootDir, fi.FullName);
                result.Add(new FileEntry
                {
                    LocalPath = ChangeToUnixPathSeparators(relativePath),
                    Length = fi.Length,
                    SHA384 = hash
                });
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                string subdirectory = Path.Combine(currentDir, subDir.Name);
                GetFileHashes(result, rootDir, subdirectory);
            }
        }

        /// <summary>
        /// // Quite a lame example from https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
