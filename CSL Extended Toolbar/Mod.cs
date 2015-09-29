using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using CommonShared;
using CommonShared.Utils;
using ExtendedToolbar.Defs;
using ExtendedToolbar.Migration;
using ExtendedToolbar.UI;
using ICities;
using UnityEngine;

namespace ExtendedToolbar
{
    public class Mod : UserModBase<Mod>
    {
        protected override ulong WorkshopId { get { return 451700838; } }

        internal Configuration Settings { get; private set; }

        internal string SettingsFilename { get; private set; }

        #region UserModBase members

        public override string Name
        {
            get { return "Extended Toolbar"; }
        }

        public override string Description
        {
            get { return "Adds a button that allows you to extend the toolbar to full width"; }
        }

        public override void OnModInitializing()
        {
            this.SettingsFilename = Path.Combine(FileUtils.GetStorageFolder(this), "ExtendedToolbar.yml");
            this.Load();

            this.Log.Debug("Mod initialized");
        }

        public override void OnModUninitializing()
        {
            this.Unload();
            this.Log.Debug("Mod uninitialized");
        }

        public override void OnGameLoaded(LoadMode mode)
        {
            if (this.Settings.Features.ToolbarToggleExtendedWidth)
                this.EnableToggleToolbarWidth(mode);
            else
                this.Log.Debug("Skipping feature ToolbarToggleExtendedWidth as it's disabled");

            this.Log.Debug("Mod loaded in-game");
        }

        public override void OnGameUnloading()
        {
            this.DisableToggleToolbarWidth();
        }

        #endregion


        #region Loading / Unloading

        private void Load()
        {
            // We have to properly migrate the outdated XML configuration file
            try
            {
                string oldXmlSettingsFilename = Path.Combine(Path.GetDirectoryName(this.SettingsFilename), Path.GetFileNameWithoutExtension(this.SettingsFilename)) + ".xml";
                if (File.Exists(oldXmlSettingsFilename) && !File.Exists(this.SettingsFilename))
                {
                    this.Settings = Configuration.LoadConfig(oldXmlSettingsFilename, new ConfigurationMigrator());
                    this.Settings.SaveConfig(this.SettingsFilename);
                    File.Delete(oldXmlSettingsFilename);
                }
                else
                {
                    this.Settings = Configuration.LoadConfig(this.SettingsFilename, new ConfigurationMigrator());
                }
            }
            catch (Exception ex)
            {
                this.Log.Warning("An error occured while loading the settings, default values will be used instead: {0}", ex);
                this.Settings = new Configuration();
            }

            this.Log.EnableDebugLogging = this.Settings.ExtraDebugLogging;
            if (this.Settings.ExtraDebugLogging)
            {
                this.Log.Warning("Extra debug logging is enabled, please use this only to get more information while hunting for bugs; don't use this when playing normally!");
            }
        }

        private void Unload()
        {
            try
            {
                this.Settings.SaveConfig(this.SettingsFilename);
            }
            catch (Exception ex)
            {
                this.Log.Warning("An error occured while saving the settings, the settings are not saved: {0}", ex);
            }
        }

        #endregion


        /// <summary>
        /// Patches the width of the toolbar to contain more items at once.
        /// </summary>
        private void EnableToggleToolbarWidth(LoadMode mode)
        {
            // We only add our switch mode button if the toolbar width hasn't been changed by some other mod, in order to prevent incompatibility
            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            Toolbar.CreateToolbarControlBox(mode);
            Toolbar.CreateToggleToolbarWidthButton(mode);
        }

        /// <summary>
        /// Reverts the width of the toolbar to its original value.
        /// </summary>
        private void DisableToggleToolbarWidth()
        {
            Toolbar.RemoveToolbarControlBox();
        }
    }
}
