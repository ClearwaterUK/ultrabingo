using System.Collections.Generic;
using UltrakillBingoClient;
namespace UltraBINGO.NetworkMessages;

public class DifficultySettings : SendMessage
{
    public new string messageType = "DifficultyOverride";
    public int gameId;
    public int baseDifficulty;
    public Dictionary<string, int> difficultyOverride = new Dictionary<string, int>();
    public RegisterTicket ticket;
}