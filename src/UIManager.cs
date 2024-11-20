﻿using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class UIManager
{
    public static GameObject ultrabingoButtonObject = null;
    public static GameObject ultrabingoEncapsulator = null;
    
    public static GameObject ultrabingoLockedPanel = null;
    
    public static void HandleGameSettingsUpdate()
    {
        //Only send if we're the host.
        if(GameManager.playerIsHost())
        {
            UpdateRoomSettingsRequest urss = new UpdateRoomSettingsRequest();
            urss.roomId = GameManager.CurrentGame.gameId;
            urss.maxPlayers = int.Parse(BingoLobby.MaxPlayers.text);
            urss.maxTeams = int.Parse(BingoLobby.MaxTeams.text);
            urss.teamComposition = BingoLobby.TeamComposition.value;
            urss.PRankRequired = BingoLobby.RequirePRank.isOn;
            urss.gameType = BingoLobby.GameType.value;
            urss.difficulty = BingoLobby.Difficulty.value;
            urss.gridSize = BingoLobby.GridSize.value;
        
            NetworkManager.sendEncodedMessage(JsonConvert.SerializeObject(urss));
        }

    }
    
    public static void Open()
    {
        if(Main.hasUnlocked)
        {
            //Hide chapter select
            ultrabingoButtonObject.transform.parent.gameObject.SetActive(false);
        
            NetworkManager.analyseCatalog();
            BingoEncapsulator.Root.SetActive(true);
            BingoEncapsulator.BingoMenu.SetActive(true);
        }
        else
        {
            //Show locked panel
            ultrabingoLockedPanel.SetActive(true);
        }

    }
    
    public static void Close()
    {
        //Show chapter select
        ultrabingoEncapsulator.SetActive(false);
        ultrabingoButtonObject.transform.parent.gameObject.SetActive(true);
    }
    
    public static void SetupElements(CanvasController __instance)
    {
        RectTransform canvasRectTransform = __instance.GetComponent<RectTransform>();
        GameObject chapterSelectObject = canvasRectTransform.Find("Chapter Select").gameObject;
        GameObject difficultySelectObject = canvasRectTransform.Find("Difficulty Select (1)").gameObject;
        if (chapterSelectObject == null)
        {
            Logging.Error("Chapter Select object is null");
            return;
        }
        
        if(ultrabingoButtonObject == null)
        {
            ultrabingoButtonObject = GameObject.Instantiate(AssetLoader.BingoEntryButton,difficultySelectObject.transform);
            ultrabingoButtonObject.name = "UltraBingoButton";
        }
        Button bingoButton = ultrabingoButtonObject.GetComponent<Button>();
        bingoButton.onClick.AddListener(Open);
        if(ultrabingoEncapsulator == null)
        {
            ultrabingoEncapsulator = BingoEncapsulator.Init();
            ultrabingoEncapsulator.name = "UltraBingo";
            ultrabingoEncapsulator.transform.parent = __instance.transform;
            ultrabingoEncapsulator.transform.localPosition = Vector3.zero;
        }
        ultrabingoEncapsulator.SetActive(false);
    }
    
    public static void DisableMajorAssists(GameObject canvas)
    { 
        GameObject optionsMenu = MonoSingleton<OptionsMenuToManager>.Instance.optionsMenu;
        GameObject assistsList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu,"Assist Options"),"Scroll Rect"),"Contents");
        GetGameObjectChild(assistsList,"Text (6)").SetActive(false);
        GetGameObjectChild(assistsList,"Major Assists").SetActive(false);
        
        GameObject disabledNotification = UIHelper.CreateText("Major assists are <color=orange>disabled</color> while playing Baphomet's Bingo.",26,"TextDisabled");
        disabledNotification.transform.SetParent(assistsList.transform);
    }
}