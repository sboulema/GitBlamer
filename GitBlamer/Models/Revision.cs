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
            ShortSha = revision[0];
            Name = revision[1];
            Date = revision[2];
            Subject = revision[3];
            Message = revision[4];
        }

        public string ShortSha { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string CompareSide { get; set; }
        public string Date { get; set; }

        public List<Change> Changes => new List<Change> { CommandHelper.GetChanges(this) };

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

        public TextAlignment CompareSideTextAlignment => CompareSide.Equals("Left") ? TextAlignment.Left : TextAlignment.Right;
    }
}
