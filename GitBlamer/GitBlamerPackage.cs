using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace GitBlamer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GitBlamerPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(GitBlamer.ToolWindow.GitBlamerToolWindow))]
    public sealed class GitBlamerPackage : AsyncPackage
    {
        public const string PackageGuidString = "492cc0c2-bdd8-4279-a8a8-1c353760ad68";
        public DTE DTE;

        public GitBlamerPackage()
        {
        }

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            DTE = await GetServiceAsync(typeof(DTE)) as DTE;

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await GitBlamerCommand.InitializeAsync(this);
        }

        #endregion
    }
}
