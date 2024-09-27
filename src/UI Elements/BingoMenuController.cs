﻿using System.Threading.Tasks;
using UltrakillBingoClient;
using UnityEngine;
using static UltraBINGO.CommonFunctions;


namespace UltraBINGO.UI_Elements;

public static class BingoMenuController
{

    
    public static void LoadBingoLevel(string levelName, string levelCoords)
    {
        //Force disable cheats and major assists, set difficulty to difficulty of the game set by the host.
        MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", false);
        MonoSingleton<AssistController>.Instance.cheatsEnabled = false;
        MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", GameManager.CurrentGame.gameSettings.difficulty);
        
        int row = int.Parse(levelCoords[0].ToString());
        int column = int.Parse(levelCoords[2].ToString());
        
        GameManager.isInBingoLevel = true;
        GameManager.currentRow = row;
        GameManager.currentColumn = column;
        SceneHelper.LoadScene(levelName);
    }
    
    public static async void LoadBingoLevelFromPauseMenu(string levelCoords)
    {
        int row = int.Parse(levelCoords[0].ToString());
        int column = int.Parse(levelCoords[2].ToString());
        
        GameManager.currentRow = row;
        GameManager.currentColumn = column;
        
        string levelName = GameManager.CurrentGame.grid.levelTable[levelCoords].levelName;
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Moving to "+levelName + "...");
        await Task.Delay(1000);
        SceneHelper.LoadScene(levelName);
    }
    
    public static void ExitBingoLevel()
    {
        
    }
    
    public static void ReturnToMenu(GameObject bingoMenu)
    {
        Logging.Message("Exiting menu");
        BingoEncapsulator.Root.SetActive(false);
        //bingoMenu.SetActive(false);
        GetGameObjectChild(bingoMenu.transform.parent.parent.gameObject,"Chapter Select").SetActive(true);
        NetworkManager.DisconnectWebSocket(1001,"Client exited Bingo menu");
    }
    
    public static void CreateRoom()
    {
        NetworkManager.CreateRoom();
    }
    
    public static void StartGame()
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("The game has begun!");
        GameManager.MoveToCard();

    }
    
}