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
    /// Creates a signed container (folder or zip file) using certificate.
    /// </summary>
    /// <remarks>
    /// input: folder, output: container (folder or zip file)
    /// </remarks>
    internal class CreateContainerCommand : Command<CreateContainerCommand.Settings>
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
