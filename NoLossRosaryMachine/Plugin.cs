using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;

namespace NoLossRosaryMachine
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> rosaryCostDecrease;
        private static new ManualLogSource Logger;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            rosaryCostDecrease = Config.Bind("General", "RosaryCostDecrease", 20, "Amount to decrease the cost of the rosary string machine by.");
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
        }

        [HarmonyPatch(typeof(DialogueYesNoV2), nameof(DialogueYesNoV2.DoOpen))]
        [HarmonyPrefix]
        static void PatchStringMachines(DialogueYesNoV2 __instance)
        {
            if (__instance.owner.name == "rosary_string_machine")
            {
                var defaultRosaryCost = __instance.CurrencyCost.Value;
                __instance.CurrencyCost.obj = null;
                __instance.CurrencyCost.value = defaultRosaryCost - rosaryCostDecrease.Value;
            }
        }
    }
}
