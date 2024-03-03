using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MirrorDecor
{
    public class Config
    {
        public static void Load()
        {
            Config.mirrorEnabled = Plugin.config.Bind<bool>("Mirror", "MirrorEnabled", true, "Will you be able to purchase the mirror?");
            Config.mirrorPrice = Plugin.config.Bind<int>("Mirror", "MirrorPrice", 100, "What will be the price of the mirror?");
            Config.alwaysAvailable = Plugin.config.Bind<bool>("Mirror", "AlwaysAvailable", true, "Is the mirror always available to purchase from the store?");
            Config.resolution = Plugin.config.Bind<int>("Mirror", "MirrorResolution", 512, "What is the resolution/quality of the mirror image? (ex. 2000 = 2000x2000 pixels)");
        }

        public static ConfigEntry<bool> mirrorEnabled;
        public static ConfigEntry<int> mirrorPrice;
        public static ConfigEntry<bool> alwaysAvailable;
        public static ConfigEntry<int> resolution;
    }
}
