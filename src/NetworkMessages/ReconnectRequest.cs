using System.Threading.Tasks;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class ReconnectRequest : SendMessage
{
    public new string messageType = "ReconnectRequest";
    
    public int roomId;
    public string steamId;
    public RegisterTicket ticket;
}

public class ReconnectResponse : MessageResponse
{
    public string status;
    public Game gameData;
}

public static class ReconnectResponseHandler
{
    public static async void handle(ReconnectResponse response)
    {
        switch(response.status)
        {
            case "OK":
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Reconnected successfully.");
                break;
            }
            case "end":
            {
                DisconnectSignal dc = new DisconnectSignal();
                dc.disconnectMessage = "GAMEENDED";
                DisconnectSignalHandler.handle(dc);
                //MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has already ended.");
                break;
            }
            default:
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to reconnect. Leaving game in 5 seconds...");
                GameManager.ClearGameVariables();
                await Task.Delay(5000);
                
                SceneHelper.LoadScene("Main Menu");
                break;
            }
        }
        
    }
}