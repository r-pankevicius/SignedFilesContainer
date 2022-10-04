namespace SignedFilesContainer
{
    /// <summary>
    /// File entry in the container.
    /// </summary>
    public class FileEntry
    {
        /// <summary>
        /// Local path using "/" separators.
        /// </summary>
        public string LocalPath { get; set; } = "";

        public long Length { get; set; }

        /// <summary>
        /// Syntax: {Hash method}:{Base64 encoded Hash string}
        /// </summary>
        public string HashString { get; set; } = "";
    }
}
