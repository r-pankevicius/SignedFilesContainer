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

            // IMPL-IT

            return 0;
        }
    }
}
