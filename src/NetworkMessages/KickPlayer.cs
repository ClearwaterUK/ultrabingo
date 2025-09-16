using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class KickPlayer : SendMessage
{
    public new string messageType = "KickPlayer";
    
    public int gameId;
    public string playerToKick;
    
    public RegisterTicket ticket;
}

public class Kicked : MessageResponse
{
    
}

public static class KickHandler
{
    public static void handle()
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("You were kicked from the game.");
        NetworkManager.DisconnectWebSocket();
        GameManager.ClearGameVariables();
        //If dc'ing from lobby/card/end screen, return to the bingo menu.
        BingoEncapsulator.BingoCardScreen.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoEndScreen.SetActive(false);
        BingoEncapsulator.BingoMenu.SetActive(true);
    }
}


public class KickNotification : MessageResponse
{
    public string playerToKick;
    public string steamId;
}

public static class KickNotificationHandler
{
    public static void handle(KickNotification response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.playerToKick +  " was kicked from the game.");
        GameManager.CurrentGame.currentPlayers.Remove(response.steamId);
        if(getSceneName() == "Main Menu") {GameManager.RefreshPlayerList();}
        
    }
}