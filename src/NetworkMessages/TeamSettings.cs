using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class TeamSettings : SendMessage
{
    public new string messageType = "UpdateTeamSettings";
    public int gameId;
    public Dictionary<string,int> teams;
    public RegisterTicket ticket;
    
}

public class UpdateTeamsNotification : MessageResponse
{
    public int status;
}

public static class UpdateTeamsNotificationHandler
{
    public static void handle(UpdateTeamsNotification response)
    {
        string msg;
        
        if(GameManager.PlayerIsHost())
        { 
            msg = (response.status == 0 ? "Teams have been set. The room has been locked." : "Teams have been cleared. The room has been unlocked.");
            GameManager.CurrentGame.gameSettingsArray["hasManuallySetTeams"] = 1;
        }
        else
        {
            msg = (response.status == 0 ? "The host has set the teams. The room has been locked." : "The host has cleared the teams. The room has been unlocked.");
            GameManager.CurrentGame.gameSettingsArray["hasManuallySetTeams"] = 0;
        }
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}

public class ClearTeamSettings : SendMessage
{
    public new string messageType = "ClearTeams";
    
    public int gameId;
    public RegisterTicket ticket;
}