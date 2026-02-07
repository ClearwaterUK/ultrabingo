using System.Collections.Generic;
using HarmonyLib;
using UltrakillBingoClient;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(CheatsController), "Start")]
public class ForceDisableKeepEnabledCheat
{
    [HarmonyPrefix]
    public static bool forceDisableKeepEnabeldCheat()
    {
        if (GameManager.IsInBingoLevel)
        {
            MonoSingleton<PrefsManager>.Instance.SetBool("cheat.ultrakill.keep-enabled", false);
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(CheatsManager), "Start")]
public class RemoveCheatFlagInBingo
{
    [HarmonyPostfix]
    public static void disableCheatFlagInBingo(CheatsManager __instance,ref Dictionary<string, ICheat> ___idToCheat)
    {
        if(GameManager.IsInBingoLevel)
        {
            if (GameManager.CurrentGame.gameSettingsArray["gameModifier"] > 0 &&
                            getSceneName().Contains("Level "))
            {
                GameManager.cheatList = ___idToCheat;
                
                MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
                CheatsManager.Instance.SetCheatActive(___idToCheat["ultrakill.disable-enemy-spawns"],true,false);
                
                if (GameManager.CurrentGame.gameSettingsArray["gameModifier"] == 2)
                {
                    CheatsManager.Instance.SetCheatActive(___idToCheat["ultrakill.hide-weapons"],true,false);
                }
            }
            
            MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
        }
    }
}