using SignedFilesContainer;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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

            [Description("Certificate name. Same as file name, if ommited.")]
            [CommandOption("--name")]
            public string? Name { get; set; }

            [CommandOption("--password")]
            public string Password { get; set; } = "";

            [Description("DNS name, I don't know what it is for.")]
            [CommandOption("--dns-name")]
            public string? DnsName { get; set; }

            [Description("Overwrite existing.")]
            [CommandOption("--overwrite")]
            public bool Overwrite { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            try
            {
                CertificateHelpers.CreateCertificate(
                    settings.OutputFile, settings.Name, settings.Password, settings.DnsName, settings.Overwrite);
            }
            catch (SignedFilesContainerException sfcEx)
            {
                AnsiConsole.MarkupLine($"[red]{sfcEx.Message}[/]");
                return sfcEx.ExitCode;
            }


            //AnsiConsole.MarkupLine($"Created certificate file [green]{outputFile}[/]. [magenta]Keep it safe[/] in a cool dry place.");
            //AnsiConsole.MarkupLine($"Public key was written to [green]{publicKeyFile}[/].");
            // TODO: write info
            return 0;
        }
    }
}
