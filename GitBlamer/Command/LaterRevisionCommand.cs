﻿using System;
using System.ComponentModel.Design;
using EnvDTE;
using GitBlamer.Helpers;
using GitBlamer.Models;
using Microsoft.VisualStudio.Shell;
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
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);

            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;

            commandService.AddCommand(menuItem);

            _dte = (package as GitBlamerPackage).DTE;
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ((OleMenuCommand)sender).Enabled = CommandHelper.LaterRevisionCommandIsEnabled();
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

            CommandHelper.MoveRevision(_dte, Direction.Later);
        }
    }
}
