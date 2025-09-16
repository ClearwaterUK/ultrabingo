using System.Collections.Generic;
using Newtonsoft.Json;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class FetchGamesRequest : SendMessage
{
    public new string messageType = "FetchGames";
}

public class FetchGamesResponse : MessageResponse
{
    public string status;
    public string gameData;
}

public class PublicGameData
{
    public string R_PASSWORD;
    public int R_CURRENTPLAYERS;
    public string C_USERNAME;
    public int R_MAXPLAYERS;
    public int R_DIFFICULTY;
}

public static class FetchGamesReponseHandler
{
    public static void handle(FetchGamesResponse response)
    {
        List<PublicGameData> games = JsonConvert.DeserializeObject<List<PublicGameData>>(response.gameData);
        NetworkManager.DisconnectWebSocket(1000,"GameList");
        
        BingoBrowser.PopulateGames(games);
    }
}