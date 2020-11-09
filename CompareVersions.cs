using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Compares an assembly and manifest version string, logs an error and optionally fails if they don't match.
    /// </summary>
    public class CompareVersions : Task
    {
        #region Task Parameters

        /// <summary>
        /// The mod's version as reported by the manifest.
        /// </summary>
        [Required]
        public virtual string? PluginVersion { get; set; }

        /// <summary>
        /// The mod's version as reported by the assembly.
        /// </summary>
        [Required]
        public virtual string? AssemblyVersion { get; set; }

        /// <summary>
        /// If enabled, this task will report a failure if the assembly version and manifest version don't match or there was a problem getting the value for either of them.
        /// </summary>
        public virtual bool ErrorOnMismatch { get; set; } = false;
        
        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(PluginVersion))
            {
                throw new ArgumentNullException(nameof(PluginVersion));
            }

            if (string.IsNullOrWhiteSpace(AssemblyVersion))
            {
                throw new ArgumentNullException(nameof(AssemblyVersion));
            }

            // Strip any pre-release or build tags and split into components
            string[] pluginVersion = PluginVersion!.Split('-', '+')[0].Split('.');
            string[] assemblyVersion = AssemblyVersion!.Split('-', '+')[0].Split('.');
            int maxLength = Math.Max(pluginVersion.Length, assemblyVersion.Length);

            // Compare each component
            for (int i = 0; i < maxLength; ++i)
            {
                if (
                    pluginVersion.Length <= i || pluginVersion[i] == "*"
                    || assemblyVersion.Length <= i || assemblyVersion[i] == "*"
                )
                {
                    return true;
                }
                if (pluginVersion[i] != assemblyVersion[i])
                {
                    if (ErrorOnMismatch)
                    {
                        Log.LogError("PluginVersion '{0}' does not match AssemblyVersion '{1}'.", PluginVersion, AssemblyVersion);
                        return false;
                    }
                    else
                    {
                        Log.LogWarning("PluginVersion '{0}' does not match AssemblyVersion '{1}'.", PluginVersion, AssemblyVersion);
                        return true;
                    }
                }
            }
            return true;
        }
    }
}