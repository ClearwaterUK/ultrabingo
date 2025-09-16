using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class LeaveGameRequest : SendMessage
{
    public new string messageType = "LeaveGame";
    
    public int roomId;
    public string username;
    public string steamId;
}

