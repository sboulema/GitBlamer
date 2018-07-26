using Caliburn.Micro;
using GitBlamer.Helpers;
using System.Windows;

namespace GitBlamer.Models
{
    public class CommitDetailsViewModel : PropertyChangedBase
    {
        private Revision _previousRevision;
        public Revision PreviousRevision
        {
            get => _previousRevision;
            set
            {
                _previousRevision = value;
                NotifyOfPropertyChange();
            }
        }

        private Revision _laterRevision;
        public Revision LaterRevision
        {
            get => _laterRevision;
            set
            {
                _laterRevision = value;
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// File path of the file on disk we are getting revisions for
        /// </summary>
        public string DiskPath { get; set; }

        public bool PreviousRevisionCommandIsEnabled => CommandHelper.PreviousRevisionCommandIsEnabled();
        public bool LaterRevisionCommandIsEnabled => CommandHelper.LaterRevisionCommandIsEnabled();
        public bool ShowBothCommitsCommandIsEnabled => CommandHelper.ShowBothCommitsCommandIsEnabled();
        public bool PreviousRevisionCommandGrayscale => !PreviousRevisionCommandIsEnabled;
        public bool LaterRevisionCommandGrayscale => !LaterRevisionCommandIsEnabled;
        public bool ShowBothCommitsCommandGrayscale => !ShowBothCommitsCommandIsEnabled;

        private bool _showBothCommits;
        public bool ShowBothCommits
        {
            get => _showBothCommits;
            set
            {
                _showBothCommits = value;
                NotifyOfPropertyChange("ShowBothCommits");
                NotifyOfPropertyChange("BothCommitsVisible");
            }
        }

        public Visibility BothCommitsVisible => ShowBothCommits ? Visibility.Visible : Visibility.Collapsed;

        public void NotifyOfRevisionMove()
        {
            NotifyOfPropertyChange("PreviousRevisionCommandIsEnabled");
            NotifyOfPropertyChange("LaterRevisionCommandIsEnabled");
            NotifyOfPropertyChange("PreviousRevisionCommandGrayscale");
            NotifyOfPropertyChange("LaterRevisionCommandGrayscale");
        }
    }
}
