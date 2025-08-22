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

    public static void onSettingUpdate(string settingType, int value)
    {
        switch (settingType)
        {
            case "maxPlayers":
            {
                GameManager.CurrentGame.gameSettingsArray["maxPlayers"] = Mathf.Clamp(Mathf.Max(value,GameManager.CurrentGame.currentPlayers.Count),2,16);
                MaxPlayers.text = Mathf.Clamp(value, Mathf.Max(value, GameManager.CurrentGame.currentPlayers.Count), 16f).ToString();
                break;
            }
            case "maxTeams":
            {
                GameManager.CurrentGame.gameSettingsArray["maxTeams"] = Mathf.Clamp(value,2,4);
                MaxTeams.text = Mathf.Clamp(value,2f,4f).ToString();
                break;
            }
            case "teamComposition":
            {
                SetTeams.SetActive(value == 1 && GameManager.PlayerIsHost());
                break;
            }
            case "timeLimit":
            {
                GameManager.CurrentGame.gameSettingsArray["timeLimit"] = Mathf.Clamp(value,5,30);
                TimeLimit.text = Mathf.Clamp(value,5,30).ToString();
                break;
            }
            default: break;
        }
        
        GameManager.CurrentGame.gameSettingsArray[settingType] = value;
        UIManager.HandleGameSettingsUpdate();
    }
    
    public static void updateFromNotification(UpdateRoomSettingsNotification newSettings)
    {
        MaxPlayers.text = newSettings.updatedSettings["maxPlayers"].ToString();
        MaxTeams.text = newSettings.updatedSettings["maxTeams"].ToString();
        TimeLimit.text = newSettings.updatedSettings["timeLimit"].ToString();
        Gamemode.value = newSettings.updatedSettings["gamemode"];
        TeamComposition.value = newSettings.updatedSettings["teamComposition"];
        RequirePRank.isOn = (newSettings.updatedSettings["requiresPRank"] == 1);
        Difficulty.value = newSettings.updatedSettings["difficulty"];
        GridSize.value = newSettings.updatedSettings["gridSize"];
        AllowRejoin.isOn = (newSettings.updatedSettings["allowRejoin"] == 1);
        DisableCampaignAltExits.isOn = (newSettings.updatedSettings["disableCampaignAltExits"] == 1);
        GameVisibility.value = newSettings.updatedSettings["gameVisibility"];

        GameManager.CurrentGame.gameSettingsArray = newSettings.updatedSettings;
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
        MaxPlayers.onEndEdit.AddListener(delegate { onSettingUpdate("maxPlayers",int.Parse(MaxPlayers.text)); });

        MaxTeams = GetGameObjectChild(GetGameObjectChild(GameOptions,"MaxTeams"),"Input").GetComponent<TMP_InputField>();
        MaxTeams.onEndEdit.AddListener(delegate { onSettingUpdate("maxTeams",int.Parse(MaxTeams.text)); });

        TimeLimit = GetGameObjectChild(GetGameObjectChild(GameOptions,"TimeLimit"),"Input").GetComponent<TMP_InputField>();
        TimeLimit.onEndEdit.AddListener(delegate { onSettingUpdate("timeLimit",int.Parse(TimeLimit.text)); });

        TeamComposition = GetGameObjectChild(GetGameObjectChild(GameOptions,"TeamComposition"),"Dropdown").GetComponent<TMP_Dropdown>();
        TeamComposition.onValueChanged.AddListener(delegate { onSettingUpdate("teamComposition",TeamComposition.value); });

        Gamemode = GetGameObjectChild(GetGameObjectChild(GameOptions,"Gamemode"),"Dropdown").GetComponent<TMP_Dropdown>();
        Gamemode.onValueChanged.AddListener(delegate { onSettingUpdate("gamemode",Gamemode.value); });

        GridSize = GetGameObjectChild(GetGameObjectChild(GameOptions,"GridSize"),"Dropdown").GetComponent<TMP_Dropdown>();
        GridSize.onValueChanged.AddListener(delegate { onSettingUpdate("gridSize",GridSize.value); });

        Difficulty = GetGameObjectChild(GetGameObjectChild(GameOptions,"Difficulty"),"Dropdown").GetComponent<TMP_Dropdown>();
        Difficulty.onValueChanged.AddListener(delegate { onSettingUpdate("difficulty",Difficulty.value); });

        RequirePRank = GetGameObjectChild(GetGameObjectChild(GameOptions,"RequirePRank"),"Input").GetComponent<Toggle>();
        RequirePRank.onValueChanged.AddListener(delegate { onSettingUpdate("requiresPRank",(RequirePRank.isOn ? 1 : 0)); });

        DisableCampaignAltExits = GetGameObjectChild(GetGameObjectChild(GameOptions,"DisableCampaignAltEnds"),"Input").GetComponent<Toggle>();
        DisableCampaignAltExits.onValueChanged.AddListener(delegate { onSettingUpdate("disableCampaignAltExits",(DisableCampaignAltExits.isOn ? 1 : 0)); });

        GameVisibility = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameVisibility"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameVisibility.onValueChanged.AddListener(delegate { onSettingUpdate("gameVisibility",GameVisibility.value); });

        AllowRejoin = GetGameObjectChild(GetGameObjectChild(GameOptions,"AllowRejoin"),"Input").GetComponent<Toggle>();
        AllowRejoin.onValueChanged.AddListener(delegate { onSettingUpdate("allowRejoin",(AllowRejoin.isOn ? 1 : 0)); });

        GameModifiers = GetGameObjectChild(GetGameObjectChild(GameOptions,"GameModifier"),"Dropdown").GetComponent<TMP_Dropdown>();
        GameModifiers.onValueChanged.AddListener(delegate { onSettingUpdate("gameModifier", GameModifiers.value);});

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