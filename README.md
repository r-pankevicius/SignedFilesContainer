# Signed Files Container

The .net toolset and a library to digitally sign and verify file containers: folders in file system or zip files.

The **container** is content-agnostic, it just cares about integrity of the **content**: files you put in the folder or zip file.

*Works only with file folders now, zip files support will come later.*

Project contains 2 main parts:
- SignedFilesContainer: .net library with all the logic.
- SignedFilesContainerCLI: console app on top of that.

## Workflow
### Create self-signed certificate
TBD

### Sign container
TBD

It uses assymetric key signing, difference between signing and encryption [explained here](https://stackoverflow.com/a/454069/1175698).

### Validate container
Validates that container is the same as after "Sign container" step:
- Contents file is signed with your private key.
- Container contains same files and folders: no files were removed, no files were added.
- All files' contents are the same.


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
