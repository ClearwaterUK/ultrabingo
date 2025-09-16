using System;
using HarmonyLib;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(ShopZone),"TurnOn")]
public class ShopAddLevelInfo
{
    [HarmonyPostfix]
    public static void AddBingoLevelInfo(ShopZone __instance, Canvas ___shopCanvas)
    {
        if(GameManager.IsInBingoLevel && ___shopCanvas != null && !___shopCanvas.gameObject.name.Contains("Shop"))
        {
            try
            {
                TMP_Text origTip = __instance.tipOfTheDay;
                string coords = GameManager.CurrentRow + "-" + GameManager.CurrentColumn;
                
                string teamClaim = GameManager.CurrentGame.grid.levelTable[coords].claimedBy;
                float time = GameManager.CurrentGame.grid.levelTable[coords].timeToBeat;
                
                float secs = time;
                float mins = 0;
                while (secs >= 60f)
                {
                    secs -= 60f;
                    mins += 1f;
                }
                string formattedTime = mins + ":" + secs.ToString("00.000");
                
                string unclaimed = "This level is currently <color=orange>unclaimed</color>.\nHurry and be the first to <color=green>claim it for your team</color>!";
                string claimedByOwnTeam = "This level is currently <color=green>claimed by your team</color>.\n<color=orange>Choose another level</color> to claim, or <color=orange>try and improve the current requirement</color> to make it harder for other teams to reclaim!";
                string claimedByOtherTeam = "This level is currently claimed by another team.\n<color=orange>Beat</color> the current requirement to <color=green>reclaim it for your team</color>!";
                
                if(teamClaim == "NONE")
                {
                    origTip.text = unclaimed;
                }
                else
                {
                    origTip.text = "Claimed by: <color="+ teamClaim.ToLower() + ">" + teamClaim + "</color> team\n\n" +
                    "TIME TO BEAT: <color=orange>" + formattedTime  + "</color>\n\n" +
                    (teamClaim == GameManager.CurrentTeam ? claimedByOwnTeam : claimedByOtherTeam);
                }
                
                //Hide the CG and sandbox buttons
                GameObject shopObject = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(___shopCanvas.gameObject,"Background"),"Main Panel"),"Main Menu");
                
                GameObject cgButton = GetGameObjectChild(GetGameObjectChild(shopObject,"Buttons"),"CyberGrindButton");
                cgButton.SetActive(false);
                
                GameObject sandboxButton = GetGameObjectChild(GetGameObjectChild(shopObject,"Buttons"),"SandboxButton");
                sandboxButton.SetActive(false);
            }

            catch (Exception e)
            {
                Logging.Warn("This shop isn't vanilla or an error occured");
                Logging.Error(e.ToString());
            }
        }
    }
}