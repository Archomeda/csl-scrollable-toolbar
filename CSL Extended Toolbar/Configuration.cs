using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CommonShared;
using CommonShared.Configuration;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar
{
    [XmlRoot("Configuration")]
    public class Configuration : VersionedConfig
    {
        [XmlRoot("Features")]
        public class FeaturesConfig
        {
            public FeaturesConfig()
            {
                this.ToolbarToggleExtendedWidth = true;
            }

            public bool ToolbarToggleExtendedWidth { get; set; }
        }

        [XmlRoot("State")]
        public class StateConfig
        {
            public StateConfig()
            {
                this.ToolbarHasExtendedWidth = false;
            }

            public bool ToolbarHasExtendedWidth { get; set; }
        }

        public Configuration()
        {
            this.Version = 1;

            this.Features = new FeaturesConfig();
            this.State = new StateConfig();
            this.ExtraDebugLogging = false;
        }

        public FeaturesConfig Features { get; set; }

        public StateConfig State { get; set; }

        public bool ExtraDebugLogging { get; set; }
    }
}
