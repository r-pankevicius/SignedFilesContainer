using SignedFilesContainerCLI.Commands;
using Spectre.Console.Cli; // don't uprade to a newer version of Spectre.Console otherwise you'll get an error here. https://www.nuget.org/packages?q=Spectre.Console.Cli is in preview 0.8
using System;
using System.Reflection;

namespace SignedFilesContainerCLI
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine($"{nameof(SignedFilesContainerCLI)} {Assembly.GetExecutingAssembly().GetName().Version}");

            var x = new Spectre.Console.Calendar(DateTime.Now);
            var cliApp = new CommandApp<GenerateCertCommand>();
            cliApp.Configure(config =>
            {
            });

            return cliApp.Run(args);
        }
    }
}