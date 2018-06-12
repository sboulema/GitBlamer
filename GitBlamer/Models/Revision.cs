namespace GitBlamer.Models
{
    public class Revision
    {
        public string ShortSha { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Message { get; internal set; }
        public object FileDisplayName { get; internal set; }
        public string FilePath { get; internal set; }
    }
}
