using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignedFilesContainer
{
    public enum SignedFilesContainerError
    {
        InputFolderDoesntExist = 1,
        CertificateDoesntExist = 2,
        OutputFolderExists = 3,

        UnknownDirectory = 10,
        DirectoryDoesntExist = 1,
        OutputFileExists = 11,

        ContentsFileDoesntExist = 20,
        PublicKeyFileDoesntExist = 21,
        CouldNotCreateRSAPublicKey = 22,
        ContentsFileIsInvalid = 23,
        FileCountMismatch = 24,
        FileIsDifferent = 25,
    }
}
