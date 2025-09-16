using System.Collections.Generic;
using System.Threading.Tasks;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Fields;
using AngryLevelLoader.Managers;
using AngryLevelLoader.Notifications;
using TMPro;
using UltraBINGO.Components;
using UltrakillBingoClient;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoMenuController
{
    public static string currentlyDownloadingLevel = "";

    public static bool checkSteamAuthentication()
    {
        if(Main.UpdateAvailable)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>An update is available! Please update your mod to play Baphomet's Bingo.</color>");
            return false;
        }
        if(!Main.IsSteamAuthenticated)
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Unable to authenticate with Steam.\nYou must be connected to the Steam servers, and own a legal copy of ULTRAKILL to play Baphomet's Bingo.");
            return false;
        }
        return Main.IsSteamAuthenticated;
    }
    
    public static void LoadBingoLevel(string levelName, string levelCoords, BingoLevelData levelData)
    {
        //Make sure the game hasn't ended.
        if(GameManager.CurrentGame.isGameFinished())
        {
            return;
        }
        
        if(!GameManager.CurrentGame.isGameFinished())
        {
            //Force disable cheats and major assists, set difficulty to difficulty of the game set by the host.
            MonoSingleton<PrefsManager>.Instance.SetBool("majorAssist", false);
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty",
                GameManager.CurrentGame.gameSettingsArray["difficulty"]);
            int row = int.Parse(levelCoords[0].ToString());
            int column = int.Parse(levelCoords[2].ToString());
            GameManager.IsInBingoLevel = true;
        
            //Check if the level we're going into is campaign or Angry.
            //If it's Angry, we need to do some checks if the level is downloaded before going in.
            if(levelData.isAngryLevel)
            {
                handleAngryLoad(levelData,row,column);
            }
            else
            {
                handleCampaignLoad(levelName,row,column);
            }
        }
    }
    
    public static ScriptManager.LoadScriptResult loadAngryScript(string scriptName)
    {
        return ScriptManager.AttemptLoadScriptWithCertificate(scriptName);
    }
    
    public static async Task<bool> DownloadAngryScript(string scriptName)
    {
        ScriptUpdateNotification.ScriptUpdateProgressField field = new ScriptUpdateNotification.ScriptUpdateProgressField();
        field.scriptName = scriptName;
        field.scriptStatus = ScriptUpdateNotification.ScriptUpdateProgressField.ScriptStatus.Download;
        field.StartDownload();
        while(field.downloading)
        {
            await Task.Delay(500);
        }
        if(field.isDone)
        {
            Logging.Warn("Download finished");
            return true;
        }
        else
        {
            Logging.Error("Download failed!");
            return false;
        }
    }

    public static void handleCampaignLoad(string campaignLevelName, int row=0, int column=0)
    {
        if (!GameManager.CurrentGame.isGameFinished())
        {
            NetworkManager.setState(UltrakillBingoClient.State.INGAME);
            GameManager.UpdateGridPosition(row,column);
            SceneHelper.LoadScene(campaignLevelName);
        }

    }
    
    public static async void handleAngryLoad(BingoLevelData angryLevelData,int row=0, int column=0)
    {
            //Make sure the game hasn't ended.
            if(GameManager.CurrentGame.isGameFinished())
            {
                return;
            }
        
            //Prevent changing levels while downloading to avoid problems.
            if(GameManager.IsDownloadingLevel == true)
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Please wait for the current download to complete before switching to a different level.");
                return;
            }
        
            //First check if the level exists locally, and is up to date.
            bool isLevelReady = OnlineLevelsManager.onlineLevels[angryLevelData.angryParentBundle]._status == OnlineLevelField.OnlineLevelStatus.installed;
            
            //If already downloaded and up to date, load the bundle.
            if(isLevelReady)
            {
                Dictionary<string,AngryBundleContainer> locallyDownloadedLevels = AngryLevelLoader.Plugin.angryBundles;
                AngryBundleContainer bundleContainer = locallyDownloadedLevels[angryLevelData.angryParentBundle];
                
                Logging.Message("Loading Angry level");
                //Need to (re)load the bundle before accessing it to make sure the level fields are accessible.
                GameManager.EnteringAngryLevel = true;
                
                //If the level we're trying to load is in the same bundle...
                if (AngrySceneManager.currentBundleContainer != null)
                {
                    if (angryLevelData.angryParentBundle != AngrySceneManager.currentBundleContainer.bundleData.bundleGuid)
                    {
                        await bundleContainer.UpdateScenes(true,false);
                        await Task.Delay(250);
                    }
                }
                else
                {
                    await bundleContainer.UpdateScenes(true,false);
                    await Task.Delay(250);
                }
                
                //Make sure the given angry level ID exists inside the bundle...
                Dictionary<string,LevelContainer> levelsInBundle = bundleContainer.levels;
                bool containsLevel = levelsInBundle.TryGetValue(angryLevelData.angryLevelId, out LevelContainer customLevel);
                if(containsLevel)
                {
                    //...if it does, gather all the necessary data and ask Angry to load it.
                    string msg = (getSceneName() != "Main Menu" ? "MOVING TO <color=orange>" + angryLevelData.levelName + "</color>..." : "LOADING <color=orange>" + angryLevelData.levelName + "</color>...");
                    
                    if(getSceneName() == "Main Menu")
                    {
                        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
                    }
                    else
                    {
                        BingoCardPauseMenu.DescriptorText.GetComponent<TextMeshProUGUI>().text = msg;
                    }
                    await Task.Delay(1000);
                    NetworkManager.setState(UltrakillBingoClient.State.INGAME);
                    AngryLevelLoader.Plugin.selectedDifficulty = GameManager.CurrentGame.gameSettingsArray["difficulty"];

                    //Before loading, check if the level uses any custom scripts.
                    List<string> requiredAngryScripts = ScriptManager.GetRequiredScriptsFromBundle(bundleContainer);
                    if(requiredAngryScripts.Count > 0)
                    {
                        Logging.Message("Level requires custom scripts, checking if locally loaded");
                        
                        //If they do, check if the scripts are already downloaded and loaded locally.
                        foreach(string scriptName in requiredAngryScripts)
                        {
                            if(!ScriptManager.ScriptExists(scriptName))
                            {
                                Logging.Message("Asking Angry to download " + scriptName);
                                 bool downloadResult = await DownloadAngryScript(scriptName);
                                 if(downloadResult == true)
                                 {
                                     ScriptManager.LoadScriptResult res = loadAngryScript(scriptName);
                                     if(res != ScriptManager.LoadScriptResult.Loaded)
                                     {
                                         Logging.Error("Failed to load script with reason: ");
                                         Logging.Error(res.ToString());
                                     }
                                 }
                            }
                            else
                            {
                                Logging.Message(scriptName + " is already downloaded");
                            }
                        }
                        if(!GameManager.CurrentGame.isGameFinished())
                        {
                            GameManager.IsSwitchingLevels = true;
                            AngryLevelLoader.Plugin.difficultyField.gamemodeListValueIndex =
                                GameManager.CurrentGame.gameSettingsArray["gameModifier"];
                            
                            GameManager.UpdateGridPosition(row,column);
                            AngrySceneManager.LoadLevelWithScripts(requiredAngryScripts,bundleContainer,customLevel,customLevel.data,customLevel.data.scenePath);
                        }
                    }
                    else
                    {   
                        if(!GameManager.CurrentGame.isGameFinished())
                        {
                            GameManager.IsSwitchingLevels = true;
                            AngryLevelLoader.Plugin.difficultyField.gamemodeListValueIndex =
                                GameManager.CurrentGame.gameSettingsArray["gameModifier"];
                            GameManager.UpdateGridPosition(row,column);
                            AngrySceneManager.LoadLevel(bundleContainer,customLevel,customLevel.data,customLevel.data.scenePath,true);
                        }
                    }
                }
                else
                {
                    Logging.Error("Given level ID does not exist inside the bundle!");
                    Logging.Error("Given level ID: " + angryLevelData.angryLevelId);
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>Failed to load level, something went wrong.</color>");
                }
            }
            else
            {
                //Prevent multiple downloads.
                if(GameManager.IsDownloadingLevel)
                {
                    Logging.Warn("Trying to download a level but another one is already in progress!");
                    return;
                }
                
                //If level does not already exist locally, get Angry to download it first.
                Logging.Message("Level does not already exist locally - Downloading from online repo");
                
                if(getSceneName() != "Main Menu") {MonoSingleton<OptionsManager>.Instance.UnPause();}
                GameManager.IsDownloadingLevel = true;
                
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("-- DOWNLOADING "+ angryLevelData.levelName + " --\nYou can continue to play in the meantime.");
                currentlyDownloadingLevel = angryLevelData.levelName;
                OnlineLevelsManager.onlineLevels[angryLevelData.angryParentBundle].Download();
                while(OnlineLevelsManager.onlineLevels[angryLevelData.angryParentBundle].downloading)
                {
                    await Task.Delay(500);
                }
            }
    }
    
    public static async void LoadBingoLevelFromPauseMenu(string levelCoords, BingoLevelData levelData)
    {
        //Make sure the game hasn't ended or we're not already loading a level.
        if(GameManager.CurrentGame.isGameFinished() || GameManager.IsSwitchingLevels)
        {
            return;
        }
                
        //Prevent loading the level we're already on.
        if(levelData.angryLevelId == getSceneName())
        {
            Logging.Warn("Trying to load level we're already in");
            return;
        }
        
        if(!GameManager.CurrentGame.isGameFinished())
        {
            int row = int.Parse(levelCoords[0].ToString());
            int column = int.Parse(levelCoords[2].ToString());
            
            string levelDisplayName = GameManager.CurrentGame.grid.levelTable[levelCoords].levelName;
            string levelId = GameManager.CurrentGame.grid.levelTable[levelCoords].levelId;
        
            //Save vote data if a vote is ongoing, so the panel can reappear after scene switch.
            GameManager.voteData = MonoSingleton<BingoVoteManager>.Instance.voteOngoing ? new VoteData(true,MonoSingleton<BingoVoteManager>.Instance.hasVoted,MonoSingleton<BingoVoteManager>.Instance.voteThreshold,MonoSingleton<BingoVoteManager>.Instance.currentVotes,MonoSingleton<BingoVoteManager>.Instance.map,MonoSingleton<BingoVoteManager>.Instance.timeRemaining) : new VoteData(false);
            
            if(levelData.isAngryLevel)
            {
                handleAngryLoad(levelData,row,column);
            }
            else
            {
                string msg = "MOVING TO <color=orange>" + levelDisplayName + "</color>...";
                BingoCardPauseMenu.DescriptorText.GetComponent<TextMeshProUGUI>().text = msg;
                GameManager.IsSwitchingLevels = true;
                
                await Task.Delay(1000);
                handleCampaignLoad(levelId,row,column);
            }
        }
    }
    
    public static void ReturnToMenu()
    {
        UIManager.RemoveLimit();
        BingoEncapsulator.Root.SetActive(false);
        GetGameObjectChild(GetInactiveRootObject("Canvas"),"Difficulty Select (1)").SetActive(true);
        NetworkManager.setState(UltrakillBingoClient.State.NORMAL);
    }
    
    public static void CreateRoom()
    {
        if(!checkSteamAuthentication())
        {
            return;
        }
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Creating room...");
        NetworkManager.pendingAction = AsyncAction.Host;
        NetworkManager.ConnectWebSocket();
    }
    
    public static void JoinRoom(string roomPassword)
    {
        if(!checkSteamAuthentication())
        {
            return;
        }
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Joining room...");
        NetworkManager.pendingAction = AsyncAction.Join;
        NetworkManager.pendingPassword = roomPassword;
        NetworkManager.ConnectWebSocket();
    }
    
    public static void StartGame(int gameType, bool isJoiningMidGame=false)
    {
        GameManager.SetupBingoCardDynamic();
        
        string startMessage = (isJoiningMidGame ? "Joined game in progress." : "The game has begun!");
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(startMessage);
        
        if(GameManager.CurrentGame.gameSettingsArray["gamemode"] == 1)
        {
            GameObject canvas = GetInactiveRootObject("Canvas");
            canvas.AddComponent<DominationTimeManager>();
        }
        GameManager.MoveToCard(gameType);
    }
}