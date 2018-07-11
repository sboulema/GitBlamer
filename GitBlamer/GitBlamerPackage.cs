using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using GitBlamer.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitBlamer
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GitBlamerPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(GitBlamer.ToolWindows.CommitDetailsToolWindow))]
    public sealed class GitBlamerPackage : AsyncPackage
    {
        public const string PackageGuidString = "492cc0c2-bdd8-4279-a8a8-1c353760ad68";
        public DTE DTE;
        private EventHelper _eventHelper;

        public GitBlamerPackage()
        {
        }

        #region Package Members

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            DTE = await GetServiceAsync(typeof(DTE)) as DTE;
            CommandHelper.Dte = DTE;
            _eventHelper = new EventHelper(DTE);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await PreviousRevisionCommand.InitializeAsync(this);
            await LaterRevisionCommand.InitializeAsync(this);
            await InfoRevisionCommand.InitializeAsync(this);
            await ToolWindows.CommitDetailsToolWindowCommand.InitializeAsync(this);

            CommandHelper.ImageService = await GetServiceAsync(typeof(SVsImageService)) as IVsImageService2;
        }

        #endregion
    }
}
