using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;

namespace GitBlamer.Models
{
    public class GitBlamerViewModel : PropertyChangedBase
    {
        public List<Revision> Revisions;
        public int currentIndex;

        public TextDocument BlameDocument { get; set; }      

        private Revision _currentRevision;
        public Revision CurrentRevision {
            get
            {
                if (Revisions == null) return null;
                return Revisions[currentIndex];
            }
            set
            {
                _currentRevision = value;
                NotifyOfPropertyChange();
            }
        }

        public void MoveToEarlierRevision()
        {
            currentIndex++;
            CurrentRevision = Revisions[currentIndex];
        }

        public void MoveToLaterRevision()
        {
            currentIndex--;
            CurrentRevision = Revisions[currentIndex];
        }

        public void SetBlameDocument(string text)
        {
            BlameDocument = new TextDocument { Text = text };
            NotifyOfPropertyChange("BlameDocument");
        }
    }
}
