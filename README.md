# SignedFilesContainer
The toolset to digitally sign and verify file containers: folders in file system or zip files.

## CLI interface sketch
`
SignedFilesContainerCLI
	create-certificate
		output: CertificateName.pfx + CertificateName.publickey
	create-container
		input: folder, output: container (folder or zip file)
	validate
		input: container (folder or zip file), public key, output: valid/invalid
`

## Tech tutorials
[How to: Encrypt XML Elements with X.509 Certificates](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-encrypt-xml-elements-with-x-509-certificates)

[How to: Sign XML Documents with Digital Signatures](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-sign-xml-documents-with-digital-signatures)

[How to: Verify the Digital Signatures of XML Documents](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-verify-the-digital-signatures-of-xml-documents)
