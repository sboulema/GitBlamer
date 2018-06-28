using GitBlamer.Helpers;
using GitBlamer.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitBlamer.ToolWindows
{
    /// <summary>
    /// Interaction logic for RevisionControl.xaml
    /// </summary>
    public partial class RevisionControl : UserControl
    {
        public RevisionControl()
        {
            InitializeComponent();
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var filePath = ((e.OriginalSource as TextBlock).DataContext as Change).Path;
                CommandHelper.Dte.ExecuteCommand("File.OpenFile", filePath);
            }
        }
    }
}
