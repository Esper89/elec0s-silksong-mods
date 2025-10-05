using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace NoLossRosaryMachine
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Hollow Knight Silksong.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> rosaryCostDecrease;
        private static new ManualLogSource Logger;
        private static int defaultRosaryCost = -1;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Plugin));
            rosaryCostDecrease = Config.Bind("General", "RosaryCostDecrease", 20, "Amount to decrease the cost of the rosary string machine by.");
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
        }

        [HarmonyPatch(typeof(PlayMakerNPC), "SendEvent")]
        [HarmonyPrefix]
        private static void OnStartDialoguePrefix(PlayMakerNPC __instance, ref string eventName)
        {
            // Thanks to https://github.com/BlueRaja/Silksong-Mods/ for making this easy
            if (__instance.name == "rosary_string_machine" && eventName == "INTERACT")
            {
                var fsm = Traverse.Create(__instance).Field<PlayMakerFSM>("dialogueFsm").Value;
                var costReferenceRef = fsm.FsmVariables.ObjectVariables.FirstOrDefault(o => o.Value is CostReference);
                if (costReferenceRef != null)
                {
                    CostReference costReference = (CostReference)costReferenceRef.Value;
                    if (defaultRosaryCost == -1)
                    {
                        defaultRosaryCost = costReference.Value;
                        Logger.LogInfo($"Stored default cost of rosary string machine: {defaultRosaryCost}");
                    }
                    int newCost = defaultRosaryCost - rosaryCostDecrease.Value;
                    Traverse.Create(costReference).Field<int>("value").Value = newCost;
                    Logger.LogInfo($"Set cost of rosary string machine to {newCost}");
                }
                else
                {
                    Logger.LogError("Could not find cost reference for rosary_string_machine");
                }
            }
        }
    }
}
