using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace ExtendedToolbar.Utils
{
    internal static class AtlasUtils
    {
        private static Dictionary<string, UITextureAtlas> atlases = new Dictionary<string, UITextureAtlas>();

        /// <summary>
        /// Gets the <see cref="UITextureAtlas"/> for the buttons.
        /// </summary>
        /// <returns>The <see cref="UITextureAtlas"/> for the buttons.</returns>
        public static UITextureAtlas GetUIButtonsAtlas()
        {
            if (atlases.ContainsKey("UIButtons"))
            {
                return atlases["UIButtons"];
            }

            UITextureAtlas atlas = CommonShared.Utils.AtlasUtils.CreateAtlas(
                FileUtils.GetTextureFilePath("UIButtons.png"),
                "ExtendedToolbarUIButtonsAtlas",
                "UI/Default UI Shader",
                new Vector2(36, 36),
                new Vector2(3, 2),
                new string[][] {
                    new string[] { "Base", "BaseHovered", "BasePressed" },
                    new string[] { "ArrowLeft", "ArrowRight" }
                });

            atlases.Add("UIButtons", atlas);
            return atlas;
        }
    }
}
