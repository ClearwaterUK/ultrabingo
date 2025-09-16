using System.Collections.Generic;
using System.Linq;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class JoinRoomRequest : SendMessage
{
    public new string messageType = "JoinRoom";
    
    public string password;
    public string username;
    public string steamId;
    public string rank;
    
}

public class JoinRoomResponse : MessageResponse
{
    public int status;
    public int roomId;
    public Game roomDetails;

    public string joinMidgameTeam;
    public List<string> joinMidgameTeammates;
    public int joinMidGameDominationTime;

    public bool needsTeam;
    public string joinMidGameTeam;
}

public static class JoinRoomResponseHandler
{
    public static Dictionary<int,string> messages = new Dictionary<int, string>()
    {
        {-6,"You have been kicked from this game."},
        {-5, "<color=orange>You are banned from playing Baphomet's Bingo.</color>"},
        {-4, "Game has already started."},
        {-3, "Game is not accepting new players."},
        {-2, "Game has already started."},
        {-1, "Game does not exist."},
    };

    public static void handle(JoinRoomResponse response)
    {
        string msg = "Failed to join: ";
        
        if(response.status < 0)
        {
            msg += messages[response.status];
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
        }
        else
        {
            Logging.Warn("Mid join-status code: " + response.status);
            if (response.status == 1) //Mid-game join
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joined game in progress.");
                GameManager.SetupJoinMidgameDetails(response.roomDetails, response.joinMidgameTeam, response.joinMidgameTeammates,response.joinMidGameDominationTime,response.needsTeam);
            }
            else //Normal join
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joined game.");
                GameManager.SetupGameDetails(response.roomDetails,"",false);
            }
        }
        
        BingoMainMenu.UnlockUI();
        BingoBrowser.UnlockUI();
    }
}