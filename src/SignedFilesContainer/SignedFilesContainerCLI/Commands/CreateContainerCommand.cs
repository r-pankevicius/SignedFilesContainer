using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
            return 0;
        }
    }
}
