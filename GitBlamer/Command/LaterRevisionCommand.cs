using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE;
using GitBlamer.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GitBlamer
{
    internal sealed class LaterRevisionCommand
    {
        public const int CommandId = 0x0200;
        public static readonly Guid CommandSet = new Guid("0d5a4968-48e2-45aa-987b-0196b9c63d99");

        private readonly AsyncPackage package;
        private readonly DTE _dte;

        private LaterRevisionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            _dte = (package as GitBlamerPackage).DTE;
        }

        public static LaterRevisionCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in GitBlamerCommand's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new LaterRevisionCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(CommandHelper.FilePath))
            {
                CommandHelper.FilePath = _dte.ActiveDocument.FullName;
            }

            if (_dte.ActiveWindow.Caption.Contains(" vs. "))
            {
                _dte.ActiveWindow.Close();
            }

            var revisions = CommandHelper.GetRevisions(_dte);

            var rev1 = CommandHelper.SaveRevisionToFile(_dte, revisions[CommandHelper.CurrentIndex]);
            CommandHelper.CurrentIndex--;
            var rev2 = CommandHelper.SaveRevisionToFile(_dte, revisions[CommandHelper.CurrentIndex]);

            _dte.ExecuteCommand("Tools.DiffFiles", $"\"{rev2}\" \"{rev1}\"");

            File.Delete(rev1);
            File.Delete(rev2);
        }
    }
}
