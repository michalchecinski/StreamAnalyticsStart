using System.Collections.Generic;
using EventReceiverWebApp.Models;

namespace EventReceiverWebApp.Services
{
    public static class AtmEventAggregator
    {
        private static List<AtmEvent> _events = new List<AtmEvent>();

        public static void LogEvent(AtmEvent e)
        {
            if (_events.Count < 1024) // Avoid unconstrained memory growth
            {
                _events.Add(e);
            }
        }

        public static AtmEvent[] GetLoggedEvents()
        {
            var events = _events.ToArray();
            _events.Clear();
            return events;
        }
    }
}