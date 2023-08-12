using HarmonyLib;
using Techntonica_BeltTweaks.Singleton;

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
