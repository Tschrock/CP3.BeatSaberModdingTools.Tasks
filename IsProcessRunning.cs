using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CP3.BeatSaberModdingTools.Tasks
{
    /// <summary>
    /// Checks if a process is currently running.
    /// </summary>
    public class IsProcessRunning : Task
    {
        #region Task Parameters

        /// <summary>
        /// Name of the process.
        /// </summary>
        [Required]
        public virtual string? ProcessName { get; set; }

        #endregion

        #region Task Output

        /// <summary>
        /// True if the process is running, false otherwise.
        /// </summary>
        [Output]
        public virtual bool IsRunning { get; private set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(ProcessName))
            {
                throw new ArgumentNullException(nameof(ProcessName));
            }

            IsRunning = Process.GetProcessesByName(ProcessName).Any();

            return true;
        }
    }
}
