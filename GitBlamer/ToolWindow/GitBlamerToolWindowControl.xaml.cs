namespace GitBlamer.ToolWindow
{
    using EnvDTE;
    using GitBlamer.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Process = System.Diagnostics.Process;

    public partial class GitBlamerToolWindowControl : UserControl
    {
        public DTE DTE;
        internal readonly GitBlamerViewModel ViewModel;

        public GitBlamerToolWindowControl()
        {
            InitializeComponent();

            ViewModel = new GitBlamerViewModel();
            DataContext = ViewModel;       
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveToEarlierRevision();
            GetBlame(ViewModel.currentIndex);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.MoveToLaterRevision();
            GetBlame(ViewModel.currentIndex);
        }

        public void GetRevisions()
        {
            var revisions = new List<Revision>();

            var solutionDir = GetSolutionDir();

            if (string.IsNullOrEmpty(solutionDir) || DTE.ActiveDocument == null) return;

            var currentFilePath = DTE.ActiveDocument.FullName;

            var result = StartProcessGitResult($"log --format=\"%h|%an|%s\" {currentFilePath}", solutionDir);

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

            ViewModel.Revisions = revisions;
        }

        public void GetBlame(int index)
        {
            var result = StartProcessGitResult($"blame -s {ViewModel.Revisions[index].ShortSha} {DTE.ActiveDocument.FullName}", GetSolutionDir());
            ViewModel.SetBlameDocument(result.Replace(";", Environment.NewLine));
        }

        private string GetSolutionDir()
        {
            string fileName = DTE.Solution.FullName;
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