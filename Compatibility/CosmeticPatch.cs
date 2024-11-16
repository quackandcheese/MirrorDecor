using HarmonyLib;
using MoreCompany.Cosmetics;
using MoreCompany;

namespace MirrorDecor.Compatibility
{
    class CosmeticPatch
    {
        [HarmonyPatch(typeof(CosmeticApplication), "UpdateAllCosmeticVisibilities")]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void Postfix(CosmeticApplication __instance, bool isLocalPlayer)
        {
            if (__instance.parentType != ParentType.Player)
                return;
            if (!isLocalPlayer)
                return;
            if (!MainClass.cosmeticsSyncOther.Value)
                return;

            foreach (var spawnedCosmetic in __instance.spawnedCosmetics)
            {
                if (spawnedCosmetic.cosmeticType == CosmeticType.HAT && __instance.detachedHead)
                    continue;

                spawnedCosmetic.gameObject.SetActive(true);
                CosmeticRegistry.RecursiveLayerChange(spawnedCosmetic.transform, 23);
            }
        }
    }

}
