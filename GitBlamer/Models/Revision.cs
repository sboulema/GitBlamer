using Caliburn.Micro;
using GitBlamer.Helpers;
using System.Collections.Generic;
using System.Windows;

namespace GitBlamer.Models
{
    public class Revision : PropertyChangedBase
    {
        public Revision(string input)
        {
            var revision = input.Split('|');
            Hash = revision[0];
            Name = revision[1];
            Date = revision[2];
            Subject = revision[3];
            Message = revision[4];
        }

        public string Hash { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string CompareSide { get; set; }
        public string Date { get; set; }

        public string ShortHash => Hash.Substring(0, 8);

        public List<Change> Changes => new List<Change> { CommandHelper.GetChanges(this) };

        /// <summary>
        /// Display name to use in the 'Compare files' tab
        /// </summary>
        public object FileDisplayName { get; set; }

        /// <summary>
        /// File path of the file in repository we are getting revisions for
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// File path of the file on disk containing the specific revision content
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

        public TextAlignment CompareSideTextAlignment => CompareSide.Equals("Left") ? TextAlignment.Left : TextAlignment.Right;

        public Visibility CompareSideVisibility => CommandHelper.ShowBothCommitsCommandIsEnabled() ? Visibility.Visible : Visibility.Collapsed;
    }
}
