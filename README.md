# SignedFilesContainer
The .net toolset to digitally sign and verify file containers: folders in file system or zip files plus the library for applications.

## Scenario test runs

After `git clone <repo url>`.

### Create self signed public key, sign a folder with it, validate signed folder

#### Linux bash
```bash
cd SignedFilesContainer/src

dotnet build

./SignedFilesContainerCLI/bin/Debug/net6.0/SignedFilesContainerCLI create-certificate ./sample-data/cool-dry-place/CertForSample1.pfx --password Kuku --overwrite

cp ./sample-data/cool-dry-place/CertForSample1.pfx.publicKey ./sample-data

./SignedFilesContainerCLI/bin/Debug/net6.0/SignedFilesContainerCLI create-container ./sample-data/sample1/input ./sample-data/sample1/output --certificate ./sample-data/cool-dry-place/CertForSample1.pfx --password Kuku --overwrite

./SignedFilesContainerCLI/bin/Debug/net6.0/SignedFilesContainerCLI validate-container ./sample-data/sample1/output --public-key-file ./sample-data/CertForSample1.pfx.publicKey
```

### Windows command prompt
```cmd
cd SignedFilesContainer\src

dotnet build

.\SignedFilesContainerCLI\bin\Debug\net6.0\SignedFilesContainerCLI create-certificate .\sample-data\cool-dry-place\CertForSample1.pfx --password Kuku --overwrite

copy /Y .\sample-data\cool-dry-place\CertForSample1.pfx.publicKey .\sample-data

.\SignedFilesContainerCLI\bin\Debug\net6.0\SignedFilesContainerCLI create-container .\sample-data\sample1\input .\sample-data\sample1\output --certificate .\sample-data\cool-dry-place\CertForSample1.pfx --password Kuku --overwrite

.\SignedFilesContainerCLI\bin\Debug\net6.0\SignedFilesContainerCLI validate-container .\sample-data\sample1\output --public-key-file .\sample-data\CertForSample1.pfx.publicKey
```

### PowerShell
```PowerShell
<# TBD #>
```

## Tech tutorials
[How to: Sign XML Documents with Digital Signatures](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-sign-xml-documents-with-digital-signatures)

[How to: Verify the Digital Signatures of XML Documents](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-verify-the-digital-signatures-of-xml-documents)
