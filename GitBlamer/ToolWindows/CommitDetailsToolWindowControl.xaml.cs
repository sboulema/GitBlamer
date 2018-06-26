namespace GitBlamer.ToolWindows
{
    using EnvDTE;
    using GitBlamer.Helpers;
    using GitBlamer.Models;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CommitDetailsToolWindowControl.
    /// </summary>
    public partial class CommitDetailsToolWindowControl : UserControl
    {
        public CommitDetailsViewModel ViewModel;
        public DTE Dte;

        public CommitDetailsToolWindowControl(DTE dte)
        {
            InitializeComponent();

            if (CommandHelper.ViewModel == null)
            {
                CommandHelper.ViewModel = new CommitDetailsViewModel();
            }

            ViewModel = CommandHelper.ViewModel;
            DataContext = ViewModel;

            Dte = dte;
        }

        private void PreviousRevisionButton_Click(object sender, System.Windows.RoutedEventArgs e) 
            => CommandHelper.MoveRevision(Dte, true);

        private void LaterRevisionButton_Click(object sender, System.Windows.RoutedEventArgs e) 
            => CommandHelper.MoveRevision(Dte, false);
    }
}