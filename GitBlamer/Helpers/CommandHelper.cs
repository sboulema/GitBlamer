using EnvDTE;
using GitBlamer.Models;
using Microsoft.VisualStudio.Shell.Interop;
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
        public static string RevisionPath;
        public static string FilePath;
        public static List<Revision> Revisions;
        public static CommitDetailsViewModel ViewModel;
        public static IVsImageService2 ImageService;
        public static DTE Dte;
        private static Direction currentDirection;

        public static Revision SaveRevisionToFile(DTE dte, Revision revision, int gridRow, string compareSide)
        {
            var fileName = Path.GetFileNameWithoutExtension(revision.FilePath);
            var fileExtension = Path.GetExtension(revision.FilePath);
            var tempPath = Path.GetTempPath();
            var revisionPath = Path.Combine(tempPath, $"{fileName};{revision.ShortSha}{fileExtension}");

            File.WriteAllText(revisionPath, GetText(revision));

            revision.RevisionPath = revisionPath;
            revision.FileDisplayName = $"{fileName}{fileExtension};{revision.ShortSha}";
            revision.GridRow = gridRow;
            revision.CompareSide = compareSide;

            return revision;
        }

        /// <summary>
        /// Get all revisions for a file
        /// </summary>
        /// <param name="dte"></param>
        /// <returns></returns>
        public static List<Revision> GetRevisions(DTE dte)
        {
            if (Revisions == null)
            {
                var revisions = new List<Revision>();

                var solutionDir = GetSolutionDir(dte);

                if (string.IsNullOrEmpty(solutionDir) || dte.ActiveDocument == null) return revisions;

                var result = StartProcessGitResult($"log --follow --name-only --format=\"%h|%an|%ad|%s|%b\" {FilePath}", solutionDir);

                if (!result.Any())
                {
                    Revisions = revisions;
                    return revisions;
                }

                for (var i = 0; i < result.Count; i = i + 3)
                {
                    var lines = result.Skip(i).Take(3).ToList();

                    if (lines.Count == 3 && lines[0].Contains("|"))
                    {
                        var revision = new Revision(lines[0]);
                        revision.FilePath = lines[2];
                        revisions.Add(revision);
                    }
                }

                Revisions = revisions;
            }

            return Revisions;
        }

        /// <summary>
        /// Get the full text for a given file revision
        /// </summary>
        /// <param name="revision"></param>
        /// <returns></returns>
        public static string GetText(Revision revision)
        {
            var fileDirectory = Path.GetDirectoryName(FilePath);
            var fileName = Path.GetFileName(revision.FilePath);

            return string.Join(Environment.NewLine, StartProcessGitResult($"show {revision.ShortSha}:./{fileName}", fileDirectory));
        }

        public static Change GetChanges(Revision revision)
        {
            var solutionDir = GetSolutionDir(Dte);
            var fileDirectory = Path.GetDirectoryName(FilePath);

            var results = StartProcessGitResult($" show --name-status --pretty=\"\" {revision.ShortSha}", fileDirectory);

            return MakeTreeFromChanges(results, solutionDir, "Changes");
        }

        public static Change MakeTreeFromChanges(List<string> results, string solutionDir, 
            string rootNodeName = "", char separator = '/')
        {
            var rootNode = new Change(rootNodeName);
            foreach (var line in results.Where(x => !string.IsNullOrEmpty(x.Trim())))
            {
                var currentNode = rootNode;
                var status = line.Split('\t')[0];
                var path = line.Split('\t')[1];
                var pathItems = path.Split(separator);
                foreach (var item in pathItems)
                {
                    var tmp = currentNode.Changes.Cast<Change>().Where(x => x.Name.Equals(item));

                    if (tmp.Count() > 0)
                    {
                        currentNode = tmp.Single();
                    }
                    else
                    {
                        var newNode = new Change(item, Path.Combine(solutionDir, path.Replace("/", "\\")), status);
                        currentNode.Changes.Add(newNode);
                        currentNode = newNode;
                    }
                }
            }
            return rootNode;
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

        /// <summary>
        /// Execute a Git command and return the output
        /// </summary>
        /// <param name="commands">Git command to be executed</param>
        /// <returns>Git output</returns>
        public static List<string> StartProcessGitResult(string commands, string solutionDir)
        {
            var output = new List<string>();
            var error = new List<string>();

            if (string.IsNullOrEmpty(solutionDir)) return new List<string>();

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
                output.Add(proc.StandardOutput.ReadLine());
            }
            while (!proc.StandardError.EndOfStream)
            {
                error.Add(proc.StandardError.ReadLine());
            }

            return output.Count == 0 ? error : output;
        }

        /// <summary>
        /// Move between revisions of a file
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="previous">Move to a previous revision or later revision</param>
        public static void MoveRevision(DTE dte, Direction direction)
        {
            if (ViewModel == null)
            {
                ViewModel = new CommitDetailsViewModel();
            }      

            // Close any active 'Compare Files' tab
            if (dte.ActiveWindow.Caption.Contains(" vs. "))
            {
                dte.ActiveWindow.Close();
            }

            if (dte.ActiveDocument == null) return;

            // If we changed files clear any history
            if (!dte.ActiveDocument.FullName.Equals(RevisionPath))
            {
                Revisions = null;
                FilePath = null;
                RevisionPath = null;
            }

            // Save current file as active
            if (string.IsNullOrEmpty(FilePath))
            {
                FilePath = dte.ActiveDocument.FullName;
            }

            // Get Git revisions for file
            var revisions = GetRevisions(dte);

            // Can we make the move between revisions
            if (!revisions.Any() ||
                (direction == Direction.Previous && CurrentIndex >= revisions.Count - 1) || 
                (direction == Direction.Later && CurrentIndex == 0))
            {
                return;
            }

            // Handle switching direction
            if (currentDirection != direction)
            {
                if (currentDirection == Direction.Previous)
                {
                    CurrentIndex--;
                }
                else
                {
                    CurrentIndex++;
                }
            }
            currentDirection = direction;

            // Move between revisions
            if (direction == Direction.Previous)
            {
                ViewModel.LaterRevision = SaveRevisionToFile(dte, revisions[CurrentIndex], 3, "Right");
                RevisionPath = ViewModel.LaterRevision.RevisionPath;

                CurrentIndex++;

                ViewModel.PreviousRevision = SaveRevisionToFile(dte, revisions[CurrentIndex], 1, "Left");
            }
            else
            {
                ViewModel.PreviousRevision = SaveRevisionToFile(dte, revisions[CurrentIndex], 1, "Left");
                RevisionPath = ViewModel.LaterRevision.RevisionPath;

                CurrentIndex--;

                ViewModel.LaterRevision = SaveRevisionToFile(dte, revisions[CurrentIndex], 3, "Right");
            }

            ViewModel.NotifyOfRevisionMove();

            // Open 'Compare Files' tab for the two revisions
            dte.ExecuteCommand("Tools.DiffFiles", $"\"{ViewModel.PreviousRevision.RevisionPath}\" \"{ViewModel.LaterRevision.RevisionPath}\" \"{ViewModel.PreviousRevision.FileDisplayName}\" \"{ViewModel.LaterRevision.FileDisplayName}\"");

            File.Delete(ViewModel.PreviousRevision.RevisionPath);
            File.Delete(ViewModel.LaterRevision.RevisionPath);
        }

        public static bool PreviousRevisionCommandIsEnabled(DTE dte) 
            => Revisions == null || CurrentIndex < GetRevisions(dte).Count - 1;

        public static bool PreviousRevisionCommandIsEnabled()
            => Revisions == null || CurrentIndex < Revisions.Count - 1;

        public static bool LaterRevisionCommandIsEnabled()
            => Revisions != null && Revisions.Any() && CurrentIndex > 0;
    }
}
