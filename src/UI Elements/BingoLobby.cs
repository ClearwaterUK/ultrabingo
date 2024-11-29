﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoLobby 
{
    public static GameObject PlayerList;
    public static GameObject ReturnToBingoMenu;
    public static GameObject SelectMaps;
    public static GameObject SetTeams;
    public static GameObject StartGame;
    public static GameObject RoomIdDisplay;

    
    public static GameObject GameOptions;
    public static TMP_InputField MaxPlayers;
    public static TMP_InputField MaxTeams;
    public static TMP_Dropdown TeamComposition;
    public static TMP_Dropdown GridSize;
    public static TMP_Dropdown GameType;
    public static TMP_Dropdown Difficulty;
    public static Toggle RequirePRank;
    
    
    public static void onMaxPlayerUpdate(string playerAmount)
    {
        int amount = int.Parse(playerAmount);
        GameManager.CurrentGame.gameSettings.maxPlayers = Mathf.Clamp(amount,2,16);
        
        MaxPlayers.text = Mathf.Clamp(amount,2f,16f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onMaxTeamUpdate(string teamAmount)
    {
        int amount = int.Parse(teamAmount);
        GameManager.CurrentGame.gameSettings.maxTeams = Mathf.Clamp(amount,2,4);
        MaxTeams.text = Mathf.Clamp(amount,2f,4f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onTeamCompositionUpdate(int value)
    {
        GameManager.CurrentGame.gameSettings.teamComposition = value;
        SetTeams.SetActive(value == 1 && GameManager.PlayerIsHost());
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGridSizeUpdate(int value)
    {
        GridSize.value = value;
        GameManager.CurrentGame.gameSettings.gridSize = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGameTypeUpdate(int value)
    {
        GameType.value = value;
        GameManager.CurrentGame.gameSettings.gameType = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onDifficultyUpdate(int value)
    {
        GameManager.CurrentGame.gameSettings.difficulty = value;
        Difficulty.value = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onPRankRequiredUpdate(bool value)
    {
        RequirePRank.isOn = value;
        GameManager.CurrentGame.gameSettings.requiresPRank = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void updateFromNotification(UpdateRoomSettingsNotification newSettings)
    {
        MaxPlayers.text = newSettings.maxPlayers.ToString();
        MaxTeams.text = newSettings.maxTeams.ToString();
        TeamComposition.value = newSettings.teamComposition;
        RequirePRank.isOn = newSettings.PRankRequired;
        GameType.value = newSettings.gameType;
        Difficulty.value = newSettings.difficulty;
        GridSize.value = newSettings.gridSize;
        
        GameManager.CurrentGame.gameSettings.maxPlayers = newSettings.maxPlayers;
        GameManager.CurrentGame.gameSettings.maxTeams = newSettings.maxTeams;
        GameManager.CurrentGame.gameSettings.teamComposition = newSettings.teamComposition;
        GameManager.CurrentGame.gameSettings.requiresPRank = newSettings.PRankRequired;
        GameManager.CurrentGame.gameSettings.gameType = newSettings.gameType;
        GameManager.CurrentGame.gameSettings.difficulty = newSettings.difficulty;
        GameManager.CurrentGame.gameSettings.gridSize = newSettings.gridSize;
    }
    
    public static void Init(ref GameObject BingoLobby)
    {
        //Player list
        PlayerList = GetGameObjectChild(BingoLobby,"BingoLobbyPlayers");
        
        //Leave game button
        ReturnToBingoMenu = GetGameObjectChild(BingoLobby,"LeaveGame");
        ReturnToBingoMenu.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.LeaveGame();
        });
        
        SelectMaps = GetGameObjectChild(BingoLobby,"SelectMaps");
        SelectMaps.GetComponent<Button>().onClick.AddListener( delegate
        {
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoMapSelectionMenu.SetActive(true);
            BingoMapSelection.Setup();
        });
        
        SetTeams = GetGameObjectChild(BingoLobby,"SetTeams");
        SetTeams.GetComponent<Button>().onClick.AddListener(delegate
        {
            BingoSetTeamsMenu.Setup();
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoSetTeams.SetActive(true);
        });
        
        //Start game button
        StartGame = GetGameObjectChild(BingoLobby,"StartGame");
        StartGame.GetComponent<Button>().onClick.AddListener(delegate
        {
            if(GameManager.PreStartChecks())
            {
                GameManager.StartGame();
            }
        });
        
        //Room id text
        RoomIdDisplay = GetGameObjectChild(BingoLobby,"BingoGameID");
        
        //Game options
        GameOptions = GetGameObjectChild(BingoLobby,"BingoGameSettings");

        MaxPlayers = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxPlayers"),"Input").GetComponent<TMP_InputField>();
        MaxPlayers.onEndEdit.AddListener(onMaxPlayerUpdate);
        
        MaxTeams = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxTeams"),"Input").GetComponent<TMP_InputField>();
        MaxTeams.onEndEdit.AddListener(onMaxTeamUpdate);
        
        TeamComposition = GetGameObjectChild(GetGameObjectChild(GameOptions,"TeamComposition"),"Dropdown").GetComponent<TMP_Dropdown>();
        TeamComposition.onValueChanged.AddListener(onTeamCompositionUpdate);
        
        GridSize = GetGameObjectChild(GetGameObjectChild(GameOptions,"GridSize"),"Dropdown").GetComponent<TMP_Dropdown>();
        GridSize.onValueChanged.AddListener(onGridSizeUpdate);
        
        GameType = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameType"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameType.onValueChanged.AddListener(onGameTypeUpdate);
        
        Difficulty = GetGameObjectChild(GetGameObjectChild(GameOptions,"Difficulty"),"Dropdown").GetComponent<TMP_Dropdown>();
        Difficulty.onValueChanged.AddListener(onDifficultyUpdate);
        
        RequirePRank = GetGameObjectChild(GetGameObjectChild(GameOptions,"RequirePRank"),"Input").GetComponent<Toggle>();
        RequirePRank.onValueChanged.AddListener(onPRankRequiredUpdate);
        BingoLobby.SetActive(false);
    }
}