using System;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

// Submit a run to the server.
public class SubmitRunRequest : SendMessage
{
    public new string messageType = "SubmitRun";
    
    public string team;
    public int gameId;
    
    public int column;
    public int row;
    
    public string levelName;
    public string levelId;
    public string playerName;
    public string steamId;
    
    public float time;
    
    public RegisterTicket ticket;
}

// Notify all players that a map has been claimed/reclaimed/improved.
public class LevelClaimNotification : PlayerNotification
{
    public int claimType; //0:Claimed , 1: Improved, 2: Reclaimed
    public string username;

    public string levelname;
    public string team;
    
    public int row;
    public int column;
    
    public float newTimeRequirement;
    
    public bool isMapVoted;
}

public static class LevelClaimHandler
{
    public static void handle(LevelClaimNotification response)
    {
        try
        {
            if(response == null)
            {
                Logging.Error("Level claim response was null!");
                throw new ArgumentNullException();
            }
            string actionType = "";
            switch(response.claimType)
            {
                case 0: {actionType = "claimed "; break;}
                case 1: {actionType = "improved "; break;}
                case 2: {actionType = "reclaimed "; break;}
            }
                        
            float secs = response.newTimeRequirement;
            float mins = 0;
            while (secs >= 60f)
            {
                secs -= 60f;
                mins += 1f;
            }
            
            //Hide set time if the corresponding setting is enabled.
            string formattedTime = 
                (GameManager.CurrentGame.gameSettingsArray["hidePlayerTimes"] == 1 ? ""
            : "(" + mins + ":" + secs.ToString("00.000") + ")");

            //Hide the level name if the corresponding setting is enabled.
            response.levelname = (GameManager.CurrentGame.gameSettingsArray["hideLevelNames"] == 1 ? "a level" : ("<color=orange>"+response.levelname+"</color>"));
            
            string broadcastString = "<color="+response.team.ToLower()+">"+response.username + "</color> has " + actionType + response.levelname
                                     + " for the <color="+ response.team.ToLower()+">" + response.team + " </color>team <color=orange>"+formattedTime+"</color>.";
            
            if(response.isMapVoted)
            {
                broadcastString += "\n Cancelling reroll vote.";
            }
            
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(broadcastString);
            GameManager.UpdateCards(response.row,response.column,response.team,response.username,response.newTimeRequirement);
        }
        
        catch (Exception e)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("A level was claimed by someone but was unable to update the grid.\nCheck BepInEx console and report it to Clearwater!");
            Logging.Error(e.Message);
        }
    }
}