using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ScrollableToolbar.UI
{
    /// <summary>
    /// This static class contains various UI event helpers.
    /// </summary>
    internal static class EventHelpers
    {
        /// <summary>
        /// Start monitoring for various UI events in the game.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public static void StartEvents(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                default:
                    HookToolbar();
                    break;
                case LoadMode.NewAsset:
                case LoadMode.LoadAsset:
                    ToolsModifierControl.toolController.eventEditPrefabChanged += OnAssetEditorModeChange;
                    break;
            }

            Debug.Log("Started events");
        }

        /// <summary>
        /// Stop monitoring for the UI events in the game.
        /// </summary>
        public static void StopEvents()
        {
            ToolsModifierControl.toolController.eventEditPrefabChanged -= OnAssetEditorModeChange;
            UnhookToolbar();

            Debug.Log("Stopped events");
        }

        /// <summary>
        /// The asset editor has the ability to change the toolbar on the fly.
        /// We can monitor for these changes.
        /// </summary>
        /// <param name="prefabInfo"></param>
        private static void OnAssetEditorModeChange(PrefabInfo info)
        {
            HookToolbar();
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

        private static bool isToolbarOpen = false;


        private static void OnToolbarOpened()
        {
            Debug.Log("Toolbar opened");
            var handler = ToolbarOpened;
            if (handler != null)
                handler();
        }

        private static void OnToolbarClosed()
        {
            Debug.Log("Toolbar closed");
            var handler = ToolbarClosed;
            if (handler != null)
                handler();
        }

        private static void HookToolbar()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            if (tsContainer != null)
            {
                foreach (UIScrollablePanel panel in tsContainer.GetComponentsInChildren<UIScrollablePanel>())
                {
                    panel.eventVisibilityChanged += TSContainerPanel_OnVisibilityChanged;
                }
            }
        }

        private static void UnhookToolbar()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            if (tsContainer != null)
            {
                foreach (UIScrollablePanel panel in tsContainer.GetComponentsInChildren<UIScrollablePanel>())
                {
                    panel.eventVisibilityChanged -= TSContainerPanel_OnVisibilityChanged;
                }
            }
        }

        private static void TSContainerPanel_OnVisibilityChanged(UIComponent component, bool value)
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
                        OnToolbarOpened();
                        isToolbarOpen = true;
                    }
                }
            }
            else if (isToolbarOpen)
            {
                OnToolbarClosed();
                isToolbarOpen = false;
            }
        }

    }
}
