using System;
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
    
    public static bool wasVsyncActive = false;
    public static int fpsLimit = -1;
    
    public static List<String> nonWhitelistedMods = new List<string>();

    public static void HandleGameSettingsUpdate()
    {
        //Only send if we're the host.
        if(GameManager.PlayerIsHost())
        {
            UpdateRoomSettingsRequest urss = new UpdateRoomSettingsRequest();
            urss.roomId = GameManager.CurrentGame.gameId;
            urss.maxPlayers = int.Parse(BingoLobby.MaxPlayers.text);
            urss.maxTeams = int.Parse(BingoLobby.MaxTeams.text);
            urss.timeLimit = int.Parse(BingoLobby.TimeLimit.text);
            urss.gamemode = BingoLobby.Gamemode.value;
            urss.teamComposition = BingoLobby.TeamComposition.value;
            urss.PRankRequired = BingoLobby.RequirePRank.isOn;
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
        
        string text = "<color=orange>";
        
        foreach (string mod in UIManager.nonWhitelistedMods)
        {
           {text += mod + "\n";}
        }
        text += "</color>";
        mods.text = text;
    }
    
    public static void EnforceLimit()
    {
        wasVsyncActive = QualitySettings.vSyncCount == 1;
        fpsLimit = Application.targetFrameRate;
        
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }
    
    public static void RemoveLimit()
    {
        Application.targetFrameRate = fpsLimit;
        QualitySettings.vSyncCount = wasVsyncActive ? 1 : 0;
    }
    
    public static void Open()
    {
        if(!NetworkManager.modlistCheckDone)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Unable to contact server, please restart your game.\nIf this keeps happening, please check your internet and console for any errors.");
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
            
            //Enforce FPS and VSync lock to minimize crash/freezing from UI elements.
            EnforceLimit();
            
            //Hide chapter select
            ultrabingoButtonObject.transform.parent.gameObject.SetActive(false);
            
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.Root.SetActive(true);
            BingoEncapsulator.BingoMenu.SetActive(true);
            
            NetworkManager.setState(UltrakillBingoClient.State.INMENU);
        }
        else
        {
            //Show locked panel
            ultrabingoLockedPanel.SetActive(true);
        }
    }
}