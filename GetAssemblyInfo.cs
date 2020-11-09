using System;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CP3.BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Parses the AssemblyVersion from an AssemblyInfo.cs file.
    /// </summary>
    public class GetAssemblyInfo : Task
    {
        private static readonly Regex AssemblyVersionRegex = new Regex("\\[\\s*assembly\\s*:\\s*AssemblyVersion\\s*\\(\\s*(?<versionString>\"[^\"]+\")\\s*\\)\\s*\\]");

        #region Task Parameters

        /// <summary>
        /// Optional: Path to the file containing the assembly information. Default is 'Properties\AssemblyInfo.cs'.
        /// </summary>
        public virtual string AssemblyInfoPath { get; set; } = Path.Combine("Properties", "AssemblyInfo.cs");

        /// <summary>
        /// If enabled, this task will report a failure if it cannot parse the AssemblyVersion.
        /// </summary>
        public virtual bool FailOnError { get; set; } = false;

        #endregion

        #region Task Output 

        /// <summary>
        /// Version of the assembly.
        /// </summary>
        [Output]
        public virtual string? AssemblyVersion { get; private set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                string assemblyFileContents = File.ReadAllText(AssemblyInfoPath);

                Match versionMatch = AssemblyVersionRegex.Match(assemblyFileContents);
                if (versionMatch.Success)
                {
                    string assemblyVersionString = versionMatch.Groups["versionString"].Value;
                    AssemblyVersion = Regex.Unescape(assemblyVersionString);
                    return true;
                }
                else if (FailOnError)
                {
                    Log.LogError("AssemblyVersion could not be determined.");
                    return false;
                }
                else
                {
                    Log.LogWarning("AssemblyVersion could not be determined.");
                    return true;
                }
            }
            catch (Exception e)
            {
                if (FailOnError)
                {
                    Log.LogErrorFromException(e);
                    return false;
                }
                else
                {
                    Log.LogWarningFromException(e);
                    return true;
                }
            }
        }
    }
}