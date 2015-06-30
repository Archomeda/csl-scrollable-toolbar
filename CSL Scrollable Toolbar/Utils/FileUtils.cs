using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScrollableToolbar.Utils
{
    internal static class FileUtils
    {
        public static string GetTextureFilePath(string filename)
        {
            return Path.Combine(Path.Combine(CommonShared.Utils.FileUtils.GetAssemblyFolder(Mod.Instance), "Textures"), filename);
        }
    }
}
