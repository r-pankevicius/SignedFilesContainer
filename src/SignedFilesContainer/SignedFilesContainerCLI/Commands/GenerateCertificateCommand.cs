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
    /// Generates self signed certificate.
    /// </summary>
    /// <remarks>
    /// output: CertificateName.pfx + CertificateName.publickey
    /// </remarks>
    internal class GenerateCertificateCommand : Command<GenerateCertificateCommand.Settings>
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
