using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace ScrollableToolbar.UI
{
    internal static class Buttons
    {
        private static UIMultiStateButton switchModeButton;
        private static float originalTSContainerX;
        private static float originalTSContainerWidth;
        private static float lastTSContainerX;
        private static float lastTSContainerWidth;
        private const float measureRange = 5f;


        /// <summary>
        /// Creates our button to switch between the different toolbar modes.
        /// </summary>
        public static void CreateSwitchModeButton(LoadMode mode)
        {
            // Initialize variables
            lastTSContainerWidth = 859f;
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                case LoadMode.NewAsset:
                case LoadMode.LoadAsset:
                default:
                    lastTSContainerX = 590f;
                    break;
                case LoadMode.NewMap:
                case LoadMode.LoadMap:
                    lastTSContainerX = 676f;
                    break;
            }

            // Create button and attach it
            UISlicedSprite tsBar = GameObject.Find("TSBar").GetComponent<UISlicedSprite>();
            switchModeButton = tsBar.AddUIComponent<UIMultiStateButton>();

            // Set layout
            switchModeButton.isVisible = false;
            switchModeButton.size = new Vector2(18, 18);
            ResetSwitchModeButtonPosition();

            // Set additional settings
            switchModeButton.name = "SwitchToolbarModeButton";
            switchModeButton.playAudioEvents = true;
            switchModeButton.tooltip = "Toggle between normal and extended width";
            switchModeButton.isTooltipLocalized = false;

            // Add background sprites
            switchModeButton.backgroundSprites.AddState();
            switchModeButton.backgroundSprites[0].disabled = "OptionBase";
            switchModeButton.backgroundSprites[0].hovered = "OptionBaseHovered";
            switchModeButton.backgroundSprites[0].normal = "OptionBase";
            switchModeButton.backgroundSprites[0].pressed = "OptionBasePressed";
            switchModeButton.backgroundSprites[1].disabled = "OptionBase";
            switchModeButton.backgroundSprites[1].normal = "OptionBaseFocused";
            switchModeButton.backgroundSprites[1].pressed = "OptionBasePressed";

            // Add foreground sprites
            switchModeButton.foregroundSprites.AddState();

            // Hook onto various events
            EventHelpers.ToolbarOpened += EventHelpers_ToolbarOpened;
            EventHelpers.ToolbarClosed += EventHelpers_ToolbarClosed;

            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            tsContainer.eventSizeChanged += TSContainer_OnSizeChanged;

            // Listen to various events
            switchModeButton.eventActiveStateIndexChanged += switchModeButton_eventActiveStateIndexChanged;
        }

        private static void switchModeButton_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            UIButton tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIButton>();

            float newX = tsContainer.absolutePosition.x;
            float newWidth = tsContainer.width;

            switch (value)
            {
                case 0:
                    // Reset to normal width
                    newX = originalTSContainerX;
                    newWidth = originalTSContainerWidth;
                    break;

                case 1:
                    // Patch to full width

                    // Extend to the right
                    originalTSContainerWidth = tsContainer.width;
                    int extendAmount = (int)((tsCloseButton.absolutePosition.x - tsContainer.absolutePosition.x - tsContainer.width) / 109f);
                    newWidth += extendAmount * 109f;

                    // Extend to the left
                    originalTSContainerX = tsContainer.absolutePosition.x;
                    GameObject advisorButtonGameObject = GameObject.Find("AdvisorButton");
                    UIPanel optionsBar = GameObject.Find("OptionsBar").GetComponent<UIPanel>();

                    float maxX = optionsBar.absolutePosition.x + optionsBar.width;
                    if (advisorButtonGameObject != null)
                    {
                        UIMultiStateButton advisorButton = advisorButtonGameObject.GetComponent<UIMultiStateButton>();
                        maxX = Mathf.Max(maxX, advisorButton.absolutePosition.x + advisorButton.width);
                    }
                    extendAmount = (int)((tsContainer.absolutePosition.x - maxX) / 109f);
                    newX = tsContainer.absolutePosition.x - extendAmount * 109f;
                    newWidth += extendAmount * 109f;

                    break;
            }

            lastTSContainerX = newX;
            lastTSContainerWidth = newWidth;
            tsContainer.absolutePosition = new Vector2(newX, tsContainer.absolutePosition.y);
            tsContainer.width = newWidth;
        }

        /// <summary>
        /// Removes our button to switch between the different toolbar modes.
        /// </summary>
        public static void RemoveSwitchModeButton()
        {
            EventHelpers.ToolbarOpened -= EventHelpers_ToolbarOpened;
            EventHelpers.ToolbarClosed -= EventHelpers_ToolbarClosed;

            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            tsContainer.eventSizeChanged -= TSContainer_OnSizeChanged;

            UISlicedSprite tsBar = GameObject.Find("TSBar").GetComponent<UISlicedSprite>();
            tsBar.RemoveUIComponent(switchModeButton);
            switchModeButton = null;
        }

        private static void ResetSwitchModeButtonPosition()
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            switchModeButton.absolutePosition = (Vector2)tsContainer.absolutePosition + new Vector2(tsContainer.size.x - switchModeButton.size.x - 8, 8);
        }

        private static void EventHelpers_ToolbarOpened()
        {
            // For compatibility with Enhanced Build Panel, we have to check if the UIScrollablePanel has not been patched.
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            UIScrollablePanel panel = tsContainer.GetComponentsInChildren<UIScrollablePanel>().FirstOrDefault(p => p.isVisible);
            if (panel.name != "ScrollablePanel")
            {
                return;
            }
            else
            {
                // We will keep a small compatibility problem however. This method gets called before Enhanced Build Panel changes
                // the name of the panel for the first opening of each panel. So we check it once after a small delay to see if the name
                // has been changed.
                Timer timer = new Timer(100);
                timer.Elapsed += (sender, e) =>
                {
                    timer.Stop();
                    if (panel.name != "ScrollablePanel")
                    {
                        switchModeButton.isVisible = false;
                    }
                    timer.Dispose();
                };
                timer.Start();
            }


            // We need to check if the size and position of TSContainer is still the same as we left it.
            // If not, then another mod has probably overwritten some stuff and we cannot enable the button.
            // Sorry Sapphire users, but I assume your skin already patches the width of the toolbar ;)
            if (tsContainer.absolutePosition.x >= lastTSContainerX - measureRange && tsContainer.absolutePosition.x <= lastTSContainerX + measureRange &&
               tsContainer.width >= lastTSContainerWidth - measureRange && tsContainer.width <= lastTSContainerWidth + measureRange)
            {
                switchModeButton.isVisible = true;
            }
        }

        private static void EventHelpers_ToolbarClosed()
        {
            switchModeButton.isVisible = false;
        }

        private static void TSContainer_OnSizeChanged(UIComponent component, Vector2 value)
        {
            ResetSwitchModeButtonPosition();

            // We have to reset the child components somehow, we hack this by making the current visible panel invisible and visible again.
            // If you know a better way to reset the layout, let me know!
            foreach (var child in component.components.Where(c => c.isVisible))
            {
                child.isVisible = false;
                child.isVisible = true;
            }
        }
    }
}
