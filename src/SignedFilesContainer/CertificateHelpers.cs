using System;
using System.IO;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace SignedFilesContainer
{
    /// <summary>
    /// Static helper methods.
    /// </summary>
    public static class CertificateHelpers
    {
        public static Certificate CreateSelfSignedX509Certificate(string certificateName, string? password, string? dnsName = null)
        {
            var X509Certificate = CreateSelfSignedServerCertificate(certificateName, password, dnsName);
            return new Certificate(X509Certificate, password);
        }

        // https://creativecommons.org/licenses/by-sa/4.0/
        // https://stackoverflow.com/a/50138133/1175698
        public static X509Certificate2 CreateSelfSignedServerCertificate(string certificateName, string? password, string? dnsName = null)
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
    }
}
