using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignedFilesContainer
{
    /// <summary>
    /// Helper methods to work with container: folders and zip files.
    /// </summary>
    public static class ContainerHelpers
    {
        public static readonly string MetaInfoFolderName = "META-INFO";
        public static readonly string ContentsFileName = "com.github.SignedFilesContainer.Contents.xml";

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

    }
}
