using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using CommonShared;
using CommonShared.Configuration;
using CommonShared.Events;
using CommonShared.Extensions;
using CommonShared.Utils;
using ICities;
using ScrollableToolbar.Defs;
using ScrollableToolbar.UI;
using UnityEngine;

namespace ScrollableToolbar
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        internal static Configuration Settings { get; private set; }
        internal static string SettingsFilename { get; private set; }
        internal static Logger Log { get; private set; }
        internal static Mod Instance { get; private set; }

        public string Name
        {
            get { return "Scrollable Toolbar"; }
        }

        public string Description
        {
            get { return "Makes the toolbar scrollable with your mouse wheel. Save your left mouse button!"; }
        }


        private void Init()
        {
            SettingsFilename = Path.Combine(FileUtils.GetStorageFolder(this), "ScrollableToolbar.xml");
            Log = new Logger(this);
            Instance = this;
        }

        private void Load(LoadMode mode)
        {
            try
            {
                Mod.Settings = VersionedConfig.LoadConfig<Configuration>(Mod.SettingsFilename);
            }
            catch (Exception ex)
            {
                Mod.Log.Error("An exception occured while loading the configuration. Default settings will be loaded instead. {0}", ex);
                Mod.Settings = new Configuration();
            }

            Mod.Log.EnableDebugLogging = Mod.Settings.ExtraDebugLogging;

            if (Mod.Settings.ExtraDebugLogging)
            {
                Mod.Log.Warning("Extra debug logging is enabled, please use this only to get more information while hunting for bugs; don't use this when playing normally!");
            }

            if (Mod.Settings.Features.ToolbarToggleExtendedWidth)
            {
                this.EnableToggleToolbarWidth(mode);
            }
            else
            {
                Mod.Log.Debug("Skipping feature ToolbarToggleExtendedWidth as it's disabled");
            }
        }

        private void Unload()
        {
            try
            {
                Mod.Settings.SaveConfig(Mod.SettingsFilename);
            }
            catch (Exception ex)
            {
                Mod.Log.Error("An exception occured while saving the configuration. Configuration has not been saved. {0}", ex);
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
            int currentWidth = Mathf.RoundToInt(tsContainer.width);
            if (currentWidth == 859)
            {
                Toolbar.CreateToolbarControlBox(mode);
                Toolbar.CreateToggleToolbarWidthButton(mode);
            }
            else
            {
                Mod.Log.Warning("Skipped creating button to switch the toolbar width as its width seems to have changed by some other mod already; expected: ~859, actual: {0}", tsContainer.width);
            }
        }

        /// <summary>
        /// Reverts the width of the toolbar to its original value.
        /// </summary>
        private void DisableToggleToolbarWidth()
        {
            Toolbar.RemoveToolbarControlBox();
        }


        /// <summary>
        /// The mouse wheel event on the parent panel of a <see cref="UIScrollablePanel"/>.
        /// </summary>
        /// <param name="component">The component on which the event has been fired.</param>
        /// <param name="eventParam">Additional event parameters.</param>
        private void parentPanel_eventMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
        {
            // We can't be sure if we get false positives
            // In order to be sure, we have to check if this component contains the right child first
            UIScrollablePanel scrollablePanel = component.GetComponentInChildren<UIScrollablePanel>();
            if (scrollablePanel != null)
            {
                Mod.Log.Debug("Caught a mouse wheel event on a parent panel, redirecting to its scrollable panel child");

                // We have a UIScrollablePanel as a direct child, this is good, redirect event call
                ReflectionUtils.InvokePrivateMethod(scrollablePanel, "OnMouseWheel", eventParam);
            }
            else
            {
                Mod.Log.Debug("Caught a mouse wheel event on a parent panel, but no scrollable panel child has been found");
            }
        }
    }
}
