﻿
using System;
using System.IO;
using System.Reflection;
using AngryLevelLoader.Fields;
using BepInEx;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UltraBINGO;
using UltraBINGO.UI_Elements;
using UnityEngine;
using UnityEngine.SceneManagement;
using LogType = UnityEngine.LogType;

using WebSocketSharp;

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
    [BepInPlugin(Main.pluginId, Main.pluginName, Main.pluginVersion)]
    [BepInDependency("com.eternalUnion.angryLevelLoader", BepInDependency.DependencyFlags.HardDependency)]
    public class Main : BaseUnityPlugin
    {   
        public const string pluginId = "clearwater.ultrakillbingo.ultrakillbingo";
        public const string pluginName = "Baphomet's BINGO";
        public const string pluginVersion = "0.1.0";
        
        public static bool IsDevelopmentBuild = true;
        
        public static bool isSteamAuthenticated = false;
        
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        private void Awake()
        {
            // Plugin startup logic
            //Debug.unityLogger.filterLogType = LogType.Exception;
            
            Logging.Message("--Now loading Baphomet's Bingo...--");
            if(Main.IsDevelopmentBuild)
            {
                Logging.Warn("-- DEVELOPMENT BUILD. REQUESTS WILL BE SENT TO LOCALHOST. --");
            }
            else
            {
                Logging.Warn("-- RELEASE BUILD. REQUESTS WILL BE SENT TO REMOTE SERVER. --");
            }
            
            Logging.Message("--Loading assetbundle...--");
            AssetLoader.LoadAssets();
            
            Harmony harmony = new Harmony(pluginId);
            harmony.PatchAll();
            
            SceneManager.sceneLoaded += onSceneLoaded;
            
            NetworkManager.initialise();
        }
        
        public bool Authenticate()
        {
            Logging.Message("Authenticating game ownership with Steam...");
            try
            {
                AuthTicket ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
                string ticketString = BitConverter.ToString(ticket.Data,0, ticket.Data.Length).Replace("-", string.Empty);
                if(ticketString.Length > 0)
                {
                    isSteamAuthenticated = true;
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
        
        public void onSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameManager.hasSent = false;
            GameManager.enteringAngryLevel = false;
            GameManager.triedToActivateCheats = false;
            
            if(getSceneName() == "Main Menu")
            {
                if(!isSteamAuthenticated)
                {
                    Authenticate();
                }
                
                if(GameManager.CurrentGame != null && GameManager.CurrentGame.isGameFinished())
                {
                    BingoEnd.ShowEndScreen();
                    MonoSingleton<AssistController>.Instance.majorEnabled = false;
                    MonoSingleton<AssistController>.Instance.gameSpeed = 1f;
                }
            }
            else
            {
                if(GameManager.isInBingoLevel)
                {
                    UIManager.DisableMajorAssists(GetInactiveRootObject("Canvas"));
                }
            }
        }
    }
}