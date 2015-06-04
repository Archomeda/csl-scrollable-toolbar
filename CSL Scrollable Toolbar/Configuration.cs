﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ScrollableToolbar.Utils;

namespace ScrollableToolbar
{
    [XmlRoot("Configuration")]
    public class Configuration
    {
        [XmlRoot("Features")]
        public class FeaturesConfig
        {
            public FeaturesConfig()
            {
                this.ToolbarScrolling = true;
                this.ToolbarToggleExtendedWidth = true;
            }

            public bool ToolbarScrolling { get; set; }

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
            this.Features = new FeaturesConfig();
            this.State = new StateConfig();
            this.ExtraDebugLogging = false;
        }

        public FeaturesConfig Features { get; set; }

        public StateConfig State { get; set; }

        public bool ExtraDebugLogging { get; set; }

        [XmlIgnore]
        public static Configuration Instance { get; private set; }


        /// <summary>
        /// Load configuration.
        /// </summary>
        public static void Load()
        {
            string path = Path.Combine(FileUtils.GetDataFolder(), Mod.AssemblyName + ".xml");
            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    Instance = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(sr);
                }
                Logger.Info("Loaded configuration");
            }
            else
            {
                Instance = new Configuration();
                Logger.Info("No configuration file found, loaded default");
            }
        }

        /// <summary>
        /// Save configuration.
        /// </summary>
        public static void Save()
        {
            if (Instance == null)
            {
                Logger.Warning("Cannot save configuration when there's no configuration instance!");
                return;
            }

            string path = Path.Combine(FileUtils.GetDataFolder(), Mod.AssemblyName + ".xml");
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            using (StreamWriter sw = new StreamWriter(path))
            {
                new XmlSerializer(typeof(Configuration)).Serialize(sw, Instance);
            }
            Logger.Info("Saved configuration");
        }
    }
}
