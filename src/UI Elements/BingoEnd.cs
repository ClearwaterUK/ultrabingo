using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoEnd
{
    public static GameObject Root;
    public static GameObject WinnerIndicator;
    public static GameObject WinningPlayers;
    public static GameObject Stats;
    public static GameObject LeaveGame;
    
    public static string winningTeam;
    public static string winningPlayers;
    public static string timeElapsed;
    public static int numOfClaims;
    public static string firstMap;
    public static string lastMap;
    public static float bestStatValue;
    public static string bestStatName;
    
    public static int endCode;
    public static string tiedTeams;
    
    public static async void ShowEndScreen()
    { 
        await Task.Delay(50); //Give the game a moment to fully load back into the menu before displaying
        
        GetGameObjectChild(GetInactiveRootObject("Canvas"),"Main Menu (1)").SetActive(false);
        BingoEncapsulator.Root.SetActive(true);
        BingoEncapsulator.BingoMenu.SetActive(false);
        BingoEncapsulator.BingoCardScreen.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoEndScreen.SetActive(true);
        
        string message = "";
        switch(endCode)
        {
            case 0:
            {
                message = "The <color="+winningTeam.ToLower()+">" + winningTeam + " </color>team has won the game!";
                break;
            }
            case 1:
            {
                message = "No team won the game.";
                break;
            }
            case 2:
            {
                message = "The " + tiedTeams + " teams have tied!";
                break;
            }
        }
        
        WinnerIndicator.GetComponent<TextMeshProUGUI>().text = message;
        
        GetGameObjectChild(WinningPlayers,"Text (TMP) (1)").GetComponent<TextMeshProUGUI>().text = winningPlayers;
        GetGameObjectChild(GetGameObjectChild(Stats,"TimeElapsed"),"Value").GetComponent<TextMeshProUGUI>().text = "<color=orange>"+timeElapsed+"</color>";
        GetGameObjectChild(GetGameObjectChild(Stats,"TotalClaims"),"Value").GetComponent<TextMeshProUGUI>().text = "<color=orange>"+numOfClaims+"</color>";
        GetGameObjectChild(GetGameObjectChild(Stats,"FirstMap"),"Value").GetComponent<TextMeshProUGUI>().text = "<color=orange>"+firstMap+"</color>";
        GetGameObjectChild(GetGameObjectChild(Stats,"LastMap"),"Value").GetComponent<TextMeshProUGUI>().text = "<color=orange>"+lastMap+"</color>";
        
        GetGameObjectChild(GetGameObjectChild(Stats,"HighestStat"),"Value").GetComponent<TextMeshProUGUI>().text =
            "<color=orange>" + bestStatValue + " </color>(<color=orange>" + bestStatName + "</color>)";
        Root.SetActive(true);
        
    }
    
    public static void Init(ref GameObject BingoEndScreen)
    {
        if(Root == null)
        {
            Root = new GameObject();
        }
        Root.name = "BingoEndScreen";
        
        WinnerIndicator = GetGameObjectChild(BingoEndScreen,"WinningTeam");
        
        WinningPlayers = GetGameObjectChild(BingoEndScreen,"WinningPlayers");
        
        Stats = GetGameObjectChild(BingoEndScreen,"Stats");
        
        LeaveGame = GetGameObjectChild(BingoEndScreen,"LeaveGame");
        LeaveGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
            BingoMapPoolSelection.ClearList(true);
        });
        LeaveGame.transform.SetParent(BingoEndScreen.transform);
        BingoEndScreen.SetActive(false);    
    }
}