using System.Threading.Tasks;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class DisconnectSignal : MessageResponse
{
    public int disconnectCode;
    public string disconnectMessage;
}

public static class DisconnectSignalHandler
{
    public static async void handle(DisconnectSignal response = null)
    {
        string disconnectReason = "";
        switch(response.disconnectMessage)
        {
            case "HOSTLEFTGAME": {disconnectReason = "The host has left the game. The game will be ended.";break;}
            case "HOSTDROPPED": {disconnectReason = "The host has lost connection. The game will be ended.";break;}
            case "RECONNECTFAILED": {disconnectReason = "Failed to reconnect.";break;}
            case "GAMEENDED": {disconnectReason = "The game has already ended.";break;}
            default: {disconnectReason = "Disconnected for an unspecified reason (check console).\nThe game will be ended.";break;}
        }

        GameManager.ClearGameVariables();
        //If the player is in-game, warn them of returning to menu in 5 seconds.
        if(getSceneName() == "Main Menu")
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(disconnectReason);
            BingoEncapsulator.BingoCardScreen.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoEndScreen.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
        }
        else
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(disconnectReason+"\nExiting in 5 seconds...");
            await Task.Delay(5000);
            SceneHelper.LoadScene("Main Menu");
        }
    }
}

public class DisconnectNotification : MessageResponse
{
    public string username;
    public string steamId;
}

public static class DisconnectNotificationHandler
{
    public static void handle(DisconnectNotification response)
    {
        string message = response.username + " has left the game.";
        if(GameManager.PlayerIsHost() && BingoLobby.TeamComposition.value == 1 && !GameManager.IsInBingoLevel) {message += "\n Please recalibrate teams.";}
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(message);
        GameManager.CurrentGame.currentPlayers.Remove(response.steamId);
        GameManager.RefreshPlayerList();
    }
}
