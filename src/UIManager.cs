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
    public static GameObject ultrabingoErrorMessagePanel = null;
    
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
            urss.allowRejoin = BingoLobby.AllowRejoin.isOn;
            urss.gameModifier = BingoLobby.GameModifiers.value;
            
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
    
    public static string PopulateUnallowedMods()
    {
        string text = "<color=orange>";
        
        foreach (string mod in nonWhitelistedMods)
        {
           {text += mod + "\n";}
        }
        text += "</color>";
        //mods.text = text;

        return text;
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

    public static void showErrorMessage(string headerText, string paragraphText="", string rowText = "")
    {
        GameObject ErrorPanel = GetGameObjectChild(GetGameObjectChild(ultrabingoErrorMessagePanel, "BingoLockedPanel"),
                "Panel");

        TextMeshProUGUI header = GetGameObjectChild(ErrorPanel, "Text").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI content = GetGameObjectChild(ErrorPanel, "Text (1)").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI modList = GetGameObjectChild(ErrorPanel, "ModList").GetComponent<TextMeshProUGUI>();;

        header.text = "<color=orange>"+headerText+"</color>";
        content.text = paragraphText;
        modList.text = rowText;
        
        ultrabingoErrorMessagePanel.SetActive(true);
    }
    
    public static void Open()
    {
        if(!Main.IsSteamAuthenticated)
        {
            showErrorMessage("Avast matey",
                "Unable to authenticate with Steam.\n\nYou must be connected to the Steam servers (not running in offline mode), and own a legal copy of ULTRAKILL to play Baphomet's Bingo.");
            return;
        }
        if (!NetworkManager.startupDone)
        {
            showErrorMessage("Unable to contact server",
                "ULTRAKILL failed to establish a connection to the Baphomet's Bingo server. Please restart your game to try again.\nIf this keeps happening, please check your internet connection & console for any errors.\n(<color=red>" + NetworkManager.lastErrorString + "</color>)");
            return;
        }
        if(!NetworkManager.modlistCheckPassed)
        {
            string unallowedMods = PopulateUnallowedMods();
            showErrorMessage("Non-whitelisted mods detected",
                "To ensure fair gameplay between players, please <color=orange>disable the following mods</color> and <color=orange>restart</color> to play Baphomet's Bingo:",
                unallowedMods);
            
            return;
        }

        if (Main.UpdateAvailable)
        {
            showErrorMessage("UPDATE AVAILABLE",
                "<color=orange>An update is available! Please update your mod to play Baphomet's Bingo.</color>");
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