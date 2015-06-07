using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonShared;
using CommonShared.Utils;
using ICities;

namespace ScrollableToolbar
{
    public class Mod : IUserMod
    {
        internal static Configuration Settings;
        internal static string SettingsFilename = Path.Combine(FileUtils.GetStorageFolder(), "ScrollableToolbar.xml");
        internal static Logger Log = new Logger();

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
