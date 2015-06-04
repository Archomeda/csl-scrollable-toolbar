using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace ScrollableToolbar.Events
{
    internal static class EventsController
    {
        private static List<IEvent> events = new List<IEvent>();

        /// <summary>
        /// Start monitoring for various events in the game.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public static void StartEvents(LoadMode mode)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IEvent).IsAssignableFrom(p) && p.IsClass);

            foreach (var type in types)
            {
                IEvent @event = (IEvent)Activator.CreateInstance(type);
                events.Add(@event);
                @event.Start(mode);
            }
            Logger.Info("{0} event classes have been started", events.Count);
        }

        /// <summary>
        /// Stop monitoring for the events in the game.
        /// </summary>
        public static void StopEvents()
        {
            foreach (var @event in events)
            {
                @event.Stop();
            }
            Logger.Info("{0} event classes have been stopped", events.Count);
            events.Clear();
        }
    }
}
