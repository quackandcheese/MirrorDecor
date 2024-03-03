using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using LethalSettings.UI.Components;
using LethalSettings.UI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using System.Runtime.CompilerServices;
using static UnityEngine.Rendering.DebugUI;

namespace MirrorDecor
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("evaisa.lethallib")]
    [BepInDependency("com.willis.lc.lethalsettings", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "quackandcheese.mirrordecor";
        public const string ModName = "MirrorDecor";
        public const string ModVersion = "1.3.2";

        public const int MirrorCameraCullingMask = 565909343;

        public static AssetBundle Bundle;

        public static ConfigFile config;

        public static BepInEx.Logging.ManualLogSource logger;

        public static List<CustomUnlockable> customUnlockables;

        public const int MirrorMinRes = 8;
        public const int MirrorMaxRes = 1024;


        public static event EventHandler<OnUpdateResolutionEventArgs> OnUpdateResolution;
        public class OnUpdateResolutionEventArgs : EventArgs
        {
            public OnUpdateResolutionEventArgs(int res)
            {
                this.res = res;
            }
            public int res;
        }

        private void Awake()
        {
/*            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }*/

            Bundle = QuickLoadAssetBundle("mirror.assets");
            logger = Logger;
            config = Config;

            Harmony harmony = new Harmony(ModGUID);
            harmony.PatchAll();

            MirrorDecor.Config.Load();
            RegisterItems();
            if (Chainloader.PluginInfos.ContainsKey("com.willis.lc.lethalsettings"))
            {
                AddSetting();
            }
            UpdateMirrorRes((int)MirrorDecor.Config.resolution.Value);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddSetting()
        {
            ModMenu.RegisterMod(new ModMenu.ModSettingsConfig
            {
                Name = ModName,
                Id = ModGUID,
                Version = ModVersion,
                Description = "Adds a working mirror decoration that you can buy for your ship!",
                MenuComponents = new MenuComponent[]
                {
                    new HorizontalComponent
                    {
                        Children = new MenuComponent[]
                        {
                            new SliderComponent
                            {
                                Value = MirrorDecor.Config.resolution.Value,
                                MinValue = MirrorMinRes,
                                MaxValue = MirrorMaxRes,
                                Text = "Mirror Resolution",
                                OnValueChanged = (self, value) => UpdateMirrorRes((int)value),
                                // MirrorRes = (int)value,
                                //Logger.LogInfo($"{value}x{value} px")
                                WholeNumbers = true,
                                OnInitialize = (self) => UpdateMirrorRes((int)MirrorDecor.Config.resolution.Value)
                            }
                        }
                    },
                    new LabelComponent
                    {
                        Text = "The resolution/quality of the mirror image. Reduce size for better framerate."
                    }
                }
            }, true, true);
        }

        private void UpdateMirrorRes(int res)
        {
            int resolution = Mathf.Clamp(res, MirrorMinRes, MirrorMaxRes);

            RenderTexture mirrorRenderTexture = Bundle.LoadAsset<RenderTexture>("Assets/Mirror/Materials/Mirror.renderTexture");
            Resize(mirrorRenderTexture, resolution, resolution);
            MirrorDecor.Config.resolution.Value = resolution;
        }

        private void RegisterItems()
        {
            customUnlockables = new List<CustomUnlockable>
            {
                CustomUnlockable.Add("Mirror", "Assets/Mirror/Unlockables/Mirror/Mirror.asset", "Assets/Mirror/Unlockables/Mirror/MirrorInfo.asset", null, MirrorDecor.Config.mirrorPrice.Value, MirrorDecor.Config.mirrorEnabled.Value)
            };

            UnlockableItem mirror = Bundle.LoadAsset<UnlockableItemDef>("Assets/Mirror/Unlockables/Mirror/Mirror.asset").unlockable;
            mirror.alwaysInStock = MirrorDecor.Config.alwaysAvailable.Value;
            /*
                        RenderTexture mirrorRenderTexture = Bundle.LoadAsset<RenderTexture>("Assets/Mirror/Materials/Mirror.renderTexture");
                        int resolution = DefaultMirrorRes; //MirrorDecor.Config.resolution.Value;
                        Resize(mirrorRenderTexture, resolution, resolution);
            */
            foreach (CustomUnlockable customUnlockable in customUnlockables)
            {
                if (customUnlockable.enabled)
                {
                    UnlockableItem unlockable = Bundle.LoadAsset<UnlockableItemDef>(customUnlockable.unlockablePath).unlockable;
                    if (unlockable.prefabObject != null)
                    {
                        NetworkPrefabs.RegisterNetworkPrefab(unlockable.prefabObject);
                    }
                    TerminalNode terminalNode = null;
                    if (customUnlockable.infoPath != null)
                    {
                        terminalNode = Bundle.LoadAsset<TerminalNode>(customUnlockable.infoPath);
                    }
                    Unlockables.RegisterUnlockable(unlockable, StoreType.Decor, null, null, terminalNode, customUnlockable.unlockCost);
                }
            }
        }

        #region HELPERS
        public static T FindAsset<T>(string name) where T : UnityEngine.Object
        {
            return Resources.FindObjectsOfTypeAll<T>().ToList().Find(x => x.name == name);
        }

        public static AssetBundle QuickLoadAssetBundle(string assetBundleName)
        {
            string AssetBundlePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), assetBundleName);

            return AssetBundle.LoadFromFile(AssetBundlePath);
        }

        public static void Resize(RenderTexture renderTexture, int width, int height)
        {
            if (renderTexture)
            {
                renderTexture.Release();
                renderTexture.width = width;
                renderTexture.height = height;
            }
        }
        #endregion

        #region MOD SYNC
        public void sendModInfo()
        {
            foreach (var plugin in Chainloader.PluginInfos)
            {
                if (plugin.Value.Metadata.GUID.Contains("ModSync"))
                {
                    try
                    {
                        List<string> list = new List<string>
                        {
                            "quackandcheese",
                            "MirrorDecor"
                        };
                        plugin.Value.Instance.BroadcastMessage("getModInfo", list, UnityEngine.SendMessageOptions.DontRequireReceiver);
                    }
                    catch (Exception e)
                    {
                        // ignore mod if error, removing dependency
                        logger.LogInfo($"Failed to send info to ModSync, go yell at Minx");
                    }
                    break;
                }

            }
        }
        #endregion
    }
}