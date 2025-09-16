using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class RegisterTicket : SendMessage
{
    public new string messageType = "RegisterTicket";
    
    public string steamTicket;
    public string steamId;
    public string steamUsername;
    public int gameId;
}