using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CP3.BeatSaberModdingTools.Tasks
{
    public class GetCommitInfo : Task
    {
        private static readonly Regex GitHubSshRegex = new Regex("^git@github\\.com:(?<username>[A-z0-9-]+)/.*\\.git$");
        private static readonly Regex GitHubHttpRegex = new Regex("^https?://github\\.com/(?<username>[A-z0-9-]+)/.*\\.git$");

        #region Task Parameters 

        /// <summary>
        /// The directory of the project.
        /// </summary>
        [Required]
        public virtual string? ProjectDir { get; set; }

        /// <summary>
        /// Optional: Number of characters to retrieve from the hash.
        /// Default is 7.
        /// </summary>
        public virtual int HashLength { get; set; } = 7;

        #endregion

        #region Task Output 

        /// <summary>
        /// Commit hash up to the number of characters set by <see cref="HashLength"/>.
        /// </summary>
        [Output]
        public virtual string? CommitHash { get; private set; }

        /// <summary>
        /// 'Modified' if the repository has uncommitted changes, 'Unmodified' if it doesn't. Will be left blank if unsupported (Only works if git bash is installed).
        /// </summary>
        [Output]
        public virtual string? Modified { get; private set; }

        /// <summary>
        /// Name of the current repository branch, if available.
        /// </summary>
        [Output]
        public virtual string? Branch { get; private set; }

        /// <summary>
        /// URL for the repository's origin.
        /// Null/Empty if unavailable.
        /// </summary>
        [Output]
        public virtual string? OriginUrl { get; private set; }

        /// <summary>
        /// Username the repository belongs to.
        /// Null/Empty if unavailable.
        /// </summary>
        [Output]
        public virtual string? GitUser { get; private set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(ProjectDir))
            {
                throw new ArgumentNullException(nameof(ProjectDir));
            }

            try
            {
                // Run status command
                string output = RunCommand("git", "status --porcelain=v2 --branch");

                string[] lines = output.Split('\n', '\r');

                // Parse the header lines
                Dictionary<string, string> statusHeaders = lines
                    .Where(line => line.StartsWith("# "))
                    .Select(line => line.Substring(2).Split(new char[] { ' ' }, 2))
                    .ToDictionary(lineParts => lineParts[0], lineParts => lineParts[1]);

                // Get the latest commit hash
                if (statusHeaders.TryGetValue("branch.oid", out string branchOid) && branchOid != "(initial)")
                {
                    CommitHash = branchOid.Substring(0, HashLength);
                }

                // Get the current branch
                if (statusHeaders.TryGetValue("branch.head", out string branchHead) && branchHead != "(detached)")
                {
                    Branch = branchHead;
                }

                // Check if there are any uncommited changes
                // # = Header, 1 = Changed, 2 = Moved, u = Unmerged, ? = Untracked, ! = Ignored
                if (lines.Any(line => line.StartsWith("1") || line.StartsWith("2") || line.StartsWith("u")))
                {
                    Modified = "Modified";
                }
                else
                {
                    Modified = "Unmodified";
                }

                // Get the current branch's upstream
                if (statusHeaders.TryGetValue("branch.upstream", out string branchUpstream))
                {
                    // Extract the remote
                    string remote = branchUpstream.Substring(0, branchUpstream.IndexOf('/'));

                    // Get the remote's URL
                    OriginUrl = RunCommand("git", $"remote get-url {remote}");

                    // Try to parse out the GitHub username
                    Match httpMatch = GitHubHttpRegex.Match(OriginUrl);
                    if(httpMatch.Success) {
                        GitUser = httpMatch.Groups["username"].Value;
                    }
                    else {
                        Match sshMatch = GitHubSshRegex.Match(OriginUrl);
                        if(sshMatch.Success) {
                            GitUser = sshMatch.Groups["username"].Value;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Log.LogWarningFromException(e);
                return true;
            }
        }

        private string RunCommand(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = this.ProjectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = Process.Start(startInfo);

            // To avoid deadlocks, read stdout/stderr asynchronously
            // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.standarderror
            string errorString = "";
            string outputString = "";
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => { errorString += e.Data; });
            process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => { outputString += e.Data; });
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            // Wait for exit
            bool exited = process.WaitForExit(1000);
            if (!exited || !process.HasExited)
            {
                process.Kill();
                throw new TimeoutException($"Process '{command} {arguments}' timed out.");
            }

            // Check stderr and exit code
            if (!string.IsNullOrWhiteSpace(errorString))
            {
                throw new Exception(errorString);
            }
            else if (process.ExitCode != 0)
            {
                throw new Exception($"Process '{command} {arguments}' exited with non-zero exit code {process.ExitCode}.");
            }
            else
            {
                return outputString;
            }
        }
    }
}
