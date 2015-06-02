using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScrollableToolbar.Utils
{
    internal static class ToolbarUtils
    {
        public static GameObject GetTSBar()
        {
            return GameObject.Find("TSBar");
        }

        public static GameObject GetTSCloseButton()
        {
            return GameObject.Find("TSCloseButton");
        }

        public static GameObject GetTSContainer()
        {
            return GameObject.Find("TSContainer");
        }
    }
}
