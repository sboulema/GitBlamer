using EnvDTE;
using GitBlamer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Process = System.Diagnostics.Process;

namespace GitBlamer.Helpers
{
    public static class CommandHelper
    {
        public static int CurrentIndex;
        public static string FilePath;
        public static List<Revision> Revisions;

        public static string SaveRevisionToFile(DTE dte, Revision revision)
        {
            var fileName = Path.GetFileName(FilePath);
            var tempPath = Path.GetTempPath();
            var revisionPath = Path.Combine(tempPath, $"{fileName};{revision.ShortSha}");

            File.WriteAllText(revisionPath, GetText(dte, revision, FilePath));

            return revisionPath;
        }

        public static List<Revision> GetRevisions(DTE dte)
        {
            if (Revisions == null)
            {
                var revisions = new List<Revision>();

                var solutionDir = GetSolutionDir(dte);

                if (string.IsNullOrEmpty(solutionDir) || dte.ActiveDocument == null) return revisions;

                var result = StartProcessGitResult($"log --format=\"%h|%an|%s\" {FilePath}", solutionDir);

                if (string.IsNullOrEmpty(result))
                {
                    Revisions = revisions;
                    return revisions;
                }

                foreach (var revision in result.Split(';'))
                {
                    var info = revision.Split('|');
                    revisions.Add(new Revision
                    {
                        ShortSha = info[0],
                        Name = info[1],
                        Subject = info[2]
                    });
                }

                Revisions = revisions;
            }

            return Revisions;
        }

        public static string GetBlame(DTE dte, Revision revision, string filePath)
        {
            var relativeFilePath = GetRelativePath(GetSolutionDir(dte), filePath);
            var result = StartProcessGitResult($"blame -s {revision.ShortSha} {filePath}", GetSolutionDir(dte));
            return result.Replace(";", Environment.NewLine);
        }

        public static string GetText(DTE dte, Revision revision, string filePath)
        {
            var relativeFilePath = GetRelativePath(GetSolutionDir(dte), filePath);
            return StartProcessGitResult($"show {revision.ShortSha}:{relativeFilePath}", GetSolutionDir(dte)).Replace(";", Environment.NewLine);
        }

        private static string GetSolutionDir(DTE dte)
        {
            string fileName = dte.Solution.FullName;
            if (!string.IsNullOrEmpty(fileName))
            {
                var path = Path.GetDirectoryName(fileName);
                return FindGitdir(path);
            }
            return string.Empty;
        }

        private static string FindGitdir(string path)
        {
            var di = new DirectoryInfo(path);
            if (di.GetDirectories().Any(d => d.Name.Equals(".git")))
            {
                return di.FullName;
            }
            if (di.Parent != null)
            {
                return FindGitdir(di.Parent.FullName);
            }
            return string.Empty;
        }

        private static string GetRelativePath(string solutiondir, string filePath)
        {
            return filePath.Replace(solutiondir, string.Empty).Replace("\\", "/").TrimStart('/');
        }

        /// <summary>
        /// Execute a Git command and return the output
        /// </summary>
        /// <param name="commands">Git command to be executed</param>
        /// <returns>Git output</returns>
        public static string StartProcessGitResult(string commands, string solutionDir)
        {
            if (string.IsNullOrEmpty(solutionDir)) return string.Empty;

            var output = string.Empty;
            var error = string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{solutionDir}\" && git {commands}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                output += proc.StandardOutput.ReadLine() + ";";
            }
            while (!proc.StandardError.EndOfStream)
            {
                error += proc.StandardError.ReadLine();
            }

            return string.IsNullOrEmpty(output) ? error : output.TrimEnd(';');
        }
    }
}
