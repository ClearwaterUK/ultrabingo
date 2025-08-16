using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UltraBINGO;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.SceneManagement;

using static UltraBINGO.CommonFunctions;

/*
 * Baphomet's Bingo
 *
 * Adds a bingo multiplayer gamemode.
 *
 * Created by Clearwater.
 * */

namespace UltrakillBingoClient
{
    [BepInPlugin(pluginId, pluginName, pluginVersion)]
    [BepInDependency("com.eternalUnion.angryLevelLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {   
        public const string pluginId = "clearwater.ultrakillbingo.ultrakillbingo";
        public const string pluginName = "Baphomet's BINGO";
        public const string pluginVersion = "1.1.1";
        
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        public static bool IsDevelopmentBuild = true;
        public static bool IsSteamAuthenticated = false;
        public static bool HasUnlocked = true;
        public static bool UpdateAvailable = false;
        
        public static List<string> LoadedMods = new List<string>();
        
        private static readonly ConcurrentQueue<Action> actions = new();
        public static void Queue(Action action) => actions.Enqueue(action);

        //Mod init logic
        private void Awake()
        {
            Logging.Message("--Now loading Baphomet's Bingo...--");
            Debug.unityLogger.filterLogType = LogType.Warning;

            Logging.Message("--Loading asset bundle...--");
            AssetLoader.LoadAssets();
            
            Logging.Message("--Applying patches...--");
            Harmony harmony = new Harmony(pluginId);
            harmony.PatchAll();
             
            Logging.Message("--Network manager init...--");
            NetworkManager.serverURLConfig = Config.Bind("ServerConfig","serverUrl","clearwaterbirb.uk","Server URL");
            NetworkManager.serverPortConfig = Config.Bind("ServerConfig","serverPort","2052","Server Port");
            NetworkManager.lastRankUsedConfig = Config.Bind("ServerConfig","lastRankUsed","None","Last Rank Used (Only works if your SteamID has access to this rank)");

            if (!Directory.Exists(Path.Combine(ModFolder, "ThumbnailCache")))
            {
                Logging.Message("--Thumbnail cache folder doesn't exist, creating now---");
                Directory.CreateDirectory(Path.Combine(ModFolder, "ThumbnailCache"));
            }
            
            string url = NetworkManager.serverURLConfig.Value;
            string port = NetworkManager.serverPortConfig.Value;
            NetworkManager.Initialise(url,port,IsDevelopmentBuild);
            
            Logging.Message("--Done!--");
            SceneManager.sceneLoaded += onSceneLoaded;
        }
        
        void Update()
        {
            while (actions.TryDequeue(out var action))
                action();
        }
        
        //Make sure the client is running a legit copy of the game
        public bool Authenticate()
        {
            Logging.Message("Authenticating game ownership with Steam...");
            try
            {
                AuthTicket ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
                string ticketString = BitConverter.ToString(ticket.Data,0, ticket.Data.Length).Replace("-", string.Empty);
                if(ticketString.Length > 0)
                {
                    IsSteamAuthenticated = true;
                    NetworkManager.SetSteamTicket(ticketString);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.Error("Unable to authenticate with Steam!");
                return false;
            }
            return false;
        }
        
        public void VerifyModWhitelist()
        {
            Logging.Message("Validating current modlist...");
            foreach (var plugin in Chainloader.PluginInfos)
            {
                List<string> modData = plugin.Value.ToString().Split(' ').ToList();
                modData.RemoveAt(modData.Count-1);
                string modName = string.Join(" ",modData);
                LoadedMods.Add(modName);
            }
            
            VerifyModRequest vmr = new VerifyModRequest(LoadedMods, SteamClient.SteamId.ToString());
            NetworkManager.SendModCheck(vmr);
        }
        
        
        //Scene switch
        public void onSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameManager.ResetVars();

            if(getSceneName() == "Main Menu")
            {
                HasUnlocked = hasUnlockedMod();
                if(!IsSteamAuthenticated)
                {
                    Authenticate();
                    VerifyModWhitelist();
                }
                
                if(GameManager.CurrentGame != null && GameManager.CurrentGame.isGameFinished())
                {
                    BingoEnd.ShowEndScreen();
                    MonoSingleton<AssistController>.Instance.majorEnabled = false;
                    MonoSingleton<AssistController>.Instance.gameSpeed = 1f;
                }
                
                UIManager.ultrabingoLockedPanel = GameObject.Instantiate(AssetLoader.BingoLockedPanel,GetGameObjectChild(GetInactiveRootObject("Canvas"),"Difficulty Select (1)").transform);
                UIManager.ultrabingoUnallowedModsPanel = GameObject.Instantiate(AssetLoader.BingoUnallowedModsPanel,GetGameObjectChild(GetInactiveRootObject("Canvas"),"Difficulty Select (1)").transform);
                GetGameObjectChild(BingoMainMenu.VersionInfo,"VersionNum").GetComponent<TextMeshProUGUI>().text = pluginVersion;
                UIManager.ultrabingoLockedPanel.SetActive(false);
                UIManager.ultrabingoUnallowedModsPanel.SetActive(false);
            }
            else
            {
                UIManager.RemoveLimit();
                if(GameManager.IsInBingoLevel)
                {
                    if(GameManager.CurrentGame.gameSettings.disableCampaignAltExits)
                    {
                        CampaignPatches.Apply(getSceneName());
                    }
                }
            }
        }
    }
}
