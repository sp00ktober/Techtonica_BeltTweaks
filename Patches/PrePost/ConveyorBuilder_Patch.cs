using HarmonyLib;
using Techntonica_BeltTweaks.Singleton;
using UnityEngine;

namespace Techntonica_BeltTweaks.Patches.PrePost
{
    [HarmonyPatch()]
    internal class ConveyorBuilder_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConveyorBuilder), "OnReset")]
        [HarmonyPatch(typeof(ConveyorBuilder), "OnCancel")]
        public static void ClearCustomBeltStuff()
        {
            Config.INSTANCE.ResetBeltValues();
        }
    }
}
