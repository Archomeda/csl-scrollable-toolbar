using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace ScrollableToolbar
{
    public class Mod : IUserMod
    {
        internal const string FriendlyName = "Scrollable Toolbar";
        internal const string AssemblyName = "ScrollableToolbar";
        internal const ulong WorkshopId = 451700838;

        public string Name
        {
            get { return FriendlyName; }
        }

        public string Description
        {
            get { return "Makes the toolbar scrollable with your mouse wheel. Save your left mouse button!"; }
        }
    }
}
