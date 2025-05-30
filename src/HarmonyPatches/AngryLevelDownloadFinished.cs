﻿using System;
using AngryLevelLoader.Containers;
using HarmonyLib;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

namespace UltraBINGO.HarmonyPatches;

[HarmonyPatch(typeof(AngryBundleContainer),"UpdateFinalRankUI")]
public static class Patch
{
	[HarmonyPostfix]
	public static void notifyDownloadComplete(AngryBundleContainer __instance)
	{
		if(GameManager.CurrentGame != null && !GameManager.EnteringAngryLevel)
		{
			Logging.Info("Download of " + BingoMenuController.currentlyDownloadingLevel + " finished");
			GameManager.IsDownloadingLevel = false;
			
			MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("<color=orange>"+BingoMenuController.currentlyDownloadingLevel + "</color> has finished downloading.\nClick level button again to enter!");
			
			BingoMenuController.currentlyDownloadingLevel = "";
		}
	}
}