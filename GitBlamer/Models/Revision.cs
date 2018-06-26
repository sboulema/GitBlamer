using System.Windows;

namespace GitBlamer.Models
{
    public class Revision
    {
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

        public Visibility HasMessage
        {
            get
            {
                return string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
