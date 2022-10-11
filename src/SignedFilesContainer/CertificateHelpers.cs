using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SignedFilesContainer
{
    /// <summary>
    /// Helper methods to work with certificates.
    /// </summary>
    public static class CertificateHelpers
    {
        public static void CreateCertificate(
            string outputFile,
            string certificateName,
            string password,
            string dnsName,
            bool overwrite)
        {
            string extension = Path.GetExtension(outputFile);
            if (!".pfx".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                outputFile = $"{outputFile}.pfx";
            }

            outputFile = Path.GetFullPath(outputFile);
            string? directory = Path.GetDirectoryName(outputFile);
            if (string.IsNullOrEmpty(directory))
            {
                throw new SignedFilesContainerException(
                     SignedFilesContainerError.UnknownDirectory,
                     $"Could not get directory for file `{outputFile}`.");
            }

            if (!Directory.Exists(directory))
            {
                throw new SignedFilesContainerException(
                     SignedFilesContainerError.DirectoryDoesntExist,
                     $"Directory `{directory}` doesn't exist.");
            }

            if (File.Exists(outputFile))
            {
                if (!overwrite)
                {
                    throw new SignedFilesContainerException(
                        SignedFilesContainerError.OutputFileExists,
                        $"Output file `{outputFile}` exists and overwrite flag was not passed in the arguments.");
                }

                //AnsiConsole.MarkupLine($"File [yellow]{outputFile}[/] will be overwritten.");
            }

            // public key file will be always overwritten
            string publicKeyFile = string.Concat(outputFile, ".publicKey");

            certificateName = string.IsNullOrWhiteSpace(certificateName) ?
                Path.GetFileNameWithoutExtension(outputFile) : certificateName;

            CreateSelfSignedX509Certificate(
                outputFile, publicKeyFile, certificateName, password, dnsName);
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

            RSA? rsaPublicKey = certificateToExport.PublicKey.GetRSAPublicKey();
            if (rsaPublicKey == null)
                throw new InvalidOperationException("GetRSAPublicKey() failed");

            byte[] publicKeyBytes = rsaPublicKey.ExportSubjectPublicKeyInfo();
            string publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);
            File.WriteAllText(outputPublicKeyPath, publicKeyBase64);
        }

        // https://creativecommons.org/licenses/by-sa/4.0/
        // https://stackoverflow.com/a/50138133/1175698
        private static X509Certificate2 CreateSelfSignedServerCertificate1(
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

            byte[] bytes = certificate.Export(X509ContentType.Pfx, password);
            return new X509Certificate2(bytes, password, X509KeyStorageFlags.Exportable);
        }

        // https://creativecommons.org/licenses/by-sa/4.0/
        // https://stackoverflow.com/a/50138133/1175698
        public static X509Certificate2 CreateSelfSignedServerCertificate2(
            string certificateName, string? password, string? dnsName = null)
        {
            if (string.IsNullOrEmpty(certificateName))
                throw new ArgumentException($"{certificateName} is required.");

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
                   new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(
                notBefore: new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                notAfter: new DateTimeOffset(DateTime.UtcNow.AddYears(10)));
            // certificate.FriendlyName = certificateName; // FriendlyName used only under Windows

            byte[] bytes = certificate.Export(X509ContentType.Pfx, password);
            return new X509Certificate2(bytes, password, X509KeyStorageFlags.Exportable);
        }

        public static string SignXml(string inputXml, X509Certificate2 certificate)
        {
            var originalXmlDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            // TODO: what the magic? https://stackoverflow.com/questions/6784799/what-is-this-char-65279
            if (inputXml[0] == 65279)
            {
                inputXml = inputXml[1..^0];
            }

            originalXmlDoc.LoadXml(inputXml);

            // Add the signing RSA key to the SignedXml object
            SignedXml signedXml = new(originalXmlDoc)
            {
                SigningKey = certificate.GetRSAPrivateKey()
            };

            // Create a reference to be signed.
            Reference reference = new()
            {
                Uri = ""
            };

            // Add an XmlDsigEnvelopedSignatureTransform object to the Reference object.
            // A transformation allows the verifier to represent the XML data in the identical manner that the signer used.
            // XML data can be represented in different ways, so this step is vital to verification.
            XmlDsigEnvelopedSignatureTransform env = new();
            reference.AddTransform(env);

            // Add the Reference object to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature
            signedXml.ComputeSignature();

            // Retrieve the XML representation of the signature (a <Signature> element) and save it to a new XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XmlDocument object.
            originalXmlDoc.DocumentElement!.AppendChild(originalXmlDoc.ImportNode(xmlDigitalSignature, deep: true));

            return originalXmlDoc.OuterXml;
        }

        public static string GetSHA384FileHash(string pathToFile)
        {
            if (!File.Exists(pathToFile))
                throw new FileNotFoundException($"File not found: `{pathToFile}`.");

            byte[] fileBytes = File.ReadAllBytes(pathToFile);

            // SHA384Managed is obsolete but RTFM means read that f***ing manual, and the manual is really f***ing:
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha384.create?view=net-6.0
            SHA384 shaM = new SHA384Managed();
            byte[] hashBytes = shaM.ComputeHash(fileBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
