using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using CommonShared;
using CommonShared.Configuration;
using CommonShared.Extensions;
using CommonShared.Utils;
using ICities;
using ExtendedToolbar.Defs;
using ExtendedToolbar.UI;
using UnityEngine;
using CommonShared.Proxies.Plugins;
using ColossalFramework.Steamworks;

namespace ExtendedToolbar
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        internal static Mod Instance { get; private set; }

        internal Configuration Settings { get; private set; }
        internal string SettingsFilename { get; private set; }
        internal Logger Log { get; private set; }

        public string Name
        {
            get { return "Extended Toolbar"; }
        }

        public string Description
        {
            get { return "Adds a button that allows you to extend the toolbar to full width"; }
        }


        private void Init()
        {
            this.SettingsFilename = Path.Combine(FileUtils.GetStorageFolder(this), "ExtendedToolbar.xml");
            this.Log = new Logger(this.GetType().Assembly);
            Instance = this;

            this.Log.Debug("Mod initialized");
        }

        private void Load(LoadMode mode)
        {
            // This does not prevent other people from changing the source, recompiling it and republishing it,
            // but it does prevent other people from directly republishing the original files.
            // If you want to contribute, please submit a PR on GitHub here: https://github.com/Archomeda/csl-scrollable-toolbar
            IPluginInfoInteractor pluginInfo = PluginUtils.GetPluginInfo(this);
            if (pluginInfo.PublishedFileID != PublishedFileId.invalid && pluginInfo.PublishedFileID.AsUInt64 != 451700838)
            {
                // The mod is not published under my name
                this.Log.Error("YOU ARE CURRENTLY USING AN UNAUTHORIZED PUBLICATION OF THE MOD 'EXTENDED TOOLBAR' WITH FILE ID {0}.\r\nPLEASE USE THE VERSION THAT CAN BE FOUND HERE: http://steamcommunity.com/sharedfiles/filedetails/?id=451700838.\r\n\r\nThis version will not be loaded.", pluginInfo.PublishedFileID.AsUInt64);
                return;
            }

            try
            {
                this.Settings = VersionedConfig.LoadConfig<Configuration>(this.SettingsFilename);
            }
            catch (Exception ex)
            {
                this.Log.Error("An exception occured while loading the configuration. Default settings will be loaded instead. {0}", ex);
                this.Settings = new Configuration();
            }

            this.Log.EnableDebugLogging = this.Settings.ExtraDebugLogging;

            if (this.Settings.ExtraDebugLogging)
            {
                this.Log.Warning("Extra debug logging is enabled, please use this only to get more information while hunting for bugs; don't use this when playing normally!");
            }

            if (this.Settings.Features.ToolbarToggleExtendedWidth)
            {
                this.EnableToggleToolbarWidth(mode);
            }
            else
            {
                this.Log.Debug("Skipping feature ToolbarToggleExtendedWidth as it's disabled");
            }
        }

        private void Unload()
        {
            try
            {
                Mod.Instance.Settings.SaveConfig(Mod.Instance.SettingsFilename);
            }
            catch (Exception ex)
            {
                Mod.Instance.Log.Error("An exception occured while saving the configuration. Configuration has not been saved. {0}", ex);
            }

            this.DisableToggleToolbarWidth();
        }

        /// <summary>
        /// Our entry point. Here we load the mod.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            this.Init();
            base.OnLevelLoaded(mode);
            this.Load(mode);
        }

        /// <summary>
        /// Our exit point. Here we unload everything we have loaded.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            this.Unload();
        }

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
