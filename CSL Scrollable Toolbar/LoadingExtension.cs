﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework.UI;
using ICities;
using ScrollableToolbar.Detour;
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
                    ToolsModifierControl.toolController.eventEditPrefabChanged += this.OnAssetEditorModeChange;
                    this.PatchAdditionalComponents();
                    break;
            }
        }

        /// <summary>
        /// Our exit point. Here we unload everything we have loaded.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            ToolsModifierControl.toolController.eventEditPrefabChanged -= this.OnAssetEditorModeChange;

            this.UnpatchPanels();
            this.UnpatchAdditionalComponents();
        }

        private bool[] originalStates;

        /// <summary>
        /// The asset editor has the ability to change the toolbar on the fly.
        /// We can monitor these changes and patch the panels when they have been refreshed.
        /// </summary>
        /// <param name="prefabInfo"></param>
        private void OnAssetEditorModeChange(PrefabInfo prefabInfo)
        {
            this.PatchPanels();
        }

        /// <summary>
        /// Finds all <see cref="UIScrollablePanel"/>s in TSContainer that we can patch.
        /// </summary>
        /// <returns>A list of all patchable <see cref="UIScrollablePanel"/>s</returns>
        private UIScrollablePanel[] FindPatchableScrollablePanels()
        {
            GameObject obj = GameObject.Find("TSContainer");
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
                Debug.Warning("No panels found to patch, aborting; in case there are panels that should be patched, please inform the author of the mod");
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

            Debug.Log("{0} panels have been patched to be scrollable with the mouse wheel", panels.Length);
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

            Debug.Log("{0} patched panels have been reverted to their original state", panels.Length);
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
        /// The mouse wheel event on the parent panel of a <see cref="UIScrollablePanel"/>.
        /// </summary>
        /// <param name="component">The component on which the event has been fired.</param>
        /// <param name="eventParam">Additional event parameters.</param>
        void parentPanel_eventMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
        {
            // We can't be sure if we get false positives
            // In order to be sure, we have to check if this component contains the right child first
            UIScrollablePanel scrollablePanel = component.GetComponentInChildren<UIScrollablePanel>();
            if (scrollablePanel != null)
            {
                // We have a UIScrollablePanel as a direct child, this is good, redirect event call
                Utils.InvokePrivateMethod(scrollablePanel, "OnMouseWheel", eventParam);
            }
        }
    }
}
