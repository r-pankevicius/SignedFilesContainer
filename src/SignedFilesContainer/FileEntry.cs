using System.Collections.Generic;

namespace SignedFilesContainer
{
    /// <summary>
    /// Container contents.
    /// </summary>
    public class FileList
    {
        public List<FileEntry> Files { get; set; } = new();
    }

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
        /// Base64 encoded SHA384 hash string.
        /// </summary>
        public string SHA384 { get; set; } = "";
    }
}
