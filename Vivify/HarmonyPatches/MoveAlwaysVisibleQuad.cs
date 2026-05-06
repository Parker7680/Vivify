using HarmonyLib;
using Heck;
using UnityEngine;

namespace Vivify.HarmonyPatches;

[HeckPatch(PatchType.Features)]
internal static class MoveAlwaysVisibleQuad
{
    // still dont really know what this thing is for, but it has a shadowcaster so lets bump it out of view.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AlwaysVisibleQuad), nameof(AlwaysVisibleQuad.OnEnable))]
    private static void MoveIt(AlwaysVisibleQuad __instance)
    {
        __instance.transform.position = new Vector3(0, -10, 0);
    }
}
