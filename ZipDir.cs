using System;
using System.IO;
using System.IO.Compression;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Zips the contents of a directory.
    /// </summary>
    public class ZipDir : Task
    {
        #region Task Parameters

        /// <summary>
        /// Name of the directory to zip.
        /// </summary>
        [Required]
        public virtual string? SourceDirectory { get; set; }

        /// <summary>
        /// Name of the created zip file.
        /// </summary>
        [Required]
        public virtual string? DestinationFile { get; set; }

        #endregion

        #region Task Output

        /// <summary>
        /// Full path to the created zip file.
        /// </summary>
        [Output]
        public virtual string? ZipPath { get; set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(SourceDirectory))
            {
                throw new ArgumentNullException(nameof(SourceDirectory));
            }

            if (string.IsNullOrWhiteSpace(DestinationFile))
            {
                throw new ArgumentNullException(nameof(DestinationFile));
            }

            FileInfo outputFile = new FileInfo(DestinationFile);

            // Make sure the output directory exists
            outputFile.Directory.Create();

            // Delete the file if it exists
            outputFile.Delete();

            // Create the zip file
            ZipFile.CreateFromDirectory(SourceDirectory, DestinationFile);

            ZipPath = outputFile.FullName;

            return true;
        }
    }
}
