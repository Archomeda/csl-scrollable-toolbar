using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ScrollableToolbar.Events
{
    /// <summary>
    /// Contains various events related to the toolbar.
    /// </summary>
    internal class ToolbarEvents : IEvent
    {
        public static ToolbarEvents Instance { get; private set; }

        private bool isToolbarOpen;

        public void Start(LoadMode mode)
        {
            isToolbarOpen = false;

            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                default:
                    HookToolbar();
                    break;
                case LoadMode.NewAsset:
                case LoadMode.LoadAsset:
                    // The asset editor has the ability to change the toolbar on the fly,
                    // so we have to rehook on the toolbar after this happens.
                    AssetEditorEvents.AssetEditorModeChanged += AssetEditorEvents_AssetEditorModeChanged;
                    break;
            }

            Instance = this;
        }

        public void Stop()
        {
            AssetEditorEvents.AssetEditorModeChanged -= AssetEditorEvents_AssetEditorModeChanged;
            UnhookToolbar();

            isToolbarOpen = false;
            Instance = null;
        }


        public delegate void ToolbarOpenedEventHandler();
        /// <summary>
        /// Gets fired when the toolbar has been opened.
        /// </summary>
        public static event ToolbarOpenedEventHandler ToolbarOpened;

        public delegate void ToolbarClosedEventHandler();
        /// <summary>
        /// Gets fired when the toolbar has been closed.
        /// </summary>
        public static event ToolbarClosedEventHandler ToolbarClosed;

        private void OnToolbarOpened()
        {
            var handler = ToolbarOpened;
            if (handler != null)
                handler();
        }

        private void OnToolbarClosed()
        {
            var handler = ToolbarClosed;
            if (handler != null)
                handler();
        }

        private void HookToolbar()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            if (tsContainer != null)
            {
                foreach (UIScrollablePanel panel in tsContainer.GetComponentsInChildren<UIScrollablePanel>())
                {
                    panel.eventVisibilityChanged += this.ToolbarPanel_OnVisibilityChanged;
                }
            }
        }

        private void UnhookToolbar()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            if (tsContainer != null)
            {
                foreach (UIScrollablePanel panel in tsContainer.GetComponentsInChildren<UIScrollablePanel>())
                {
                    panel.eventVisibilityChanged -= this.ToolbarPanel_OnVisibilityChanged;
                }
            }
        }

        private void ToolbarPanel_OnVisibilityChanged(UIComponent component, bool value)
        {
            // We have to check the visibility of the parent of the parent of the UIScrollablePanel,
            // since the UIScrollablePanel is bound to a specific tab and we catch this event from every UIScrollablePanel.
            // This might cause a race condition for invisible tabs that send the event later than visible tabs.
            if (component.parent.parent.isVisible)
            {
                // We have to double check if the panel that has been opened, is actually a toolbar panel.
                // We can do that by checking how many child components the visible UIScrollablePanel has,
                // if it has one or more child components, we continue. Otherwise, don't do anything.
                if (component.childCount > 0)
                {
                    if (!isToolbarOpen)
                    {
                        this.OnToolbarOpened();
                        this.isToolbarOpen = true;
                    }
                }
            }
            else if (this.isToolbarOpen)
            {
                this.OnToolbarClosed();
                this.isToolbarOpen = false;
            }
        }

        private void AssetEditorEvents_AssetEditorModeChanged(PrefabInfo info)
        {
            this.OnToolbarClosed();
            this.HookToolbar();
        }
    }
}
