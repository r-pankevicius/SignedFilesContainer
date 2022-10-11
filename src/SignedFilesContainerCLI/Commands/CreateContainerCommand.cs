using SignedFilesContainer;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

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

            [Description("Certificate password.")]
            [CommandOption("--password")]
            public string Password { get; init; } = "";

            [Description("Overwrite existing container.")]
            [CommandOption("--overwrite")]
            public bool Overwrite { get; set; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            try
            {
                ContainerHelpers.CreateContainer(
                    settings.InputFolder, settings.Certificate, settings.Password, settings.OutputFolder, settings.Overwrite);
            }
            catch (SignedFilesContainerException sfcEx)
            {
                AnsiConsole.MarkupLine($"[red]{sfcEx.Message}[/]");
                return sfcEx.ExitCode;
            }

            AnsiConsole.MarkupLine($"Created a signed container [green]{settings.OutputFolder}[/].");
            AnsiConsole.MarkupLine($"[magenta]You'll need a public key to validate it.[/] I hope you remember where it is.");

            return 0;
        }
    }
}
