using System;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Newtonsoft.Json;

namespace CP3.BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Reads a BSIPA manifest json file and outputs information from the manifest.
    /// </summary>
    public class GetManifestInfo : Task
    {
        #region Task Parameters 

        /// <summary>
        /// Optional: Path to the manifest file. Default is 'manifest.json'.
        /// </summary>
        public virtual string ManifestPath { get; set; } = "manifest.json";

        /// <summary>
        /// If enabled, this task will report a failure if it cannot parse the Plugin version or Game version.
        /// </summary>
        public virtual bool FailOnError { get; set; } = false;

        #endregion

        #region Task Output 
        
        /// <summary>
        /// The mod's name as reported by the manifest.
        /// </summary>
        [Output]
        public virtual string? PluginName { get; private set; }
        
        /// <summary>
        /// The mod's name as reported by the manifest.
        /// </summary>
        [Output]
        public virtual string? PluginAuthor { get; private set; }
        
        /// <summary>
        /// The mod's version as reported by the manifest.
        /// </summary>
        [Output]
        public virtual string? PluginVersion { get; private set; }

        /// <summary>
        /// The Beat Saber game version the mod is compatible with, as reported by the manifest.
        /// </summary>
        [Output]
        public virtual string? GameVersion { get; private set; }
        
        /// <summary>
        /// The mod's version as reported by the manifest with prerelease labels stripped.
        /// Use this for comparing to the AssemblyVersion metadata.
        /// </summary>
        [Output]
        public virtual string? BasePluginVersion { get; private set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try {
                string manifestJson = File.ReadAllText(ManifestPath);
                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(manifestJson);

                PluginName = manifest.Name;
                PluginAuthor = manifest.Author;
                PluginVersion = manifest.Version;
                GameVersion = manifest.GameVersion;

                if(PluginVersion != null) {
                    BasePluginVersion = PluginVersion.Split('-', '+')[0];
                }

                return true;
            }
            catch(Exception e) {
                Log.LogWarningFromException(e);
                return false;
            }
        }
    }
}