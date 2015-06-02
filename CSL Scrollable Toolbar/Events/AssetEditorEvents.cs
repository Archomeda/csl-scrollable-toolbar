using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using ICities;

namespace ScrollableToolbar.Events
{
    /// <summary>
    /// Contains various events related to the asset editor.
    /// </summary>
    internal class AssetEditorEvents : IEvent
    {
        public static AssetEditorEvents Instance { get; private set; }

        public void Start(LoadMode mode)
        {
            Instance = this;
        }

        public void Stop()
        {
            Instance = null;
        }


        /// <summary>
        /// Gets fired when the mode in the asset editor changes.
        /// This is basically a forward to <see cref="ToolController.eventEditPrefabChanged"/>, but provided here for easy access.
        /// </summary>
        public static event ToolController.EditPrefabChanged AssetEditorModeChanged
        {
            add { ToolsModifierControl.toolController.eventEditPrefabChanged += value; }
            remove { ToolsModifierControl.toolController.eventEditPrefabChanged -= value; }
        }
    }
}
