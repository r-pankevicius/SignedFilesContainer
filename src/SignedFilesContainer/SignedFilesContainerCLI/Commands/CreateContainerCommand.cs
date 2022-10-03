using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            string metainfoFolder = Path.Combine(settings.OutputFolder, "META-INFO");
            if (!Directory.Exists(metainfoFolder))
                Directory.CreateDirectory(metainfoFolder);

            string metainfoFile = Path.Combine(metainfoFolder, "SignedFilesContainer.FileList.xml");
            if (File.Exists(metainfoFile))
                File.Delete(metainfoFile);

            CopyDirectory(settings.InputFolder, settings.OutputFolder, recursive: true);
            Directory.CreateDirectory(metainfoFolder);

            //Directory.Co

            AnsiConsole.MarkupLine($"Created a signed container [green]{settings.OutputFolder}[/].");
            AnsiConsole.MarkupLine($"[magenta]You'll need a public key to validate it.[/] I hope you remember where it is.");

            return 0;
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
