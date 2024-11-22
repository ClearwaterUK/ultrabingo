﻿using System.Collections.Generic;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class TeamSettings : SendMessage
{
    public string messageType = "UpdateTeamSettings";
    
    public int gameId;
    
    public Dictionary<string,int> teams;
    
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
        
        if(GameManager.playerIsHost())
        { 
            msg = (response.status == 0 ? "Teams have been set. The room has been locked." : "Teams have been cleared. The room has been unlocked.");
        }
        else
        {
            msg = (response.status == 0 ? "The host has set the teams. The room has been locked." : "The host has cleared the teams. The room has been unlocked.");
        }
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}

public class ClearTeamSettings : SendMessage
{
    public string messageType = "ClearTeams";
    
    public int gameId;
}