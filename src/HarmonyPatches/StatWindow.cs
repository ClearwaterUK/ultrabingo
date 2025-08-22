using HarmonyLib;
using TMPro;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(ViewModelFlip),"OnPrefChanged")]
public class WeaponPosPanelPatch
{
    [HarmonyPostfix]
    public static void changeBingoPanelPos(string key, object value)
    {
        if(key == "weaponHoldPosition" && GameManager.IsInBingoLevel && !GameManager.CurrentGame.isGameFinished())
        {
            GameObject ctr = GetGameObjectChild(GetInactiveRootObject("Canvas"),"Level Stats Controller");
            GameObject bingoPanel = GetGameObjectChild(ctr,"BingoInGamePanel");
            switch(MonoSingleton<PrefsManager>.Instance.GetInt("weaponHoldPosition", 0))
            {
                case 2: // Right
                {
                    bingoPanel.transform.localPosition = new Vector3(-425f,0f,0f);
                    break;
                }
                default: // Left/middle
                {
                    bingoPanel.transform.localPosition = new Vector3(300f,0f,0f);
                    break;
                }
            }
        }
    }
}


[HarmonyPatch(typeof(LevelStats),"Start")]
public class StatWindowStart
{
    public static GameObject originalChallengeText;
    
    [HarmonyPostfix]
    public static void setupStatWindow(ref LevelStats __instance)
    {
        if(GameManager.IsInBingoLevel)
        {
            //Prime or Encore level
            if(getSceneName().Contains("P-") || getSceneName().Contains("-E"))
            {
                GameObject majorAssists = __instance.majorAssists.transform.parent.gameObject;
                majorAssists.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";
                
                originalChallengeText = GetGameObjectChild(__instance.gameObject,"Challenge Title");
                originalChallengeText.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
                originalChallengeText.SetActive(true);
                
                __instance.GetComponent<RectTransform>().sizeDelta = new Vector2(285f,285f);
            }
            //Normal level
            else
            {
                GameObject secrets = __instance.secrets[0].transform.parent.gameObject;
                secrets.SetActive(false);
                
                GameObject challenge = __instance.challenge.transform.parent.gameObject;
                challenge.GetComponent<TextMeshProUGUI>().text = "TO BEAT:";
            
                GameObject majorAssists = __instance.majorAssists.transform.parent.gameObject;
                majorAssists.GetComponent<TextMeshProUGUI>().text = "CLAIMED BY:";
            }
        }
    }
}

[HarmonyPatch(typeof(LevelStats),"CheckStats")]
public class StatWindow
{
    public static GameObject originalChallengeText;
    
    [HarmonyPostfix]
    public static void showRequirements(ref LevelStats __instance)
    {
        if(GameManager.IsInBingoLevel)
        {
            string coords = GameManager.CurrentRow + "-" + GameManager.CurrentColumn;
            string currentTeamClaim = GameManager.CurrentGame.grid.levelTable[coords].claimedBy;
            
            float secs = GameManager.CurrentGame.grid.levelTable[coords].timeToBeat;
            float mins = 0;
            while (secs >= 60f)
            {
                secs -= 60f;
                mins += 1f;
            }
            
            string formattedTime = mins + ":" + secs.ToString("00.000");
            if(formattedTime == "0:00.000")
            {
                formattedTime = "<size=14>FINISH TO CLAIM" + (GameManager.CurrentGame.gameSettingsArray["requiresPRank"] == 1 ? "(<color=#ffa200d9>P</color>)" : "") + "</size>";
            }
                        
            string colorTag = (currentTeamClaim != "NONE"
                ? ("<color="+GameManager.CurrentGame.grid.levelTable[coords].claimedBy.ToLower()+">" + currentTeamClaim + "</color>")  : "NONE");
            
            //If we're in a Prime or Encore level, use major assists text for to beat, and challenge text for claimed by.
            if(getSceneName().Contains("P-") || getSceneName().Contains("-E"))
            {
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = formattedTime;

                GetGameObjectChild(GetGameObjectChild(__instance.gameObject,"Challenge Title"),"Challenge").GetComponent<TextMeshProUGUI>().text = colorTag;
                GetGameObjectChild(GetGameObjectChild(__instance.gameObject,"Challenge Title"),"Challenge").GetComponent<TextMeshProUGUI>().fontSize = 20;
            }
            else
            {
                __instance.challenge.GetComponent<TextMeshProUGUI>().text = formattedTime;
                
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().text = colorTag;
                __instance.majorAssists.GetComponent<TextMeshProUGUI>().fontSize = 20;
            }
        }
    }
}