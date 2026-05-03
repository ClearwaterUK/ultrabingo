using System.Collections.Generic;
using UltraBINGO.Components;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class StartGameRequest : SendMessage
{
    public new string messageType = "StartGame";
    
    public int roomId;
    public RegisterTicket ticket;

    public Dictionary<string,BingoMapSelectionID> selectedMapIds;

}

public class StartGameResponse : MessageResponse
{
    public Game game;
    public string teamColor;
    public List<string> teammates;
    public Dictionary<string, int> difficultyOverride;
    
    public GameGrid grid;
}

public static class StartGameResponseHandler
{
    public static void handle(StartGameResponse response)
    {
        GameManager.CurrentGame = response.game;
        GameManager.CurrentTeam = response.teamColor;   
        GameManager.Teammates = response.teammates;
        GameManager.CurrentGame.grid = response.grid;
        GameManager.CurrentGame.difficultyOverride = response.difficultyOverride;
        
        switch(response.game.gameSettingsArray["gamemode"])
        {
            case 1: //Domination
            {
                GameManager.dominationTimer = response.game.gameSettingsArray["timeLimit"]*60;
                break;
            }
            default: {break;}
        }

        BingoMenuController.StartGame(response.game.gameSettingsArray["gamemode"]);
    }
}