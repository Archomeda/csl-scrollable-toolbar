using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ScrollableToolbar.UI
{
    internal static class Buttons
    {
        internal static UIMultiStateButton switchModeButton;
        private static float originalTSContainerWidth;


        /// <summary>
        /// Creates our button to switch between the different toolbar modes.
        /// </summary>
        internal static void CreateSwitchModeButton()
        {
            // Create button and attach it
            UISlicedSprite tsBar = GameObject.Find("TSBar").GetComponent<UISlicedSprite>();
            switchModeButton = tsBar.AddUIComponent<UIMultiStateButton>();

            // Set layout
            switchModeButton.isVisible = false;
            switchModeButton.size = new Vector2(18, 18);
            ResetSwitchModeButtonPosition();

            // Set additional settings
            switchModeButton.playAudioEvents = true;
            switchModeButton.tooltip = "Toggles the toolbar between normal and full width";
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

        public static void switchModeButton_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
            UIButton tsCloseButton = GameObject.Find("TSCloseButton").GetComponent<UIButton>();

            switch (value)
            {
                case 0:
                    // Reset to normal width
                    tsContainer.width = originalTSContainerWidth;
                    break;
                case 1:
                    // Patch to full width
                    originalTSContainerWidth = tsContainer.width;
                    int extendAmount = (int)((tsCloseButton.absolutePosition.x - tsContainer.absolutePosition.x - tsContainer.width) / 109f);
                    tsContainer.width += extendAmount * 109f;
                    break;
            }
        }

        /// <summary>
        /// Removes our button to switch between the different toolbar modes.
        /// </summary>
        internal static void RemoveSwitchModeButton()
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
            switchModeButton.isVisible = true;
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
