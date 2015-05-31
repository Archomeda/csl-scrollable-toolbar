using System;
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
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            this.EnableScrolling();
            this.PatchToolbarWidth();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            this.DisableScrolling();
            this.UnpatchToolbarWidth();
        }

        private bool[] originalStates;
        private float originalTSContainerWidth;

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
        /// Enables scrolling on the toolbar.
        /// </summary>
        private void EnableScrolling()
        {
            UIScrollablePanel[] panels = FindPatchableScrollablePanels();
            if (panels.Length == 0)
            {
                Debug.Warning("No panels found to patch, aborting; mod probably needs to be updated, please inform the author of the mod");
                return;
            }

            this.PatchToolbarWidth();

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

            // Although it is supported, somehow disabled UI objects do not send the OnMouseWheel event.
            // We are patching that here by redirecting the calls from the original method to our method.
            CustomMouseHandler.Detour();

            Debug.Log("{0} panels have been patched to be scrollable with the mouse wheel", panels.Length);
        }

        /// <summary>
        /// Disables scrolling on the toolbar.
        /// </summary>
        private void DisableScrolling()
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

            CustomMouseHandler.UnDetour();

            Debug.Log("{0} patched panels have been reverted to their original state", panels.Length);
        }

        /// <summary>
        /// Patches the width of the toolbar to contain more items at once.
        /// </summary>
        private void PatchToolbarWidth()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            UIButton tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIButton>();
            this.originalTSContainerWidth = tsContainer.width;
            int currentWidth = Mathf.RoundToInt(tsContainer.width);
            if (currentWidth == 859)
            {
                // We only change the size if it hasn't been changed already by some other mod
                int extendAmount = (int)((tsCloseButton.absolutePosition.x - tsContainer.absolutePosition.x - tsContainer.width) / 109f);
                tsContainer.width += extendAmount * 109f;
                Debug.Log("Toolstrip width has been patched to show {0} more items at once", extendAmount);
            }
            else
            {
                Debug.Log("Toolstrip width has not been patched since it didn't have its original width; expected = {0}, actual = {1}", 859, currentWidth);
            }
        }

        /// <summary>
        /// Reverts the width of the toolbar to its original value.
        /// </summary>
        private void UnpatchToolbarWidth()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            if (tsContainer.width != this.originalTSContainerWidth)
            {
                tsContainer.width = this.originalTSContainerWidth;
                Debug.Log("Toolstrip width has been reverted to its original value");
            }
        }


        private void parentPanel_eventMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
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
