using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MirrorDecor
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpawnPlayerAnimation))]
    internal class PlayerPatch
    {
        static void Postfix(ref PlayerControllerB __instance)
        {
            if (__instance == GameNetworkManager.Instance.localPlayerController)
            {
                PlayerControllerB player = __instance;
                __instance.GetComponentInChildren<LODGroup>().enabled = false;

                player.thisPlayerModel.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                player.localVisor.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                player.thisPlayerModelLOD1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                player.thisPlayerModelLOD2.enabled = false;
                //V1 DOESNT WORK ON UPDATE v4.5
                //player.thisPlayerModel.gameObject.layer = 29;
                //player.thisPlayerModelArms.gameObject.layer = 3;

                //V2
                //player.thisPlayerModel.gameObject.layer = 29;
                //player.thisPlayerModelArms.gameObject.layer = 30;
                //player.gameplayCamera.cullingMask = 1094391807;

                //V3
                //player.thisPlayerModel.gameObject.layer = 3;
                //player.thisPlayerModelArms.gameObject.layer = 29;
                //player.gameplayCamera.cullingMask = 557520887;

                //V3
                player.thisPlayerModel.gameObject.layer = 23;
                //player.thisPlayerModelLOD1.gameObject.layer = 5;
                player.thisPlayerModelArms.gameObject.layer = 5;
            }
        }
    }
}
