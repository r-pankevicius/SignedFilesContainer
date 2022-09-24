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

            [CommandOption("--overwrite")]
            public bool Overwrite { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            string outputFile = settings.OutputFile;
            string extension = Path.GetExtension(outputFile);
            if (!".pfx".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                outputFile = $"{outputFile}.pfx";
            }

            outputFile = Path.GetFullPath(outputFile);
            string? directory = Path.GetDirectoryName(outputFile);
            if (string.IsNullOrEmpty(directory))
            {
                AnsiConsole.MarkupLine($"Could not get directory for file [red]{outputFile}[/].");
                return 1;
            }

            if (!Directory.Exists(directory))
            {
                AnsiConsole.MarkupLine($"Directory [red]{directory}[/] doesn't exist.");
                return 2;
            }

            if (File.Exists(outputFile))
            {
                if (!settings.Overwrite)
                {
                    AnsiConsole.MarkupLine($"File [red]{outputFile}[/] exist sand overwrite flag was not passed in arguments.");
                    return 3;
                }

                AnsiConsole.MarkupLine($"File [yellow]{outputFile}[/] will be overwritten.");
            }

            File.WriteAllText(outputFile, "top secret");

            // public key file will be always overwritten
            string publicKeyFile = string.Concat(outputFile, ".publicKey");
            File.WriteAllText(publicKeyFile, "public key");

            AnsiConsole.MarkupLine($"Created certificate file [green]{outputFile}[/]. [magenta]Keep it safe[/] in a cool dry place.");
            AnsiConsole.MarkupLine($"Public key was written to [green]{publicKeyFile}[/].");

            return 0;
        }
    }
}
