﻿using EnvDTE;
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
        public static string RevisionPath;
        public static string FilePath;
        public static List<Revision> Revisions;
        public static CommitDetailsViewModel ViewModel;

        public static Revision SaveRevisionToFile(DTE dte, Revision revision)
        {
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var fileExtension = Path.GetExtension(FilePath);
            var tempPath = Path.GetTempPath();
            var revisionPath = Path.Combine(tempPath, $"{fileName};{revision.ShortSha}{fileExtension}");

            File.WriteAllText(revisionPath, GetText(revision, FilePath));

            revision.RevisionPath = revisionPath;
            revision.FilePath = FilePath;
            revision.FileDisplayName = $"{fileName}{fileExtension};{revision.ShortSha}";

            return revision;
        }

        public static List<Revision> GetRevisions(DTE dte)
        {
            if (Revisions == null)
            {
                var revisions = new List<Revision>();

                var solutionDir = GetSolutionDir(dte);

                if (string.IsNullOrEmpty(solutionDir) || dte.ActiveDocument == null) return revisions;

                var result = StartProcessGitResult($"log --follow --format=\"%h|%an|%s|%b\" {FilePath}", solutionDir);

                if (!result.Any())
                {
                    Revisions = revisions;
                    return revisions;
                }

                foreach (var revision in result)
                {
                    var info = revision.Split('|');
                    revisions.Add(new Revision
                    {
                        ShortSha = info[0],
                        Name = info[1],
                        Subject = info[2],
                        Message = info[3]
                    });
                }

                Revisions = revisions;
            }

            return Revisions;
        }

        public static string GetText(Revision revision, string filePath)
        {
            var fileDirectory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            return string.Join(Environment.NewLine, StartProcessGitResult($"show {revision.ShortSha}:./{fileName}", fileDirectory));
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

        public static string GetCurrentRevisionInfo()
        {
            var currentRevision = Revisions[CurrentIndex];

            return $"Revision: {currentRevision.ShortSha}" + Environment.NewLine +
                $"Author: {currentRevision.Name}" + Environment.NewLine +
                $"Subject: {currentRevision.Subject}" + Environment.NewLine +
                $"Message: {currentRevision.Message}";
        }

        /// <summary>
        /// Move between revisions of a file
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="previous">Move to a previous revision or later revision</param>
        public static void MoveRevision(DTE dte, bool previous)
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
            if ((previous && CurrentIndex >= revisions.Count - 1) || 
                (!previous && CurrentIndex == 0))
            {
                return;
            }

            ViewModel.Revision1 = SaveRevisionToFile(dte, revisions[CurrentIndex]);
            RevisionPath = ViewModel.Revision1.RevisionPath; 

            if (previous)
            {
                CurrentIndex++;
            }
            else
            {
                CurrentIndex--;
            }
            
            ViewModel.Revision2 = SaveRevisionToFile(dte, revisions[CurrentIndex]);

            ViewModel.MoveRevision();

            // Open 'Compare Files' tab for the two revisions
            dte.ExecuteCommand("Tools.DiffFiles", $"\"{ViewModel.Revision2.RevisionPath}\" \"{ViewModel.Revision1.RevisionPath}\" \"{ViewModel.Revision2.FileDisplayName}\" \"{ViewModel.Revision1.FileDisplayName}\"");

            File.Delete(ViewModel.Revision1.RevisionPath);
            File.Delete(ViewModel.Revision2.RevisionPath);
        }

        public static bool PreviousRevisionCommandIsEnabled(DTE dte) 
            => Revisions == null || CurrentIndex < GetRevisions(dte).Count - 1;

        public static bool PreviousRevisionCommandIsEnabled()
            => Revisions == null || CurrentIndex < Revisions.Count - 1;

        public static bool LaterRevisionCommandIsEnabled()
            => Revisions != null && Revisions.Any() && CurrentIndex > 0;
    }
}
