using Caliburn.Micro;
using System.Windows;

namespace GitBlamer.Models
{
    public class Revision : PropertyChangedBase
    {
        public Revision(string input)
        {
            var revision = input.Split('|');
            ShortSha = revision[0];
            Name = revision[1];
            Subject = revision[2];
            Message = revision[3];
        }

        public string ShortSha { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Display name to use in the 'Compare files' tab
        /// </summary>
        public object FileDisplayName { get; set; }

        /// <summary>
        /// File path of the file we are getting revisions for
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File path of the file containing the revision content
        /// </summary>
        public string RevisionPath { get; set; }

        private int _gridRow;
        public int GridRow {
            get => _gridRow;
            set
            {
                _gridRow = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility HasMessage => string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;
    }
}
