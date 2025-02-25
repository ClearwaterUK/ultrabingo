﻿using System;
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
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
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
        public const string pluginVersion = "1.0.3";
        
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        public static bool IsDevelopmentBuild = false;
        public static bool IsSteamAuthenticated = false;
        public static bool HasUnlocked = true;
        public static bool UpdateAvailable = false;
        
        public static List<String> missingMaps = new List<string>();
        public static List<string> LoadedMods = new List<string>();

        public static string CatalogFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"bingocatalog.toml");
        
        string allAssets = "";
        bool ran = false;
        int index = 0;
        int totalAssets = 0;
        
        //Mod init logic
        private void Awake()
        {
            Logging.Warn("--Now loading Baphomet's Bingo...--");
            
            Logging.Message("--Loading asset bundle...--");
            AssetLoader.LoadAssets();
            
            Logging.Message("--Applying patches...--");
            Harmony harmony = new Harmony(pluginId);
            harmony.PatchAll();
             
            Logging.Message("--Network manager init...--");
            NetworkManager.serverURLConfig = Config.Bind("ServerConfig","serverUrl","clearwaterbirb.uk","Server URL");
            NetworkManager.serverPortConfig = Config.Bind("ServerConfig","serverPort","2052","Server Port");
            
            string url = NetworkManager.serverURLConfig.Value;
            string port = NetworkManager.serverPortConfig.Value;
            NetworkManager.Initialise(url,port,IsDevelopmentBuild);
            
            Logging.Message("--Done!--");
            SceneManager.sceneLoaded += onSceneLoaded;
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
                if(GameManager.IsInBingoLevel)
                {
                    //UIManager.DisableMajorAssists();
                    UIManager.HideAngryButton();
                    if(GameManager.CurrentGame.gameSettings.disableCampaignAltExits)
                    {
                        CampaignPatches.Apply(getSceneName());
                    }
                }
            }
        }
    }
}
