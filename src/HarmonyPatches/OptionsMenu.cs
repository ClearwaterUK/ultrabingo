using System;
using System.Linq;
using AngryLevelLoader.Containers;
using AngryLevelLoader.Managers;
using HarmonyLib;
using RudeLevelScript;
using SettingsMenu.Components;
using SettingsMenu.Models;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(SettingsPageBuilder),"BuildPage")]
public static class OptionsMenuPatcher
{
    [HarmonyPostfix]
    public static void DisableMajorAssistsOptions(ref SettingsPage settingsPage)
    {
        if(GameManager.IsInBingoLevel && !GameManager.CurrentGame.isGameFinished())
        {
            //Hide the PluginConfig button
            SettingsMenu.Components.SettingsMenu optionsMenu = MonoSingleton<OptionsMenuToManager>.Instance.optionsMenu;

            try
            {
                GetGameObjectChild(GetGameObjectChild(optionsMenu.gameObject,"Navigation Rail"),"PluginConfiguratorButton(Clone)").SetActive(false);
            }
            catch (Exception e)
            {
                Logging.Warn("Couldn't find PluginConfigurator button in options - Is it not working?");
                Logging.Error(e.ToString());
            }
            
            if(settingsPage.name == "Assist")
            {
                GameObject assistsList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(optionsMenu.gameObject,"Pages"),"Assist"),"Scroll Rect"),"Contents");
        
                GetGameObjectChild(assistsList,"-- Major Assists --").SetActive(false);
                GetGameObjectChild(assistsList,"Game Speed").SetActive(false);
                GetGameObjectChild(assistsList,"Damage Taken").SetActive(false);
                GetGameObjectChild(assistsList,"Boss Fight Difficulty Override").SetActive(false);
                GetGameObjectChild(assistsList,"Infinite Stamina").SetActive(false);
                GetGameObjectChild(assistsList,"Disable Whiplash Hard Damage").SetActive(false);
                GetGameObjectChild(assistsList,"Disable All Hard Damage").SetActive(false);
                GetGameObjectChild(assistsList,"Disable Weapon Freshness").SetActive(false);
                GetGameObjectChild(assistsList,"Disable Assist Popup").SetActive(false);
        
                GameObject disabledNotification = UIHelper.CreateText("Major assists are <color=orange>disabled</color> while playing Baphomet's Bingo.",26,"TextDisabled");
                disabledNotification.transform.SetParent(assistsList.transform);
            }
        }
    }
}