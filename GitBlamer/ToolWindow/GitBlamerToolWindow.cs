namespace GitBlamer.ToolWindow
{
    using System;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;

    [Guid("e1f10d62-861c-4207-8e19-e4590f4d201e")]
    public class GitBlamerToolWindow : ToolWindowPane
    {
        private readonly GitBlamerToolWindowControl _control;
        private WindowEvents _windowEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitBlamerToolWindow"/> class.
        /// </summary>
        public GitBlamerToolWindow() : base(null)
        {
            Caption = "Git Blamer";
            _control = new GitBlamerToolWindowControl();
            Content = _control;     
        }

        public override void OnToolWindowCreated()
        {
            var gitBlamerPackage = Package as GitBlamerPackage;

            _control.DTE = gitBlamerPackage.DTE;

            _windowEvents = gitBlamerPackage.DTE.Events.WindowEvents;
            _windowEvents.WindowActivated += _windowEvents_WindowActivated;       
        }

        private void _windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            _control.GetRevisions();
        }
    }
}
