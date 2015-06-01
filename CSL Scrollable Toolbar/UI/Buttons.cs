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
        private static UIMultiStateButton switchModeButton;
        private static float originalTSContainerX;
        private static float originalTSContainerWidth;
        private static float lastTSContainerX = 590f;
        private static float lastTSContainerWidth = 859f;
        private const float measureRange = 5f;


        /// <summary>
        /// Creates our button to switch between the different toolbar modes.
        /// </summary>
        public static void CreateSwitchModeButton()
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
            switchModeButton.tooltip = "Toggle the toolbar between normal and full width";
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

            switch (value)
            {
                case 0:
                    // Reset to normal width
                    tsContainer.absolutePosition = new Vector2(originalTSContainerX, tsContainer.absolutePosition.y);
                    tsContainer.width = originalTSContainerWidth;
                    break;

                case 1:
                    // Patch to full width

                    // Extend to the right
                    originalTSContainerWidth = tsContainer.width;
                    int extendAmount = (int)((tsCloseButton.absolutePosition.x - tsContainer.absolutePosition.x - tsContainer.width) / 109f);
                    tsContainer.width += extendAmount * 109f;

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
                    tsContainer.absolutePosition = new Vector2(tsContainer.absolutePosition.x - extendAmount * 109f, tsContainer.absolutePosition.y);
                    tsContainer.width += extendAmount * 109f;

                    break;
            }

            lastTSContainerX = tsContainer.absolutePosition.x;
            lastTSContainerWidth = tsContainer.width;
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
            // We need to check if the size and position of TSContainer is still the same as we left it.
            // If not, then another mod has probably overwritten some stuff and we cannot enable the button.
            // Sorry Sapphire users, but I assume your skin already patches the width of the toolbar ;)
            UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();
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
