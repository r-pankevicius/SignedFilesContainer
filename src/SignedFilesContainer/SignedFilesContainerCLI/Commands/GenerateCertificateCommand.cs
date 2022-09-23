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
    /// Generates self signed certificate.
    /// </summary>
    /// <remarks>
    /// output: CertificateName.pfx + CertificateName.publickey
    /// </remarks>
    internal class GenerateCertificateCommand : Command<GenerateCertificateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [Description("Output file, .pfx extension will be used. Public key (BASE64 encoded) will be exported with .publickey extension.")]
            [CommandArgument(0, "<outputFile>")]
            public string OutputFile { get; init; } = "";
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            string outputFile = settings.OutputFile;
            string extension = Path.GetExtension(outputFile);
            if (!"pfx".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                outputFile = $"{outputFile}.pfx";
            }

            outputFile = Path.GetFullPath(outputFile);
            string? directory = Path.GetDirectoryName(outputFile);
            if (string.IsNullOrEmpty(directory))
            {
                AnsiConsole.Markup($"Could not get directory for file [red]{outputFile}[/].");
                return 1;
            }

            if (!Directory.Exists(directory))
            {
                AnsiConsole.Markup($"Directory [red]{directory}[/] doesn't exist.");
                return 2;
            }

            return 0;
        }
    }
}
