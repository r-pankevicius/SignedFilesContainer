using SignedFilesContainerCLI.Commands;
using Spectre.Console.Cli; // don't uprade to a newer version of Spectre.Console otherwise you'll get an error here. https://www.nuget.org/packages?q=Spectre.Console.Cli is in preview 0.8

namespace SignedFilesContainerCLI
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            var cliApp = new CommandApp();

            cliApp.Configure(config =>
            {
                config.PropagateExceptions();
                config.AddCommand<GenerateCertificateCommand>("create-certificate");
                config.AddCommand<CreateContainerCommand>("create-container");
                config.AddCommand<ValidateContainerCommand>("validate-container");
            });

            return cliApp.Run(args);
        }
    }
}