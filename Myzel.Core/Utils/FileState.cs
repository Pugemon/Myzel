namespace Myzel.Core.Utils
{
    /// <summary>
    /// Represents the state of a file.
    /// </summary>
    public enum FileState
    {
        /// <summary>
        /// The file is closed.
        /// </summary>
        Closed,

        /// <summary>
        /// The file is opened.
        /// </summary>
        Opened,

        /// <summary>
        /// The file is being read.
        /// </summary>
        Reading,

        /// <summary>
        /// The file is being written to.
        /// </summary>
        Writing,

        /// <summary>
        /// The file is being processed.
        /// </summary>
        Processing
    }
}