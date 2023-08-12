using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Techntonica_BeltTweaks.Singleton;
using UnityEngine;
using UnityEngine.Animations;

namespace Techntonica_BeltTweaks.Patches.Transpiler
{
    [HarmonyPatch()]
    internal class ConveyorBuilder_Transpiler
    {
        delegate void ManipulateHeight(Vector3Int point, ref int[] arrayHeights, int countBelts);
        delegate void CheckBeltValidity(ConveyorBuilder instance, ref int num4);

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ConveyorBuilder), "UpdateEndPoint")]
        public static IEnumerable<CodeInstruction> UpdateEndPoint_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // adjust the belt height in the height array which later gets used to update the internal belt data
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Resize"),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ConveyorBuilder), "curBeltCount")));

            matcher.Advance(1);
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_1),       // point (start point)
                new CodeInstruction(OpCodes.Ldloca_S, 6),   // array
                new CodeInstruction(OpCodes.Ldloc_S, 4),    // num2
                Transpilers.EmitDelegate<ManipulateHeight>((Vector3Int point, ref int[] arrayHeights, int countBelts) =>
                {
                    bool increaseHeight = !InputHandler.instance.playerInputBlocked && !InputHandler.instance.playerInputBlockedOverride && Input.GetKeyDown(KeyCode.I);
                    bool decreaseHeight = !InputHandler.instance.playerInputBlocked && !InputHandler.instance.playerInputBlockedOverride && Input.GetKeyDown(KeyCode.Minus);
                    bool resetAll = !InputHandler.instance.playerInputBlocked && !InputHandler.instance.playerInputBlockedOverride && Input.GetKeyDown(KeyCode.N);
                    bool tweak = !InputHandler.instance.playerInputBlocked && !InputHandler.instance.playerInputBlockedOverride && Input.GetKeyDown(KeyCode.X);

                    if (tweak)
                    {
                        Config.INSTANCE.Tweak = !Config.INSTANCE.Tweak;
                        Config.INSTANCE.ClearCacheUntil(countBelts - 1);
                    }

                    if (resetAll)
                    {
                        Config.INSTANCE.ResetBeltValues();
                    }

                    if (Config.INSTANCE.DesiredBeltHeight == Config.RESETVALUE)
                    {
                        Config.INSTANCE.DesiredBeltHeight = arrayHeights[countBelts - 1]; // default to game computed end height
                    }

                    // prevent multiple height changes at the same belt
                    if (increaseHeight && !Config.INSTANCE.ModificationIsLocked && Config.INSTANCE.Tweak)
                    {
                        Config.INSTANCE.DesiredBeltHeight++;

                        Config.INSTANCE.ClearCacheUntil(countBelts - 1);
                        Config.INSTANCE.ModificationIsLocked = true;
                    }
                    if (decreaseHeight && !Config.INSTANCE.ModificationIsLocked && Config.INSTANCE.Tweak)
                    {
                        Config.INSTANCE.DesiredBeltHeight--;

                        Config.INSTANCE.ClearCacheUntil(countBelts - 1);
                        Config.INSTANCE.ModificationIsLocked = true;
                    }

                    // determine first-time-placement of belt by an increasing number of belts
                    if(Config.INSTANCE.LastCountBelts == Config.RESETVALUE)
                    {
                        Config.INSTANCE.LastCountBelts = countBelts;
                    }

                    int delta;
                    if (Config.INSTANCE.DesiredBeltHeight != arrayHeights[countBelts - 1])
                    {
                        delta = Config.INSTANCE.DesiredBeltHeight - arrayHeights[countBelts - 1];
                    }
                    else
                    {
                        delta = 0;
                    }
                    bool isNegative = delta < 0;

                    for (int i = 1; i <= countBelts; i++)
                    {
                        bool isCached = Config.INSTANCE.DeltaCache.ContainsKey(countBelts - i);

                        if (isCached)
                        {
                            arrayHeights[countBelts - i] = Config.INSTANCE.DeltaCache[countBelts - i];
                        }
                        else if(delta != 0 && Config.INSTANCE.Tweak)
                        {
                            arrayHeights[countBelts - i] += delta;
                            delta += isNegative ? 1 : -1;
                        }

                        if (!isCached && Config.INSTANCE.Tweak)
                        {
                            Config.INSTANCE.DeltaCache.Add(countBelts - i, arrayHeights[countBelts - i]);
                        }
                    }

                    Config.INSTANCE.LastCountBelts = countBelts;
                }));

            // now recheck every belt for valid build spot. this is done after the internal belt data got updated with the modified heights from above.
            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Clt),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ConveyorBuilder), "canBuildHere")));
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloca_S, 7), // if this is < 0 then canBuildHere will be true
                Transpilers.EmitDelegate<CheckBeltValidity>((ConveyorBuilder instance, ref int num4) =>
                {
                    num4 = -1;
                    // the game reserves 20 extra but stores the actual belt count in this variable
                    for(int i = 0; i < instance.curBeltCount && num4 < 0; i++)
                    {
                        bool intersect = (bool)AccessTools.Method(typeof(ConveyorBuilder), "CheckForIntersect").Invoke(instance, new object[] { instance.beltDatas[i].pos });
                        if (intersect)
                        {
                            num4 = 1;
                        }
                    }
                }));

            return matcher.InstructionEnumeration();
        }
    }
}
