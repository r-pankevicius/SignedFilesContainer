using SignedFilesContainer;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SignedFilesContainerCLI.Commands
{
    /// <summary>
    /// Validates signed container (folder or zip file) against the public key.
    /// </summary>
    /// <remarks>
    /// input: container (folder or zip file), public key, output: valid/invalid
    /// </remarks>
    internal class ValidateContainerCommand : Command<ValidateContainerCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [Description("Input folder.")]
            [CommandArgument(0, "<inputFolder>")]
            public string InputFolder { get; init; } = "";

            [Description("Path to the public key file.")]
            [CommandOption("--public-key-file")]
            public string PublicKeyFile { get; init; } = "";
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            try
            {
                ContainerHelpers.ValidateContainer(settings.InputFolder, settings.PublicKeyFile);
            }
            catch (SignedFilesContainerException sfcEx)
            {
                AnsiConsole.MarkupLine($"[red]{sfcEx.Message}[/]");
                return sfcEx.ExitCode;
            }

            AnsiConsole.MarkupLine($"[green]VALID[/]. Container is valid.");
            return 0;
        }
    }
}
