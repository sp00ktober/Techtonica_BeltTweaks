using HarmonyLib;
using I2.Loc;
using Techntonica_BeltTweaks.Singleton;
using UnityEngine;

namespace Techntonica_BeltTweaks.Patches.PrePost
{
    [HarmonyPatch()]
    internal class ConstructableCursorUI_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConstructableCursorUI), "Set")]
        public static void Set_Postfix(ref TMPro.TextMeshProUGUI ___recipeName, ResourceInfo info)
        {
            if (info == null || !info.buildable || Player.instance.toolbar.hidingShortcut || Player.instance.builder.constructionMode == PlayerBuilder.ConstructionMode.Deconstruction || !(Player.instance.builder.currentBuilder is ConveyorBuilder))
            {
                return;
            }
            ___recipeName.SetText($"{___recipeName.text} ({(Config.INSTANCE.Tweak ? "Tweaking enabled" : "Tweaking disabled")})");
        }
    }
}
