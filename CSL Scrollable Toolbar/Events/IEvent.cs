using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace ScrollableToolbar.Events
{
    /// <summary>
    /// An interface that can be inherited by classes that monitor for various events in the game.
    /// Every class that inherits from this interface, will automatically be started and stopped.
    /// </summary>
    internal interface IEvent
    {
        /// <summary>
        /// Start monitoring for events.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        void Start(LoadMode mode);

        /// <summary>
        /// Stop monitoring for events.
        /// </summary>
        void Stop();
    }
}
