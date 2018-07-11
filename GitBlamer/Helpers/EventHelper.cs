using EnvDTE;

namespace GitBlamer.Helpers
{
    public class EventHelper
    {
        private DTE _dte;
        private WindowEvents _windowEvents;

        public EventHelper(DTE dte)
        {
            _dte = dte;
            _windowEvents = dte.Events.WindowEvents;

            _windowEvents.WindowActivated += _windowEvents_WindowActivated;
        }

        private void _windowEvents_WindowActivated(Window GotFocus, Window LostFocus) => CommandHelper.Reset(_dte);
    }
}
