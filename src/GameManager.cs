﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class GameManager
{
    public static Game CurrentGame;
    
    public static bool IsInBingoLevel = false;
    public static bool ReturningFromBingoLevel = false;
    
    public static int CurrentRow = 0;
    public static int CurrentColumn = 0;
    
    public static string CurrentTeam = "";
    public static List<String> Teammates;
    
    public static bool HasSent = false;
    public static bool EnteringAngryLevel = false;    
    public static bool TriedToActivateCheats = false;
    public static bool IsDownloadingLevel = false;
    public static bool IsSwitchingLevels = false;
    public static bool alreadyStartedVote = false;
    
    public static GameObject LevelBeingDownloaded = null;
    
    public static float dominationTimer = 0;
    
    public static bool hasRankAccess = false;
    
    public static VoteData voteData = new VoteData(false);
    
    public static async void SwapRerolledMap(string oldMapId, GameLevel level, int column, int row)
    {
        if(IsInBingoLevel && CurrentGame != null)
        {
            CurrentGame.grid.levelTable[column+"-"+row] = level;
            BingoLevelData a = GetGameObjectChild(BingoCardPauseMenu.Grid,(column+"-"+row)).GetComponent<BingoLevelData>();
            
            a.isAngryLevel = level.isAngryLevel;
            a.angryParentBundle = level.angryParentBundle;
            a.angryLevelId = level.angryLevelId;
            a.levelName = level.levelName;
        }
        
        if(oldMapId == getSceneName())
        {
            Logging.Message("Currently on old map - switching in 5 seconds");
            await Task.Delay(5000);
            Button b = GetGameObjectChild(BingoCardPauseMenu.Grid,(column+"-"+row)).GetComponent<Button>();
            b.onClick.Invoke();
        }
    }
    
    public static void UpdateGridPosition(int row, int column)
    {
        CurrentRow = row;
        CurrentColumn = column;
    }
    
    public static void ClearGameVariables()
    {
        CurrentGame = null;
        CurrentTeam = null;
        CurrentRow = 0;
        CurrentColumn = 0;
        IsInBingoLevel = false;
        ReturningFromBingoLevel = false;
        Teammates = null;
        voteData = null;

        BingoMapSelection.ClearList();
        
        //Cleanup the bingo grid if on the main menu.
        if(getSceneName() == "Main Menu")
        {
            BingoCard.Cleanup();
        }
    }
    
    public static void HumiliateSelf()
    {
        CheatActivation ca = new CheatActivation();
        ca.username = sanitiseUsername(Steamworks.SteamClient.Name);
        ca.gameId = CurrentGame.gameId;
        ca.steamId = Steamworks.SteamClient.SteamId.ToString();
        
        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ca));
    }
    
    public static void LeaveGame(bool isInLevel=false)
    {
        //Send a request to the server saying we want to leave.
        NetworkManager.SendLeaveGameRequest(CurrentGame.gameId);
        
        //When that's sent off, close the connection on our end.
        Logging.Message("Closing connection");
        NetworkManager.DisconnectWebSocket(1000,"Normal close");
        
        ClearGameVariables();
        
        if(!isInLevel)
        {
            //If dc'ing from lobby/card/end screen, return to the bingo menu.
            BingoEncapsulator.BingoCardScreen.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoEndScreen.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
            
            NetworkManager.setState(UltrakillBingoClient.State.INMENU);
        }
        else {NetworkManager.setState(UltrakillBingoClient.State.NORMAL);}
    }
    
    public static void MoveToCard(int gameType)
    {
        BingoCard.UpdateTitles(gameType);
        BingoLobby.UnlockUI();
        
        BingoEncapsulator.BingoLobbyScreen.SetActive(false);
        BingoEncapsulator.BingoCardScreen.SetActive(true);
    }
    
    //Check if we're the host of the game we're in.
    public static bool PlayerIsHost()
    {
        return Steamworks.SteamClient.SteamId.ToString() == CurrentGame.gameHost;
    }
        
    //Check if the amount of selected maps is enough to fill the grid.
    public static bool PreStartChecks()
    {
        int gridSize = CurrentGame.gameSettings.gridSize+3;
        int requiredMaps = gridSize*gridSize;
        
        if(BingoMapSelection.NumOfMapsTotal < requiredMaps)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Not enough maps selected. Add more map pools, or reduce the grid size.\n" +
                                                                      "(<color=orange>" + requiredMaps + " </color>required, <color=orange>" + BingoMapSelection.NumOfMapsTotal + "</color> selected)");
            return false;
        }
        
        if(CurrentGame.gameSettings.teamComposition == 1 && CurrentGame.gameSettings.hasManuallySetTeams == false)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Teams must be set before starting the game.");
            return false;
        }
        
        return true;
    }
    
    //Reset temporary vars on level change.
    public static void ResetVars()
    {
        HasSent = false;
        EnteringAngryLevel = false;
        TriedToActivateCheats = false;
        IsSwitchingLevels = false;
    }
    
    //Update displayed player list when a player joins/leaves game.
    public static void RefreshPlayerList()
    {
        try
        {
            if(getSceneName() == "Main Menu")
            {
                BingoLobby.PlayerList.SetActive(false);
                string players = "";
                bool isHost = (CurrentGame.gameHost == Steamworks.SteamClient.SteamId.ToString());
                
                GameObject PlayerList = GetGameObjectChild(BingoLobby.PlayerList,"PlayerList");
                GameObject PlayerTemplate = GetGameObjectChild(PlayerList,"PlayerTemplate");
                
                foreach(Transform child in PlayerList.transform)
                {
                    if(child.gameObject.name != "PlayerTemplate")
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
                
                foreach(string steamId in CurrentGame.currentPlayers.Keys.ToList())
                {
                    GameObject player = GameObject.Instantiate(PlayerTemplate,PlayerList.transform);
                    
                    string hostStripped = "";
                    //If the host name has color tags in their Steam name, strip them.
                    if(steamId == CurrentGame.gameHost)
                    {
                        hostStripped = "<color=orange>"+Regex.Replace(CurrentGame.currentPlayers[steamId].username, @"^<[^>]*>", "")+"</color>";
                    }
                    
                    GetGameObjectChild(player,"PlayerName").GetComponent<Text>().text = 
                        (steamId == CurrentGame.gameHost ? hostStripped : CurrentGame.currentPlayers[steamId].username)
                        + (CurrentGame.currentPlayers[steamId].rank != "" ? (" | " + CurrentGame.currentPlayers[steamId].rank) : "");
                    
                    GetGameObjectChild(player,"Kick").GetComponent<Button>().onClick.AddListener(delegate
                    {
                        NetworkManager.KickPlayer(steamId);
                    });
                    GetGameObjectChild(player,"Kick").transform.localScale = Vector3.one;
                    GetGameObjectChild(player,"Kick").SetActive(Steamworks.SteamClient.SteamId.ToString() == CurrentGame.gameHost && steamId != Steamworks.SteamClient.SteamId.ToString());
                    player.SetActive(true);    
                }
                BingoLobby.PlayerList.SetActive(true);
            }
        }
        catch (Exception e)
        {
            Logging.Error("Something went wrong when trying to update player list");
            Logging.Error(e.ToString());
        }
    }
    
    //Setup the bingo grid.
    public static void SetupBingoCardDynamic()
    {
        //Resize the GridLayoutGroup based on the grid size.
        GameObject gridObj = GetGameObjectChild(BingoCard.Root,"BingoGrid");
        gridObj.GetComponent<GridLayoutGroup>().constraintCount = CurrentGame.grid.size;
        gridObj.GetComponent<GridLayoutGroup>().spacing = new Vector2(30,30);
        gridObj.GetComponent<GridLayoutGroup>().cellSize = new Vector2(150,50);
        
        for(int x = 0; x < CurrentGame.grid.size; x++)
        {
            for(int y = 0; y < CurrentGame.grid.size; y++)
            {
                //Clone and set up the button and hover triggers.
                GameObject level = GameObject.Instantiate(AssetLoader.BingoCardButtonTemplate,gridObj.transform);
                
                //Label the button.
                string lvlCoords = x+"-"+y;
                level.name = lvlCoords;
                GameLevel levelObject = CurrentGame.grid.levelTable[lvlCoords];
                GetGameObjectChild(level,"Text").GetComponent<Text>().text = levelObject.levelName;
                
                //Setup the BingoLevelData component.
                level.AddComponent<BingoLevelData>();
                level.GetComponent<BingoLevelData>().column = x;
                level.GetComponent<BingoLevelData>().row = y;
                level.GetComponent<BingoLevelData>().isAngryLevel = levelObject.isAngryLevel;
                level.GetComponent<BingoLevelData>().angryParentBundle = levelObject.angryParentBundle;
                level.GetComponent<BingoLevelData>().angryLevelId = levelObject.angryLevelId;
                level.GetComponent<BingoLevelData>().levelName = levelObject.levelName;
                
                //Setup the click listener.
                level.GetComponent<Button>().onClick.RemoveAllListeners();
                level.GetComponent<Button>().onClick.AddListener(delegate
                {
                    BingoMenuController.LoadBingoLevel(levelObject.levelId,lvlCoords,level.GetComponent<BingoLevelData>());
                });
                
                level.transform.SetParent(BingoCard.Grid.transform);
                level.SetActive(true);
            }
        }
        
        //Shift the position of the grid so it's better centered.
        switch(CurrentGame.grid.size)
        {
            case 3: {gridObj.transform.localPosition = new Vector3(-200f,100f,0f);}break;
            case 4: {gridObj.transform.localPosition = new Vector3(-275f,122f,0f);}break;
            case 5: {gridObj.transform.localPosition = new Vector3(-350f,145f,0f);}break;   
        }
        
        //Display teammates.
        TextMeshProUGUI teammates = GetGameObjectChild(BingoCard.Teammates,"Players").GetComponent<TextMeshProUGUI>();
        teammates.text = "";
        foreach(string player in Teammates)
        {
            teammates.text += player + "\n";
        }
        
        //Reset votes.
        alreadyStartedVote = false;
    }

    public static async void SetupGameDetails(Game game, string password, bool isHost=true)
    {
        CurrentGame = game;
        
        BingoEncapsulator.BingoMenu.SetActive(false);
        BingoEncapsulator.BingoGameBrowser.SetActive(false);
        
        //Small hack to fix the lobby UI elements & player list appearing invisible due to a base issue with TextMeshPro and how it works
        foreach(TextMeshProUGUI text in BingoEncapsulator.BingoLobbyScreen.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            await Task.Delay(1);
            text.ForceMeshUpdate();
        }
        
        BingoEncapsulator.BingoLobbyScreen.SetActive(true); 
        
        ShowGameId(password);
        RefreshPlayerList();
        
        BingoLobby.MaxPlayers.interactable = isHost;
        BingoLobby.MaxTeams.interactable = isHost;
        BingoLobby.TimeLimit.interactable = isHost;
        BingoLobby.TeamComposition.interactable = isHost;
        BingoLobby.GridSize.interactable = isHost;
        BingoLobby.Gamemode.interactable = isHost;
        BingoLobby.Difficulty.interactable = isHost;
        BingoLobby.RequirePRank.interactable = isHost;
        BingoLobby.DisableCampaignAltExits.interactable = isHost;
        BingoLobby.GameVisibility.interactable = isHost;
        BingoLobby.StartGame.SetActive(isHost);
        BingoLobby.SelectMaps.SetActive(isHost);
        BingoLobby.RoomIdDisplay.SetActive(isHost);
        BingoLobby.CopyId.SetActive(isHost);
        BingoLobby.SetTeams.GetComponent<Button>().interactable = isHost;
        
        if(isHost)
        {
            //Reset field settings to default values.
            BingoLobby.MaxPlayers.text = 8.ToString();
            BingoLobby.MaxTeams.text = 4.ToString();
            BingoLobby.TeamComposition.value = 0;
            BingoLobby.GridSize.value = 0;
            BingoLobby.Gamemode.value = 0;
            BingoLobby.TimeLimit.text = 5.ToString();
            BingoLobby.Difficulty.value = 2;
            BingoLobby.RequirePRank.isOn = false;
            BingoLobby.DisableCampaignAltExits.isOn = false;
            BingoLobby.GameVisibility.value = 0;
            
            BingoMapSelection.NumOfMapsTotal = 0;
            BingoMapSelection.UpdateNumber();
            BingoMapSelection.SelectedIds.Clear();
            
            if(BingoMapSelection.MapPoolButtons.Count > 0)
            {
                foreach(GameObject mapPoolButton in BingoMapSelection.MapPoolButtons)
                {
                    GetGameObjectChild(mapPoolButton,"Image").GetComponent<Image>().color = new Color(1,1,1,0);
                    mapPoolButton.GetComponent<MapPoolData>().mapPoolEnabled = false;
                }
            }
        }
        else
        {
            BingoLobby.MaxPlayers.text = CurrentGame.gameSettings.maxPlayers.ToString();
            BingoLobby.MaxTeams.text = CurrentGame.gameSettings.maxTeams.ToString();
            BingoLobby.TeamComposition.value = CurrentGame.gameSettings.teamComposition;
            BingoLobby.GridSize.value = CurrentGame.gameSettings.gridSize;
            BingoLobby.Gamemode.value = CurrentGame.gameSettings.gamemode;
            BingoLobby.TimeLimit.text = CurrentGame.gameSettings.timeLimit.ToString();
            BingoLobby.Difficulty.value = CurrentGame.gameSettings.difficulty;
            BingoLobby.RequirePRank.isOn = CurrentGame.gameSettings.requiresPRank;
            BingoLobby.DisableCampaignAltExits.isOn = CurrentGame.gameSettings.disableCampaignAltExits;
            BingoLobby.GameVisibility.value = CurrentGame.gameSettings.gameVisibility;
        }
        
        NetworkManager.setState(UltrakillBingoClient.State.INLOBBY);
        NetworkManager.RegisterConnection();
    }
    public static void ShowGameId(string password)
    {
        GetGameObjectChild(GetGameObjectChild(BingoLobby.RoomIdDisplay,"Title"),"Text").GetComponent<Text>().text = "Game ID: " + password;
    }
    
    public static void StartGame()
    {
        NetworkManager.SendStartGameSignal(CurrentGame.gameId);
    }
    
    public static void UpdateCards(int row, int column, string team, string playername, float newTime)
    {
        string coordLookup = row+"-"+column;
        List<string> dictKeys = CurrentGame.grid.levelTable.Keys.ToList();
        
        if(!CurrentGame.grid.levelTable.ContainsKey(coordLookup))
        {
            Logging.Error("RECEIVED AN INVALID GRID POSITION TO UPDATE!");
            Logging.Error(coordLookup);
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("A level was claimed by someone but an <color=orange>invalid grid position</color> was given.\nCheck BepInEx console and report it to Clearwater!");
            return;
        }

        try
        {
            CurrentGame.grid.levelTable[coordLookup].claimedBy = team;
            CurrentGame.grid.levelTable[coordLookup].timeToBeat = newTime;
        
            if(getSceneName() == "Main Menu")
            {
                GameObject bingoGrid = GetGameObjectChild(BingoEncapsulator.BingoCardScreen,"BingoGrid");
            
                GetGameObjectChild(bingoGrid,coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
                GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().isClaimed = true;
                GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedTeam = team;
                GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().claimedPlayer = playername;
                GetGameObjectChild(bingoGrid,coordLookup).GetComponent<BingoLevelData>().timeRequirement = newTime;
            }
            else
            {
                Color col;
                if(!BingoCardPauseMenu.teamColors.TryGetValue(team, out col))
                {
                    Logging.Error("Unable to get color, throwing exception");
                }
                else
                {
                    //Pause menu card
                    GetGameObjectChild(GetGameObjectChild(BingoCardPauseMenu.Root,"Card"),coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
                    
                    //In-game card
                    GetGameObjectChild(BingoCardPauseMenu.inGamePanel,coordLookup).GetComponent<Image>().color = BingoCardPauseMenu.teamColors[team];
                    BingoLevelData bld = GetGameObjectChild(GetGameObjectChild(BingoCardPauseMenu.Root,"Card"),coordLookup).GetComponent<BingoLevelData>();
                    
                    bld.isClaimed = true;
                    bld.claimedTeam = team;
                    bld.claimedPlayer = playername;
                    bld.timeRequirement = newTime;
                }
            }
        }
        catch (Exception e)
        {
            Logging.Error("THREW AN INVALID GRID POSITION TO UPDATE!");
            Logging.Error(e.ToString());
            Logging.Error(coordLookup);
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("A level was claimed by someone but the grid could not be updated.\nCheck BepInEx console and report it to Clearwater!");
        }
    }
    
    public static void RequestReroll(int row, int column)
    {
        if(GameManager.alreadyStartedVote)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("You have already started a vote in this game.");
        }
        else
        {
            RerollRequest rr = new RerollRequest();
            rr.gameId = CurrentGame.gameId;
            rr.steamId = Steamworks.SteamClient.SteamId.ToString();
            rr.row = row;
            rr.column = column;
            rr.steamTicket = NetworkManager.CreateRegisterTicket();
        
            NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(rr)); 
        }
        
        MonoSingleton<OptionsManager>.Instance.UnPause();
    }
    
    public static void PingMapForTeam(string team, int row, int column)
    {
        MapPing mp = new MapPing();
        mp.gameId = GameManager.CurrentGame.gameId;
        mp.team = team;
        mp.row = row;
        mp.column = column;
        mp.ticket = NetworkManager.CreateRegisterTicket();
        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(mp)); 
    }
}