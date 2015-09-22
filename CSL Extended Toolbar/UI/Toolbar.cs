using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using ColossalFramework.UI;
using CommonShared.Proxies.Events;
using CommonShared.Utils;
using ExtendedToolbar.Defs;
using ExtendedToolbar.Utils;
using ICities;
using UnityEngine;

namespace ExtendedToolbar.UI
{
    internal static class Toolbar
    {
        private static UIPanel toolbarControlBox;
        private static UIMultiStateButton toggleToolbarWidthButton;
        private static float originalTSContainerX;
        private static float originalTSContainerWidth;
        private static float lastTSContainerX;
        private static float lastTSContainerWidth;
        private const float measureRange = 5f;


        /// <summary>
        /// Creates our control box for the toolbar on which we can add buttons.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public static void CreateToolbarControlBox(LoadMode mode)
        {
            if (toolbarControlBox != null)
            {
                // We already created it
                return;
            }

            // Create panel and attach it
            UISlicedSprite tsBar = GameObject.Find(GameObjectDefs.ID_TSBAR).GetComponent<UISlicedSprite>();
            toolbarControlBox = tsBar.AddUIComponent<UIPanel>();
            toolbarControlBox.name = "ToolbarControlBox";

            // Set layout
            toolbarControlBox.isVisible = false;
            toolbarControlBox.anchor = UIAnchorStyle.Right | UIAnchorStyle.Top;
            toolbarControlBox.autoLayout = true;
            toolbarControlBox.autoLayoutPadding = new RectOffset(2, 2, 5, 5);
            toolbarControlBox.autoLayoutDirection = LayoutDirection.Horizontal;
            toolbarControlBox.autoLayoutStart = LayoutStart.TopRight;
            toolbarControlBox.autoSize = true;
            toolbarControlBox.size = new Vector2(0, 18);
            ResetToolbarControlBoxPosition();

            // Hook onto various events
            ToolbarEvents.ToolbarOpened += ToolbarEvents_ToolbarOpened;
            ToolbarEvents.ToolbarClosed += ToolbarEvents_ToolbarClosed;
            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            tsContainer.eventSizeChanged += TSContainer_OnSizeChanged;

            Mod.Instance.Log.Debug("Created ToolbarControlBox");
        }

        /// <summary>
        /// Creates our button to switch between the different toolbar modes.
        /// </summary>
        /// <param name="mode">The game mode.</param>
        public static void CreateToggleToolbarWidthButton(LoadMode mode)
        {
            if (toggleToolbarWidthButton != null)
            {
                // We already created it
                return;
            }

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
            toggleToolbarWidthButton = toolbarControlBox.AddUIComponent<UIMultiStateButton>();

            // Set layout
            toggleToolbarWidthButton.isVisible = false;
            toggleToolbarWidthButton.size = new Vector2(18, 18);

            // Set additional settings
            toggleToolbarWidthButton.name = "ToggleToolbarWidthButton";
            toggleToolbarWidthButton.playAudioEvents = true;
            toggleToolbarWidthButton.tooltip = "Toggle between normal and extended width";
            toggleToolbarWidthButton.isTooltipLocalized = false;

            // Add atlas
            toggleToolbarWidthButton.atlas = Utils.AtlasUtils.GetUIButtonsAtlas();

            // Add background sprites
            toggleToolbarWidthButton.backgroundSprites.AddState();
            toggleToolbarWidthButton.backgroundSprites[0].normal = "Base";
            toggleToolbarWidthButton.backgroundSprites[0].hovered = "BaseHovered";
            toggleToolbarWidthButton.backgroundSprites[0].pressed = "BasePressed";
            toggleToolbarWidthButton.backgroundSprites[1].normal = "Base";
            toggleToolbarWidthButton.backgroundSprites[1].hovered = "BaseHovered";
            toggleToolbarWidthButton.backgroundSprites[1].pressed = "BasePressed";

            // Add foreground sprites
            toggleToolbarWidthButton.foregroundSprites.AddState();
            toggleToolbarWidthButton.foregroundSprites[0].normal = "ArrowRight";
            toggleToolbarWidthButton.foregroundSprites[1].normal = "ArrowLeft";

            // Fix some NullReferenceException for m_spritePadding
            toggleToolbarWidthButton.spritePadding = new RectOffset();

            // Hook onto various events
            toggleToolbarWidthButton.eventActiveStateIndexChanged += toggleToolbarWidthButton_eventActiveStateIndexChanged;

            // Depending on the previous state, set correct mode
            toggleToolbarWidthButton.activeStateIndex = Mod.Instance.Settings.State.ToolbarHasExtendedWidth ? 1 : 0;

            Mod.Instance.Log.Debug("Created ToggleToolbarWidthButton");
        }


        /// <summary>
        /// Removes our button to switch between the different toolbar modes.
        /// </summary>
        public static void RemoveToolbarControlBox()
        {
            ToolbarEvents.ToolbarOpened -= ToolbarEvents_ToolbarOpened;
            ToolbarEvents.ToolbarClosed -= ToolbarEvents_ToolbarClosed;

            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            tsContainer.eventSizeChanged -= TSContainer_OnSizeChanged;

            UISlicedSprite tsBar = GameObject.Find(GameObjectDefs.ID_TSBAR).GetComponent<UISlicedSprite>();
            tsBar.RemoveUIComponent(toolbarControlBox);
            toolbarControlBox = null;
            toggleToolbarWidthButton = null;
        }

        private static void toggleToolbarWidthButton_eventActiveStateIndexChanged(UIComponent component, int value)
        {
            Mod.Instance.Log.Debug("ToggleToolbarWidthButton activeStateIndexChanged to {0}", value);

            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            UIButton tsCloseButton = GameObject.Find(GameObjectDefs.ID_TSCLOSEBUTTON).GetComponent<UIButton>();

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

            // Save state
            Mod.Instance.Settings.State.ToolbarHasExtendedWidth = value == 1;
        }

        private static void ResetToolbarControlBoxPosition()
        {
            Mod.Instance.Log.Debug("Resetting position of ToolbarControlBox");
            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            toolbarControlBox.absolutePosition = (Vector2)tsContainer.absolutePosition + new Vector2(tsContainer.size.x - 3, 0);
        }

        private static void ToolbarEvents_ToolbarOpened()
        {
            // For compatibility with Enhanced Build Panel, we have to check if the UIScrollablePanel has not been patched.
            UITabContainer tsContainer = GameObject.Find(GameObjectDefs.ID_TSCONTAINER).GetComponent<UITabContainer>();
            UIScrollablePanel panel = tsContainer.GetComponentsInChildren<UIScrollablePanel>().FirstOrDefault(p => p.isVisible);
            if (panel.name != "ScrollablePanel")
            {
                toggleToolbarWidthButton.isVisible = false;
                Mod.Instance.Log.Debug("Toolbar opened; ToggleToolbarWidthButton not visible due to panel not having its original name; expected: ScrollablePanel, actual: {0}", panel.name);
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
                        toggleToolbarWidthButton.isVisible = false;
                        Mod.Instance.Log.Debug("ToggleToolbarWidthButton not visible after delay due to panel not having its original name; expected: ScrollablePanel, actual: {0}", panel.name);
                    }
                    timer.Dispose();
                };
                timer.Start();
                Mod.Instance.Log.Debug("Toolbar opened; timer started to check if EnhancedBuildPanel changes the name of the scrollable panel after us");
            }


            // We need to check if the size and position of TSContainer is still the same as we left it.
            // If not, then another mod has probably overwritten some stuff and we cannot enable the button.
            // Sorry Sapphire users, but I assume your skin already patches the width of the toolbar ;)
            if (tsContainer.absolutePosition.x >= lastTSContainerX - measureRange && tsContainer.absolutePosition.x <= lastTSContainerX + measureRange &&
               tsContainer.width >= lastTSContainerWidth - measureRange && tsContainer.width <= lastTSContainerWidth + measureRange)
            {
                toolbarControlBox.isVisible = true;
                toggleToolbarWidthButton.isVisible = true;
                Mod.Instance.Log.Debug("Toolbar opened; ToolbarControlBox and ToggleToolbarWidthButton visibility = true");
            }
        }

        private static void ToolbarEvents_ToolbarClosed()
        {
            toolbarControlBox.isVisible = false;
            Mod.Instance.Log.Debug("Toolbar closed, ToolbarControlBox visibility = false");
        }

        private static void TSContainer_OnSizeChanged(UIComponent component, Vector2 value)
        {
            ResetToolbarControlBoxPosition();

            // We have to reset the child components somehow, we hack this by making the current visible panel invisible and visible again.
            // If you know a better way to reset the layout, let me know!
            Mod.Instance.Log.Debug("Size of TSContainer changed, resetting panel layout");
            foreach (var child in component.components.Where(c => c.isVisible))
            {
                child.isVisible = false;
                child.isVisible = true;
            }
        }
    }
}
