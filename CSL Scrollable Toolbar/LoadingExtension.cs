using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework.UI;
using ICities;
using ScrollableToolbar.Detour;
using ScrollableToolbar.Events;
using ScrollableToolbar.UI;
using ScrollableToolbar.Utils;
using UnityEngine;

namespace ScrollableToolbar
{
    public class LoadingExtension : LoadingExtensionBase
    {
        /// <summary>
        /// Our entry point. Here we load the mod.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            Configuration.Load();
            if (Configuration.Instance.ExtraDebugLogging)
            {
                Logger.Warning("Extra debug logging is enabled, please use this only to get more information while hunting for bugs; don't use this when playing normally!");
            }

            bool isActive = false;

            if (Configuration.Instance.Features.ToolbarScrolling)
            {
                switch (mode)
                {
                    case LoadMode.NewGame:
                    case LoadMode.LoadGame:
                    default:
                        this.PatchPanels();
                        this.PatchAdditionalComponents();
                        break;
                    case LoadMode.NewAsset:
                    case LoadMode.LoadAsset:
                        // The asset editor has the ability to change the toolbar on the fly,
                        // so we have to repatch our toolbar panels if this happens.
                        AssetEditorEvents.AssetEditorModeChanged += AssetEditorEvents_AssetEditorModeChanged;
                        this.PatchAdditionalComponents();
                        break;
                }
                isActive = true;
            }
            else
            {
                Logger.Debug("Skipping feature ToolbarScrolling as it's disabled");
            }

            if (Configuration.Instance.Features.ToolbarToggleExtendedWidth)
            {
                this.EnableToggleToolbarWidth(mode);
                isActive = true;
            }
            else
            {
                Logger.Debug("Skipping feature ToolbarToggleExtendedWidth as it's disabled");
            }

            if (isActive)
            {
                EventsController.StartEvents(mode);
            }
            else
            {
                Logger.Debug("No active features found, skip starting events");
            }
        }

        /// <summary>
        /// Our exit point. Here we unload everything we have loaded.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (Configuration.Instance.Features.ToolbarScrolling)
            {
                AssetEditorEvents.AssetEditorModeChanged -= this.AssetEditorEvents_AssetEditorModeChanged;
                this.UnpatchPanels();
                this.UnpatchAdditionalComponents();
            }

            this.DisableToggleToolbarWidth();

            EventsController.StopEvents();

            Configuration.Save();
        }

        private bool[] originalStates;

        private void AssetEditorEvents_AssetEditorModeChanged(PrefabInfo prefabInfo)
        {
            this.PatchPanels();
        }

        /// <summary>
        /// Finds all <see cref="UIScrollablePanel"/>s in TSContainer that we can patch.
        /// </summary>
        /// <returns>A list of all patchable <see cref="UIScrollablePanel"/>s</returns>
        private UIScrollablePanel[] FindPatchableScrollablePanels()
        {
            GameObject obj = ToolbarUtils.GetTSContainer();
            if (obj != null)
            {
                return obj.GetComponentsInChildren<UIScrollablePanel>();
            }
            return new UIScrollablePanel[0];
        }

        /// <summary>
        /// Patches the <see cref="UIScrollablePanel"/>s to make them accept the mouse wheel as input.
        /// </summary>
        private void PatchPanels()
        {
            UIScrollablePanel[] panels = FindPatchableScrollablePanels();
            if (panels.Length == 0)
            {
                Logger.Warning("No panels found to patch, aborting; in case there are panels that should be patched, please inform the author of the mod");
                return;
            }

            this.originalStates = new bool[panels.Length];
            for (int i = 0; i < panels.Length; i++)
            {
                // Apparently, scrolling with the mouse wheel is supported by the internal code.
                // But for some reason it's not activated.
                // We are patching it here by setting the correct field to true.
                this.originalStates[i] = panels[i].builtinKeyNavigation;
                panels[i].builtinKeyNavigation = true;

                // In order to have scrolling on the WHOLE panel (e.g. the little space to the left and right of the asset buttons),
                // we also have to redirect certain calls on the parent panel.
                UIPanel parentPanel = panels[i].parent as UIPanel;
                if (parentPanel != null)
                {
                    parentPanel.eventMouseWheel += parentPanel_eventMouseWheel;
                }
            }

            Logger.Info("{0} panels have been patched to be scrollable with the mouse wheel", panels.Length);
        }

        /// <summary>
        /// Undo the patch that make the <see cref="UIScrollablePanel"/> accept the mouse wheel as input.
        /// </summary>
        private void UnpatchPanels()
        {
            UIScrollablePanel[] panels = FindPatchableScrollablePanels();

            if (panels.Length == 0)
            {
                return;
            }

            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].builtinKeyNavigation = this.originalStates[i];
                UIPanel parentPanel = panels[i].parent as UIPanel;
                if (parentPanel != null)
                {
                    parentPanel.eventMouseWheel -= parentPanel_eventMouseWheel;
                }
            }

            Logger.Info("{0} patched panels have been reverted to their original state", panels.Length);
        }


        /// <summary>
        /// Patch some additional components to make them accept the mouse wheel as input.
        /// </summary>
        private void PatchAdditionalComponents()
        {
            // Although it is supported, somehow disabled UI objects do not send the OnMouseWheel event.
            // We are patching that here by redirecting the calls from the original method to our method.
            CustomMouseHandler.Detour();
        }

        /// <summary>
        /// Undo the patch that makes additional componenents accept the mouse wheel as input.
        /// </summary>
        private void UnpatchAdditionalComponents()
        {
            CustomMouseHandler.UnDetour();
        }

        /// <summary>
        /// Patches the width of the toolbar to contain more items at once.
        /// </summary>
        private void EnableToggleToolbarWidth(LoadMode mode)
        {
            // We only add our switch mode button if the toolbar width hasn't been changed by some other mod, in order to prevent incompatibility
            UITabContainer tsContainer = ToolbarUtils.GetTSContainer().GetComponent<UITabContainer>();
            int currentWidth = Mathf.RoundToInt(tsContainer.width);
            if (currentWidth == 859)
            {
                Buttons.CreateSwitchModeButton(mode);
                Logger.Info("Created button to switch the toolbar width");
            }
            else
            {
                Logger.Warning("Skipped creating button to switch the toolbar width as its width seems to have changed by some other mod already; expected: ~859, actual: {0}", tsContainer.width);
            }
        }

        /// <summary>
        /// Reverts the width of the toolbar to its original value.
        /// </summary>
        private void DisableToggleToolbarWidth()
        {
            Buttons.RemoveSwitchModeButton();
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
                Logger.Debug("Caught a mouse wheel event on a parent panel, redirecting to its scrollable panel child");

                // We have a UIScrollablePanel as a direct child, this is good, redirect event call
                ReflectionUtils.InvokePrivateMethod(scrollablePanel, "OnMouseWheel", eventParam);
            }
            else
            {
                Logger.Debug("Caught a mouse wheel event on a parent panel, but no scrollable panel child has been found");
            }
        }
    }
}
