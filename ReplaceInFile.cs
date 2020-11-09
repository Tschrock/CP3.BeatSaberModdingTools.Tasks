using System;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CP3.BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Replaces a string pattern with a value in a file.
    /// </summary>
    public class ReplaceInFile : Task
    {
        #region Task Parameters

        /// <summary>
        /// File path.
        /// </summary>
        [Required]
        public virtual string? File { get; set; }

        /// <summary>
        /// Pattern to match (case sensitive).
        /// </summary>
        [Required]
        public virtual string? Pattern { get; set; }

        /// <summary>
        /// String to substitute.
        /// </summary>
        [Required]
        public virtual string? Substitute { get; set; }

        /// <summary>
        /// Set to true if <see cref="Pattern"/> is a regular expression.
        /// </summary>
        public virtual bool UseRegex { get; set; } = false;

        /// <summary>
        /// Changes '^' and '$ so they match the beginning and end of a line instead of the entire string.
        /// </summary>
        public virtual bool RegexMultilineMode { get; set; } = false;

        /// <summary>
        /// Changes the meaning of '.' so it matches every character except '\n' (newline).
        /// </summary>
        public virtual bool RegexSinglelineMode { get; set; } = false;

        /// <summary>
        /// Escapes the '\' character in Substitute with '\\'. Does not escape '\n'.
        /// </summary>
        public virtual bool EscapeBackslash { get; set; } = false;

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(File))
            {
                throw new ArgumentNullException(nameof(File));
            }

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                throw new ArgumentNullException(nameof(Pattern));
            }

            if (string.IsNullOrWhiteSpace(Substitute))
            {
                throw new ArgumentNullException(nameof(Substitute));
            }

            if (EscapeBackslash) Substitute = Substitute!.Replace(@"\", @"\\");

            string fileText = System.IO.File.ReadAllText(File);

            Log.LogMessage(MessageImportance.High, $"Replacing '{Pattern}' with '{Substitute}' in {File}");

            if (UseRegex)
            {
                RegexOptions options = RegexOptions.None;
                if (RegexMultilineMode) options |= RegexOptions.Multiline;
                if (RegexSinglelineMode) options |= RegexOptions.Singleline;

                fileText = Regex.Replace(fileText, Pattern, Substitute, options);
            }
            else {
                fileText = fileText.Replace(Pattern, Substitute);
            }

            System.IO.File.WriteAllText(File, fileText);
            return true;
        }
    }
}
