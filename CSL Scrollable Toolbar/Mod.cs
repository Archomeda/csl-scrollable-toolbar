using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace ScrollableToolbar
{
    public class Mod : IUserMod
    {
        public string Name
        {
            get { return "Scrollable Toolbar"; }
        }

        public string Description
        {
            get { return "Makes the toolbar scrollable with your mouse wheel. Save your left mouse button!"; }
        }
    }
}
