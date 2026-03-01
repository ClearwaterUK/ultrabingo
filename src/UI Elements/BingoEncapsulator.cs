using UnityEngine;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoEncapsulator
{
    public static GameObject Root;
    public static GameObject BingoMenu;
    public static GameObject BingoLobbyScreen;
    public static GameObject BingoCardScreen;
    public static GameObject BingoEndScreen;
    public static GameObject BingoSetTeams;
    
    public static GameObject BingoGameBrowser;

    public static GameObject BingoMapSelection;
    
    public static GameObject Init()
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "UltraBingo";
        
        BingoMenu = GameObject.Instantiate(AssetLoader.BingoMainMenu,Root.transform);
        BingoMainMenu.Init(ref BingoMenu);
        BingoMenu.name = "BingoMainMenu";
        BingoMenu.AddComponent<MenuEsc>();
        BingoMenu.GetComponent<MenuEsc>().previousPage = GetGameObjectChild(GetInactiveRootObject("Canvas"),"Difficulty Select (1)");
        BingoMenu.transform.SetParent(Root.transform);
        
        BingoLobbyScreen = GameObject.Instantiate(AssetLoader.BingoLobbyMenu,Root.transform);
        BingoLobby.Init(ref BingoLobbyScreen);
        BingoLobbyScreen.transform.SetParent(Root.transform);
        
        BingoCardScreen = BingoCard.Init();
        BingoCardScreen.transform.SetParent(Root.transform);
        
        BingoSetTeams = GameObject.Instantiate(AssetLoader.BingoSetTeams,Root.transform);
        BingoSetTeamsMenu.Init(ref BingoSetTeams);
        BingoSetTeams.transform.SetParent(Root.transform);
        
        BingoEndScreen = GameObject.Instantiate(AssetLoader.BingoEndScreen,Root.transform);
        BingoEnd.Init(ref BingoEndScreen);
        
        BingoGameBrowser = GameObject.Instantiate(AssetLoader.BingoGameBrowser,Root.transform);
        BingoBrowser.Init(ref BingoGameBrowser);

        BingoMapSelection = GameObject.Instantiate(AssetLoader.BingoMapSelection, Root.transform);
        BingoMapBrowser.Init(ref BingoMapSelection);
        
        return Root;
    }
}