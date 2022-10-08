using System;

namespace SignedFilesContainer
{
    public class SignedFilesContainerException : Exception
    {
        public SignedFilesContainerException(SignedFilesContainerError error)
        {
            Error = error;
        }

        public SignedFilesContainerError Error { get; init; }
    }
}
