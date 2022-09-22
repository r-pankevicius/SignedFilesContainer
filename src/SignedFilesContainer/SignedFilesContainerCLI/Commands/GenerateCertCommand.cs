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
    /// Generate self signed certificate.
    /// </summary>
    internal class GenerateCertCommand : Command<GenerateCertCommand.Settings>
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
