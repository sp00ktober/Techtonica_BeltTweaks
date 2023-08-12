using HarmonyLib;
using I2.Loc;
using Techntonica_BeltTweaks.Singleton;

namespace Techntonica_BeltTweaks.Patches.PrePost
{
    [HarmonyPatch()]
    internal class ConstructableCursorUI_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConstructableCursorUI), "Set")]
        public static void Set_Postfix(ref TMPro.TextMeshProUGUI ___recipeName)
        {
            ___recipeName.SetText($"{___recipeName.text} ({(Config.INSTANCE.Tweak ? "Tweaking enabled" : "Tweaking disabled")})");
        }
    }
}
