using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class VerifyModRequest : SendMessage
{
    public string messageType = "VerifyModList";
    
    public List<string> clientModList;
    public string steamId;
    
    public VerifyModRequest(List<string> clientModList,string steamId)
    {
        this.clientModList = clientModList;
        this.steamId = steamId;
    }
}

public class ModVerificationResponse : MessageResponse
{
    public List<string> nonWhitelistedMods;
    
    public string latestVersion;
    
    public string motd;
    
    public string availableRanks;

    public bool canUseChat;

}

public static class ModVerificationHandler
{
    public static void handle(ModVerificationResponse response)
    {
        //Check mod whitelist
        NetworkManager.modlistCheckPassed = (response.nonWhitelistedMods.Count == 0);
        
        if(!NetworkManager.modlistCheckPassed) {UIManager.nonWhitelistedMods = response.nonWhitelistedMods;}
        
        NetworkManager.startupDone = true;
        
        //Check mod updates
        Version localVersion = new Version(Main.pluginVersion);
        Version latestVersion = new Version(response.latestVersion);
        
        try
        {
            BingoMainMenu.VersionNum.text = Main.pluginVersion;
        }
        catch (Exception e)
        {
            Logging.Warn("Wasn't able to access BingoMainMenu.VersionNum component for some reason");
            Logging.Warn(e.ToString());
        }
        
        switch(localVersion.CompareTo(latestVersion))
        {
            case -1:
            {
                Logging.Message("--UPDATE AVAILABLE--");
                Main.UpdateAvailable = true;
                GetGameObjectChild(BingoMainMenu.VersionInfo,"UpdateText").SetActive(true);
                break;
            }
            default: {Main.UpdateAvailable = false;break;}
        }
        
        //Get MOTD
        string motd = response.motd;
        BingoMainMenu.MOTD = motd;
        
        Main.Queue(() =>
        {
            BingoMainMenu.MOTDText.text = motd;
        });
        /*ThreadDispatcher.Queue(() =>
        {
            BingoMainMenu.MOTDText.text = motd;
        });*/


        //Check ranks
        if(response.availableRanks != "")
        {
            Main.Queue(() =>
            {
                BingoMainMenu.RankSelectionDropdown.ClearOptions();
            
                List<string> ranks = response.availableRanks.Split(',').ToList();
                BingoMainMenu.RankSelectionDropdown.AddOptions(ranks);
                BingoMainMenu.ranks = ranks;
                NetworkManager.requestedRank = BingoMainMenu.RankSelectionDropdown.options[0].text;
            
                //Check if the previously used rank is available in the list. If so, set it as default.
                if(ranks.Contains(NetworkManager.lastRankUsedConfig.Value))
                {
                    //NetworkManager.requestedRank = NetworkManager.lastRankUsedConfig.Value;
                    BingoMainMenu.RankSelectionDropdown.value = ranks.IndexOf(NetworkManager.lastRankUsedConfig.Value);
                }
            
                GameManager.hasRankAccess = true;
            });
        }
        else
        {
            BingoMainMenu.RankSelection.SetActive(false);
        }
        
        GameManager.canUseChat = response.canUseChat;
        NetworkManager.DisconnectWebSocket(1000,"ModCheckDone");

        if (!NetworkManager.modlistCheckPassed)
        {
            Logging.Warn("Mod whitelist check failed");
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Baphomet's Bingo mod whitelist check failed.\n" +
            "Please click on the Baphomet's Bingo option in the difficulty select menu for more details.");
        }
        else if (Main.UpdateAvailable)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Baphomet's Bingo update available!\n"
                                                                      + "Please update to play this gamemode.");
        }
        else
        {
            Main.Queue(() =>
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Baphomet's Bingo is ready to go!");
            });
        }
    }
}