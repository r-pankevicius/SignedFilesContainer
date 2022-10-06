using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    }
}
