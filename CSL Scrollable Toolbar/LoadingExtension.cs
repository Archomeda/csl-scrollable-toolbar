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
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            this.DisableScrolling();
        }

        private readonly MethodInfo processInputOriginal = typeof(UIInput.MouseHandler).GetMethod("ProcessInput");
        private readonly MethodInfo processInputReplacement = typeof(CustomMouseHandler).GetMethod("ProcessInput");

        private bool[] originalStates;
        private RedirectCallsState processInputMethodState;

        private UIScrollablePanel[] FindPatchableScrollbarPanels()
        {
            GameObject obj = GameObject.Find("TSContainer");
            if (obj != null)
            {
                return obj.GetComponentsInChildren<UIScrollablePanel>();
            }
            return new UIScrollablePanel[0];
        }

        private void EnableScrolling()
        {
            UIScrollablePanel[] panels = FindPatchableScrollbarPanels();
            if (panels.Length == 0)
            {
                Debug.Warning("No panels found to patch. Mod possibly needs to be updated, please inform the author of the mod");
            }

            this.originalStates = new bool[panels.Length];
            for (int i = 0; i < panels.Length; i++)
            {
                // Apparently, scrolling with the mouse wheel is supported by the internal code.
                // But for some reason it's not activated.
                // We are patching it here by setting the correct field to true.
                this.originalStates[i] = panels[i].builtinKeyNavigation;
                panels[i].builtinKeyNavigation = true;
            }

            // Although it is supported, somehow disabled UI objects do not send the OnMouseWheel event.
            // We are patching that here by redirecting the calls from the original method to our method.
            try
            {
                this.processInputMethodState = RedirectionHelper.RedirectCalls(processInputOriginal, processInputReplacement);
                Debug.Log("UIInput.MouseHandler.ProcessInput() has been detoured");
            }
            catch (Exception ex)
            {
                Debug.Error("Exception while detouring UIInput.MouseHandler.ProcessInput(): {0}", ex);
            }

            Debug.Log("{0} panels have been patched to be scrollable with the mouse wheel", panels.Length);
        }

        private void DisableScrolling()
        {
            UIScrollablePanel[] panels = FindPatchableScrollbarPanels();

            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].builtinKeyNavigation = this.originalStates[i];
            }

            try
            {
                RedirectionHelper.RevertRedirect(processInputOriginal, this.processInputMethodState);
                Debug.Log("UIInput.MouseHandler.ProcessInput() detour has been reverted");
            }
            catch (Exception ex)
            {
                Debug.Error("Exception while reverting detour UIInput.MouseHandler.ProcessInput(): {0}", ex);
            }

            Debug.Log("{0} patched panels have been reverted to their original state", panels.Length);
        }
    }
}
