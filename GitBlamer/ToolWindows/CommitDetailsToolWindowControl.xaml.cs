namespace GitBlamer.ToolWindows
{
    using EnvDTE;
    using GitBlamer.Helpers;
    using GitBlamer.Models;
    using System.Windows.Controls;
    using System.Windows.Input;

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

        private void ShowBothCommits_Click(object sender, System.Windows.RoutedEventArgs e)
            => ViewModel.ShowBothCommits = !ViewModel.ShowBothCommits;

        private void FlipCommits_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Revision1.GridRow = ViewModel.Revision1.GridRow == 1 ? 3 : 1;
            ViewModel.Revision2.GridRow = ViewModel.Revision2.GridRow == 1 ? 3 : 1;
        }
    }
}