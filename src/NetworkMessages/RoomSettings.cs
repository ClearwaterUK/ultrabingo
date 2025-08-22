using System.Collections.Generic;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class UpdateRoomSettingsRequest : SendMessage
{
    public string messageType = "UpdateRoomSettings";
    
    public int roomId;
    public Dictionary<string, int> updatedSettings;

    public RegisterTicket ticket;
}

public class UpdateRoomSettingsNotification : MessageResponse
{
    public Dictionary<string, int> updatedSettings;
    public bool wereTeamsReset;
}

public static class UpdateRoomSettingsHandler
{
    public static void handle(UpdateRoomSettingsNotification response)
    {
        BingoLobby.updateFromNotification(response);
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The host has updated the room settings.");
    }
}