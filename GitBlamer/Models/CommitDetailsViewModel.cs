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

        public bool PreviousRevisionCommandIsEnabled => CommandHelper.PreviousRevisionCommandIsEnabled();
        public bool LaterRevisionCommandIsEnabled => CommandHelper.LaterRevisionCommandIsEnabled();
        public bool PreviousRevisionCommandGrayscale => !PreviousRevisionCommandIsEnabled;
        public bool LaterRevisionCommandGrayscale => !LaterRevisionCommandIsEnabled;

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
