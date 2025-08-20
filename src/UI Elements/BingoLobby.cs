using TMPro;
using UltraBINGO.NetworkMessages;
using UnityEngine;
using UnityEngine.UI;
using UltraBINGO.Components;
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
    public static GameObject CopyId;
    
    public static GameObject GameOptions;
    public static TMP_InputField MaxPlayers;
    public static TMP_InputField MaxTeams;
    public static TMP_InputField TimeLimit;
    public static TMP_Dropdown Gamemode;
    public static TMP_Dropdown TeamComposition;
    public static TMP_Dropdown GridSize;
    public static TMP_Dropdown Difficulty;
    public static Toggle RequirePRank;
    public static Toggle DisableCampaignAltExits;
    public static TMP_Dropdown GameVisibility;
    public static Toggle AllowRejoin;
    public static TMP_Dropdown GameModifiers;
    
    public static GameObject chatWindow;
    
    public static void onMaxPlayerUpdate(string playerAmount)
    {
        int amount = int.Parse(playerAmount);
        GameManager.CurrentGame.gameSettings.maxPlayers = Mathf.Clamp(Mathf.Max(amount,GameManager.CurrentGame.currentPlayers.Count),2,16);
        
        MaxPlayers.text = Mathf.Clamp(amount,Mathf.Max(amount,GameManager.CurrentGame.currentPlayers.Count),16f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onMaxTeamUpdate(string teamAmount)
    {
        int amount = int.Parse(teamAmount);
        GameManager.CurrentGame.gameSettings.maxTeams = Mathf.Clamp(amount,2,4);
        MaxTeams.text = Mathf.Clamp(amount,2f,4f).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onTimeLimitUpdate(string timeLimit)
    {
        int amount = int.Parse(timeLimit);
        GameManager.CurrentGame.gameSettings.timeLimit = Mathf.Clamp(amount,5,30);
        TimeLimit.text = Mathf.Clamp(amount,5,30).ToString();
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGamemodeTypeUpdate(int value)
    {
        GameManager.CurrentGame.gameSettings.gamemode = value;
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
    
    public static void onDisableCampaignAltExitsUpdate(bool value)
    {
        DisableCampaignAltExits.isOn = value;
        GameManager.CurrentGame.gameSettings.disableCampaignAltExits = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void onGameVisibilityUpdate(int value)
    {
        GameVisibility.value = value;
        GameManager.CurrentGame.gameSettings.gameVisibility = value;
        UIManager.HandleGameSettingsUpdate();
    }

    public static void onAllowRejoinUpdate(bool value)
    {
        AllowRejoin.isOn = value;
        GameManager.CurrentGame.gameSettings.allowRejoin = value;
        UIManager.HandleGameSettingsUpdate();
    }

    public static void onGameModifierUpdate(int value)
    {
        GameModifiers.value = value;
        GameManager.CurrentGame.gameSettings.gameModifier = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void updateFromNotification(UpdateRoomSettingsNotification newSettings)
    {
        MaxPlayers.text = newSettings.maxPlayers.ToString();
        MaxTeams.text = newSettings.maxTeams.ToString();
        TimeLimit.text = newSettings.timeLimit.ToString();
        Gamemode.value = newSettings.gamemode;
        TeamComposition.value = newSettings.teamComposition;
        RequirePRank.isOn = newSettings.PRankRequired;
        Difficulty.value = newSettings.difficulty;
        GridSize.value = newSettings.gridSize;
        DisableCampaignAltExits.isOn = newSettings.disableCampaignAltExits;
        GameVisibility.value = newSettings.gameVisibility;
        
        GameManager.CurrentGame.gameSettings.maxPlayers = newSettings.maxPlayers;
        GameManager.CurrentGame.gameSettings.maxTeams = newSettings.maxTeams;
        GameManager.CurrentGame.gameSettings.timeLimit = newSettings.timeLimit;
        GameManager.CurrentGame.gameSettings.gamemode = newSettings.gamemode;
        GameManager.CurrentGame.gameSettings.teamComposition = newSettings.teamComposition;
        GameManager.CurrentGame.gameSettings.requiresPRank = newSettings.PRankRequired;
        GameManager.CurrentGame.gameSettings.difficulty = newSettings.difficulty;
        GameManager.CurrentGame.gameSettings.gridSize = newSettings.gridSize;
        GameManager.CurrentGame.gameSettings.disableCampaignAltExits = newSettings.disableCampaignAltExits;
        GameManager.CurrentGame.gameSettings.gameVisibility = newSettings.gameVisibility;
    }
    
    public static void LockUI()
    {
        StartGame.GetComponent<Button>().interactable = false;
        ReturnToBingoMenu.GetComponent<Button>().interactable = false;
        SelectMaps.GetComponent<Button>().interactable = false;
    }
    
    public static void UnlockUI()
    {
        StartGame.GetComponent<Button>().interactable = true;
        ReturnToBingoMenu.GetComponent<Button>().interactable = true;
        SelectMaps.GetComponent<Button>().interactable = true;
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
            BingoEncapsulator.BingoMapSelection.SetActive(true);
            BingoMapBrowser.Setup();
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
                //Lock the button to prevent being able to spam it
                LockUI();
                GameManager.StartGame();
            }
        });
        
        //Room id text
        RoomIdDisplay = GetGameObjectChild(BingoLobby,"BingoGameID");
        
        //Copy ID
        CopyId = GetGameObjectChild(BingoLobby,"CopyID");
        CopyId.GetComponent<Button>().onClick.AddListener(delegate
        {
            GUIUtility.systemCopyBuffer = GetGameObjectChild(GetGameObjectChild(RoomIdDisplay,"Title"),"Text").GetComponent<Text>().text.Split(':')[1];
        });
        
        //Game options
        GameOptions = GetGameObjectChild(BingoLobby,"BingoGameSettings");

        MaxPlayers = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxPlayers"),"Input").GetComponent<TMP_InputField>();
        MaxPlayers.onEndEdit.AddListener(onMaxPlayerUpdate);
        
        MaxTeams = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxTeams"),"Input").GetComponent<TMP_InputField>();
        MaxTeams.onEndEdit.AddListener(onMaxTeamUpdate);
        
        TimeLimit = GetGameObjectChild(GetGameObjectChild(GameOptions,"TimeLimit"),"Input").GetComponent<TMP_InputField>();
        TimeLimit.onEndEdit.AddListener(onTimeLimitUpdate);
        
        TeamComposition = GetGameObjectChild(GetGameObjectChild(GameOptions,"TeamComposition"),"Dropdown").GetComponent<TMP_Dropdown>();
        TeamComposition.onValueChanged.AddListener(onTeamCompositionUpdate);
        
        Gamemode = GetGameObjectChild(GetGameObjectChild(GameOptions,"Gamemode"),"Dropdown").GetComponent<TMP_Dropdown>();
        Gamemode.onValueChanged.AddListener(onGamemodeTypeUpdate);
        
        GridSize = GetGameObjectChild(GetGameObjectChild(GameOptions,"GridSize"),"Dropdown").GetComponent<TMP_Dropdown>();
        GridSize.onValueChanged.AddListener(onGridSizeUpdate);
        
        Difficulty = GetGameObjectChild(GetGameObjectChild(GameOptions,"Difficulty"),"Dropdown").GetComponent<TMP_Dropdown>();
        Difficulty.onValueChanged.AddListener(onDifficultyUpdate);
        
        RequirePRank = GetGameObjectChild(GetGameObjectChild(GameOptions,"RequirePRank"),"Input").GetComponent<Toggle>();
        RequirePRank.onValueChanged.AddListener(onPRankRequiredUpdate);
        
        DisableCampaignAltExits = GetGameObjectChild(GetGameObjectChild(GameOptions,"DisableCampaignAltEnds"),"Input").GetComponent<Toggle>();
        DisableCampaignAltExits.onValueChanged.AddListener(onDisableCampaignAltExitsUpdate);
        
        GameVisibility = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameVisibility"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameVisibility.onValueChanged.AddListener(onGameVisibilityUpdate);
        
        AllowRejoin = GetGameObjectChild(GetGameObjectChild(GameOptions,"AllowRejoin"),"Input").GetComponent<Toggle>();
        AllowRejoin.onValueChanged.AddListener(onAllowRejoinUpdate);
        
        GameModifiers = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameModifier"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameModifiers.onValueChanged.AddListener(onGameModifierUpdate);
        
        if(chatWindow == null)
        {
            chatWindow = GameObject.Instantiate(AssetLoader.BingoChat,BingoLobby.transform);
            chatWindow.name = "BingoChat";
            chatWindow.AddComponent<BingoChatManager>();
            chatWindow.GetComponent<BingoChatManager>().Bind(chatWindow);
        }
        
        
        BingoLobby.SetActive(false);
    }
}