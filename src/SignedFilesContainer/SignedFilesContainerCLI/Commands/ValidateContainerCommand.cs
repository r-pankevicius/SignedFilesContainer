using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            return 0;
        }
    }
}
