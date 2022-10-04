using SignedFilesContainer;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SignedFilesContainerCLI.Commands
{
    /// <summary>
    /// Creates a signed container (folder or zip file) using a certificate.
    /// </summary>
    /// <remarks>
    /// input: folder, output: container (folder or zip file), certificate
    /// </remarks>
    internal class CreateContainerCommand : Command<CreateContainerCommand.Settings>
    {
        private const string MetaInfoFolderName = "META-INFO";
        private const string FileListFileName = "com.github.SignedFilesContainer.FileList.xml";

        public class Settings : CommandSettings
        {
            [Description("Input folder.")]
            [CommandArgument(0, "<inputFolder>")]
            public string InputFolder { get; init; } = "";

            [Description("Output folder (signed container).")]
            [CommandArgument(1, "<output>")]
            public string OutputFolder { get; init; } = "";

            [Description("Path to the certificate file.")]
            [CommandOption("--certificate")]
            public string Certificate { get; init; } = "";

            [Description("Overwrite existing container.")]
            [CommandOption("--overwrite")]
            public bool Overwrite { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (!Directory.Exists(settings.InputFolder))
            {
                AnsiConsole.MarkupLine($"Input directory [red]{settings.InputFolder}[/] doesn't exist.");
                return 1;
            }

            if (!File.Exists(settings.Certificate))
            {
                AnsiConsole.MarkupLine($"Certificate file [red]{settings.Certificate}[/] doesn't exist.");
                return 2;
            }

            if (Directory.Exists(settings.OutputFolder))
            {
                if (!settings.Overwrite)
                {
                    AnsiConsole.MarkupLine($"Output directory [red]{settings.OutputFolder}[/] exists and overwrite flag was not passed in arguments.");
                    return 3;
                }

                AnsiConsole.MarkupLine($"Directory [yellow]{settings.OutputFolder}[/] will be overwritten.");

                Directory.Delete(settings.OutputFolder, recursive: true);
                Directory.CreateDirectory(settings.OutputFolder);
            }

            string metainfoFolder = Path.Combine(settings.OutputFolder, MetaInfoFolderName);
            if (!Directory.Exists(metainfoFolder))
                Directory.CreateDirectory(metainfoFolder);

            string fileListFile = Path.Combine(metainfoFolder, FileListFileName);
            if (File.Exists(fileListFile))
                File.Delete(fileListFile);

            CopyDirectory(settings.InputFolder, settings.OutputFolder, recursive: true);
            Directory.CreateDirectory(metainfoFolder);

            var fileList = new FileList
            {
                Files = GetFileEntries(settings.OutputFolder).ToList()
            };

            // TODO: extract
            var serializer = new XmlSerializer(typeof(FileList));

            using var memoryStream = new MemoryStream();
            var streamWriter = XmlWriter.Create(memoryStream, new()
            {
                Encoding = Encoding.UTF8,
                Indent = true
            });

            serializer.Serialize(streamWriter, fileList);
            string fileListXml = Encoding.UTF8.GetString(memoryStream.ToArray());
            Console.WriteLine(fileListXml);

            AnsiConsole.MarkupLine($"Created a signed container [green]{settings.OutputFolder}[/].");
            AnsiConsole.MarkupLine($"[magenta]You'll need a public key to validate it.[/] I hope you remember where it is.");

            return 0;
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
                string hash = GetSHA384FileHash(fi.FullName);
                string relativePath = GetRelativePathFrom(rootDir, fi.FullName);
                result.Add(new FileEntry
                {
                    LocalPath = ChangeToUnixPathSeparators(relativePath),
                    Length = fi.Length,
                    HashString = $"sha384:{hash}"
                });
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                string subdirectory = Path.Combine(currentDir, subDir.Name);
                GetFileHashes(result, rootDir, subdirectory);
            }
        }

        private static string GetRelativePathFrom(string rootDir, string fullName)
        {
            string fullRootDir = Path.GetFullPath(rootDir);
            string fullRootName = Path.GetFullPath(fullName);
            if (fullRootName.Equals(fullRootDir))
                return "";

            if (!fullRootName.StartsWith(fullRootDir))
                throw new InvalidOperationException($"File or directory '${fullName}' was expected to be under directory `{rootDir}`.");

            return fullRootName[(fullRootDir.Length + 1)..^0];
        }

        private static string ChangeToUnixPathSeparators(string localPath) =>
            localPath.Replace('\\', '/');

        private static string GetSHA384FileHash(string pathToFile)
        {
            if (!File.Exists(pathToFile))
                throw new FileNotFoundException($"File not found: `{pathToFile}`.");

            byte[] fileBytes = File.ReadAllBytes(pathToFile);

            // SHA384Managed is obsolete but RTFM means read that f***ing manual, and the manual is really f***ing:
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha384.create?view=net-6.0
            SHA384 shaM = new SHA384Managed();
            byte[] hashBytes = shaM.ComputeHash(fileBytes);
            return Convert.ToBase64String(hashBytes);
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
