﻿using System.Collections.Generic;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoBrowser
{
    public static GameObject Root;
    public static GameObject FetchText;
    public static GameObject GameTemplate;
    public static GameObject GameListWrapper;
    public static GameObject GameList;
    public static GameObject Back;
    
    public static void LockUI()
    {
        
    }
    
    public static void UnlockUI()
    {
        
    }
    
    public static void Init(ref GameObject BingoGameBrowser)
    {
        FetchText = GetGameObjectChild(BingoGameBrowser,"FetchText");
        Back = GetGameObjectChild(BingoGameBrowser,"Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate
        {
            BingoEncapsulator.BingoGameBrowser.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
        });

        GameListWrapper = GetGameObjectChild(BingoGameBrowser,"GameList");
        GameList = GetGameObjectChild(GetGameObjectChild(GameListWrapper,"Viewport"),"Content");
        GameTemplate = GetGameObjectChild(GameList,"GameTemplate");
        
    }
    
    public static void clearOldGames()
    {
        foreach(Transform child in GameList.transform)
        {
            if(child.gameObject.name != "GameTemplate")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
    
    public static void PopulateGames(List<PublicGameData> games)
    {
        Dictionary<int,string> difficultyNames = new Dictionary<int, string>()
        {
            {0,"HARMLESS"},
            {1, "LENIENT"},
            {2, "STANDARD"},
            {3, "VIOLENT"},
            {4, "BRUTAL"}
        };
        
        if(games.Count == 0)
        {
            Logging.Warn("No public games available");
            FetchText.GetComponent<TextMeshProUGUI>().text = "No public games found. Check back later.";
            return;
        }
        
        //Clear previous games
        clearOldGames();
        
        foreach(PublicGameData game in games)
        {
            GameObject gameBar = GameObject.Instantiate(GameTemplate,GameTemplate.transform.parent);
            GetGameObjectChild(gameBar,"HostName").GetComponent<Text>().text = game.C_USERNAME;
            GetGameObjectChild(gameBar,"Difficulty").GetComponent<Text>().text = difficultyNames[game.R_DIFFICULTY];
            GetGameObjectChild(gameBar,"Players").GetComponent<Text>().text = game.R_CURRENTPLAYERS + "/" + game.R_MAXPLAYERS;
            GetGameObjectChild(GetGameObjectChild(gameBar,"JoinWrapper"),"JoinButton").GetComponent<Button>().onClick.AddListener(delegate
            {
                BingoMenuController.JoinRoom(game.R_PASSWORD);
            });
            gameBar.SetActive(true);
        }
        FetchText.SetActive(false);
        GameListWrapper.SetActive(true);
        GameList.SetActive(true);
    }
    
    public static void FetchGames()
    {
        FetchText.SetActive(true);
        GameListWrapper.SetActive(false);
        GameList.SetActive(false);
        FetchText.GetComponent<TextMeshProUGUI>().text = "Fetching games, please wait...";
        NetworkManager.RequestGames();
    }
}