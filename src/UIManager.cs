﻿using System;
using System.Collections.Generic;
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

public static class UIManager
{
    public static GameObject ultrabingoButtonObject = null;
    public static GameObject ultrabingoEncapsulator = null;
    public static GameObject ultrabingoLockedPanel = null;
    public static GameObject ultrabingoUnallowedModsPanel = null;
    
    public static void HandleGameSettingsUpdate()
    {
        //Only send if we're the host.
        if(GameManager.PlayerIsHost())
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
            urss.disableCampaignAltExits = BingoLobby.DisableCampaignAltExits.isOn;
            urss.gameVisibility = BingoLobby.GameVisibility.value;
            urss.ticket = NetworkManager.CreateRegisterTicket();
        
            NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(urss));
        }
    }
    
    public static void SetupElements(CanvasController __instance)
    {
        RectTransform canvasRectTransform = __instance.GetComponent<RectTransform>();
        GameObject difficultySelectObject = canvasRectTransform.Find("Difficulty Select (1)").gameObject;
        
        if(ultrabingoButtonObject == null)
        {
            ultrabingoButtonObject = GameObject.Instantiate(AssetLoader.BingoEntryButton,difficultySelectObject.transform);
            ultrabingoButtonObject.name = "UltraBingoButton";
        }
        Button bingoButton = ultrabingoButtonObject.GetComponent<Button>();
        bingoButton.onClick.AddListener(delegate
        {
            Open();
        });
        if(ultrabingoEncapsulator == null)
        {
            ultrabingoEncapsulator = BingoEncapsulator.Init();
            ultrabingoEncapsulator.name = "UltraBingo";
            ultrabingoEncapsulator.transform.parent = __instance.transform;
            ultrabingoEncapsulator.transform.localPosition = Vector3.zero;
            ultrabingoEncapsulator.AddComponent<BingoMenuManager>();
        }
        ultrabingoEncapsulator.SetActive(false);
    }
    
    public static void PopulateUnallowedMods()
    {
        TextMeshProUGUI mods = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(ultrabingoUnallowedModsPanel,"BingoLockedPanel"),"Panel"),"ModList").GetComponent<TextMeshProUGUI>();
        
        List<string> whitelistedMods = new List<string>()
        {
            "PluginConfigurator","AngryLevelLoader","Baphomet's BINGO"
        };
        
        string text = "<color=orange>";
        
        foreach (string mod in Main.LoadedMods)
        {
            if(!whitelistedMods.Contains(mod)) {text += mod + "\n";}
        }
        text += "</color>";
        mods.text = text;
    }
    
    public static void Open()
    {
        if(!NetworkManager.modlistCheckDone)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Mod check failed, please restart your game.\nIf this keeps happening, please check your internet.");
            return;
        }
        if(!NetworkManager.modlistCheckPassed)
        {
            PopulateUnallowedMods();
            ultrabingoUnallowedModsPanel.SetActive(true);
            return;
        }
        if(Main.HasUnlocked)
        {
            if(NetworkManager.IsConnectionUp())
            {
                NetworkManager.DisconnectWebSocket();
                GameManager.ClearGameVariables();
            }
            //Hide chapter select
            ultrabingoButtonObject.transform.parent.gameObject.SetActive(false);
        
            //NetworkManager.analyseCatalog();
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.Root.SetActive(true);
            BingoEncapsulator.BingoMenu.SetActive(true);
        }
        else
        {
            //Show locked panel
            ultrabingoLockedPanel.SetActive(true);
        }
    }
}