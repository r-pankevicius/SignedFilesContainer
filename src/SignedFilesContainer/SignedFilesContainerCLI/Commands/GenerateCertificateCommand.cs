using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
            string outputFile = settings.OutputFile;
            string extension = Path.GetExtension(outputFile);
            if (!".pfx".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                outputFile = $"{outputFile}.pfx";
            }

            outputFile = Path.GetFullPath(outputFile);
            string? directory = Path.GetDirectoryName(outputFile);
            if (string.IsNullOrEmpty(directory))
            {
                AnsiConsole.MarkupLine($"Could not get directory for file [red]{outputFile}[/].");
                return 1;
            }

            if (!Directory.Exists(directory))
            {
                AnsiConsole.MarkupLine($"Directory [red]{directory}[/] doesn't exist.");
                return 2;
            }

            if (File.Exists(outputFile))
            {
                if (!settings.Overwrite)
                {
                    AnsiConsole.MarkupLine($"File [red]{outputFile}[/] exists and overwrite flag was not passed in arguments.");
                    return 3;
                }

                AnsiConsole.MarkupLine($"File [yellow]{outputFile}[/] will be overwritten.");
            }

            // public key file will be always overwritten
            string publicKeyFile = string.Concat(outputFile, ".publicKey");

            string certificateName = string.IsNullOrWhiteSpace(settings.Name) ?
                Path.GetFileNameWithoutExtension(outputFile) : settings.Name;

            CreateSelfSignedX509Certificate(
                outputFile, publicKeyFile, certificateName, settings.Password, settings.DnsName);

            AnsiConsole.MarkupLine($"Created certificate file [green]{outputFile}[/]. [magenta]Keep it safe[/] in a cool dry place.");
            AnsiConsole.MarkupLine($"Public key was written to [green]{publicKeyFile}[/].");

            return 0;
        }

        private static void CreateSelfSignedX509Certificate(
            string outputCertificateFilePath, string outputPublicKeyPath,
            string certificateName, string password, string? dnsName)
        {
            // Export self signed X509 certificate to pfx
            var certificateToExport = CreateSelfSignedServerCertificate(certificateName, password, dnsName);
            byte[] certBytes = certificateToExport.Export(X509ContentType.Pfx, password);
            File.WriteAllBytes(outputCertificateFilePath, certBytes);
            // Dump it on Windows: certutil -dump .\PrivateKey.pem.pfx

            // On windows, with NuGet package System.Windows.Extensions
            // X509Certificate2UI.DisplayCertificate(certificateToExport);

            RSA? rsaPublicKey = certificateToExport.PublicKey.GetRSAPublicKey();
            if (rsaPublicKey == null)
                throw new InvalidOperationException("GetRSAPublicKey() failed");

            byte[] publicKeyBytes = rsaPublicKey.ExportSubjectPublicKeyInfo();
            string publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);
            File.WriteAllText(outputPublicKeyPath, publicKeyBase64);
        }

        // https://creativecommons.org/licenses/by-sa/4.0/
        // https://stackoverflow.com/a/50138133/1175698
        private static X509Certificate2 CreateSelfSignedServerCertificate(
            string certificateName,
            string password,
            string? dnsName)
        {
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentException($"{nameof(certificateName)} is required.");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException($"{nameof(password)} is required.");

            SubjectAlternativeNameBuilder sanBuilder = new();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            if (!string.IsNullOrEmpty(dnsName))
            {
                sanBuilder.AddDnsName(dnsName);
            }

            X500DistinguishedName distinguishedName = new(distinguishedName: $"CN={certificateName}");

            using RSA rsa = RSA.Create(2048);

            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DataEncipherment |
                    X509KeyUsageFlags.KeyEncipherment |
                    X509KeyUsageFlags.DigitalSignature,
                    critical: false));

            request.CertificateExtensions.Add(
               new X509EnhancedKeyUsageExtension(
                   new OidCollection
                   {
                       new Oid("1.3.6.1.5.5.7.3.1") // WTF? I don't know.
                   }, critical: false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(
                notBefore: new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                notAfter: new DateTimeOffset(DateTime.UtcNow.AddYears(10)));
            // certificate.FriendlyName = certificateName; // FriendlyName used only under Windows

            byte[] bytes = certificate.Export(X509ContentType.Pfx, password);
            return new X509Certificate2(bytes, password, X509KeyStorageFlags.Exportable);
        }
    }
}
