using Caliburn.Micro;
using GitBlamer.Helpers;
using System.Windows;

namespace GitBlamer.Models
{
    public class CommitDetailsViewModel : PropertyChangedBase
    {
        private Revision _revision1;
        public Revision Revision1
        {
            get
            {
                return _revision1;
            }
            set
            {
                _revision1 = value;
                NotifyOfPropertyChange();
            }
        }

        private Revision _revision2;
        public Revision Revision2
        {
            get
            {
                return _revision2;
            }
            set
            {
                _revision2 = value;
                NotifyOfPropertyChange();
            }
        }

        public bool PreviousRevisionCommandIsEnabled
        {
            get
            {
                return CommandHelper.PreviousRevisionCommandIsEnabled();
            }
        }

        public bool LaterRevisionCommandIsEnabled
        {
            get
            {
                return CommandHelper.LaterRevisionCommandIsEnabled();
            }
        }

        public bool PreviousRevisionCommandGrayscale => !PreviousRevisionCommandIsEnabled;
        public bool LaterRevisionCommandGrayscale => !LaterRevisionCommandIsEnabled;

        private bool _showBothCommits;
        public bool ShowBothCommits
        {
            get
            {
                return _showBothCommits;
            }
            set
            {
                _showBothCommits = value;
                NotifyOfPropertyChange("ShowBothCommits");
                NotifyOfPropertyChange("BothCommitsVisible");
            }
        }

        public Visibility BothCommitsVisible => ShowBothCommits ? Visibility.Visible : Visibility.Collapsed;

        public void MoveRevision()
        {
            NotifyOfPropertyChange("PreviousRevisionCommandIsEnabled");
            NotifyOfPropertyChange("LaterRevisionCommandIsEnabled");
            NotifyOfPropertyChange("PreviousRevisionCommandGrayscale");
            NotifyOfPropertyChange("LaterRevisionCommandGrayscale");
        }
    }
}
