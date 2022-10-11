using System;

namespace SignedFilesContainer
{
    public class SignedFilesContainerException : Exception
    {
        private readonly string _errorMessage;

        public SignedFilesContainerException(SignedFilesContainerError error,  string errorMessage)
        {
            Error = error;
            _errorMessage = errorMessage;
        }

        public SignedFilesContainerError Error { get; init; }

        public override string Message => _errorMessage;

        public int ExitCode => (int)Error;
    }
}
