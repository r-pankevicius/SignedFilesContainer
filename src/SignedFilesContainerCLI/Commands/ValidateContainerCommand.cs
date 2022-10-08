using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Xml;
using SignedFilesContainer;
using System.Xml.Serialization;

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
            [Description("Input folder.")]
            [CommandArgument(0, "<inputFolder>")]
            public string InputFolder { get; init; } = "";

            [Description("Path to the public key file.")]
            [CommandOption("--public-key-file")]
            public string PublicKey { get; init; } = "";
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            if (!Directory.Exists(settings.InputFolder))
            {
                AnsiConsole.MarkupLine($"Input directory [red]{settings.InputFolder}[/] doesn't exist.");
                return 1;
            }

            string contentsXmlPath = Path.Combine(settings.InputFolder,
                ContainerHelpers.MetaInfoFolderName, ContainerHelpers.ContentsFileName);
            if (!File.Exists(contentsXmlPath))
            {
                AnsiConsole.MarkupLine($"File listing metainfo file [red]{contentsXmlPath}[/] doesn't exist.");
                return 2;
            }

            if (!File.Exists(settings.PublicKey))
            {
                AnsiConsole.MarkupLine($"Public key file [red]{settings.PublicKey}[/] doesn't exist.");
                return 3;
            }

            string publicKeyString = File.ReadAllText(settings.PublicKey);
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyString);

            var publicKey = PublicKey.CreateFromSubjectPublicKeyInfo(publicKeyBytes, out _);
            RSA? rsaPublicKey = publicKey.GetRSAPublicKey();
            if (rsaPublicKey == null)
                throw new InvalidOperationException("GetRSAPublicKey() failed");

            var signedXmlDoc = new XmlDocument
            {
                PreserveWhitespace = true
            };

            signedXmlDoc.Load(contentsXmlPath);
            
            // Verify the signature of the signed file list XML.
            bool contentsXmlIsValid = VerifyXml(signedXmlDoc, rsaPublicKey);
            if (!contentsXmlIsValid)
            {
                AnsiConsole.MarkupLine($"[red]INVALID[/]. File list file signature invalid.");
                return 5;
            }

            var serializer = new XmlSerializer(typeof(Contents));
            using var stringReader = new StringReader(signedXmlDoc.OuterXml);
            var declaredContents = (Contents?)serializer.Deserialize(stringReader);
            if (declaredContents is null)
                throw new InvalidOperationException("Deserialized contents xml to null");

            // Verify all files sha and the fact that only these files are in container
            var actualContents = ContainerHelpers.GetDirectoryContents(settings.InputFolder);
            actualContents.Files.RemoveAll(fe => fe.LocalPath == ContainerHelpers.ContentsFileLocalPath);

            if (actualContents.Files.Count != declaredContents.Files.Count)
            {
                AnsiConsole.MarkupLine(
                    $"[red]INVALID[/]. File count mismatch. Declared: {declaredContents.Files.Count}, found: {actualContents.Files.Count}.");
                return 10;
            }

            foreach (var actualFileEntry in actualContents.Files)
            {
                int idx = declaredContents.Files.IndexOf(actualFileEntry);
                if (idx < 0)
                {
                    // TODO: compare what's different and display more informative msg
                    AnsiConsole.MarkupLine(
                        $"[red]INVALID[/]. The file {actualFileEntry.LocalPath} is different than declared.");
                    return 11;
                }

                declaredContents.Files.RemoveAt(idx);
            }

            if (declaredContents.Files.Count > 0)
            {
                AnsiConsole.MarkupLine(
                    "[red]ERROR[/]. Something went wrong.");
                return 19;
            }

            AnsiConsole.MarkupLine($"[green]VALID[/]. Container is valid.");
            return 0;
        }

        // Verify the signature of an XML file against an asymmetric
        // algorithm and return the result.
        public static bool VerifyXml(XmlDocument xmlDoc, RSA key)
        {
            // Check arguments.
            if (xmlDoc == null)
                throw new ArgumentException(null, nameof(xmlDoc));
            if (key == null)
                throw new ArgumentException(null, nameof(key));

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new(xmlDoc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            }

            // This example only supports one signature for
            // the entire XML document.  Throw an exception
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");
            }

            // Load the first <signature> node.
            signedXml.LoadXml((XmlElement)nodeList[0]!);

            // Check the signature and return the result.
            return signedXml.CheckSignature(key);
        }
    }
}
