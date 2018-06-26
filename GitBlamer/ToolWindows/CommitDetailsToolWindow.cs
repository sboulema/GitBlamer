namespace GitBlamer.ToolWindows
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("84649ca3-0c6c-487b-a3d7-6356745c77d2")]
    public class CommitDetailsToolWindow : ToolWindowPane
    {
        private readonly CommitDetailsToolWindowControl _control;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitDetailsToolWindow"/> class.
        /// </summary>
        public CommitDetailsToolWindow() : base(null)
        {
            Caption = "GitBlamer Commit Details";
            _control = new CommitDetailsToolWindowControl(null);
            Content = _control;
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            var gitBlamerPackage = Package as GitBlamerPackage;
            if (_control.Dte == null && gitBlamerPackage.DTE != null)
            {
                _control.Dte = gitBlamerPackage.DTE;
            }
        }
    }
}
