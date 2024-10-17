﻿using System.IO;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UltraBINGO;

public static class AssetLoader
{
    public static TMP_FontAsset gameFont;
    public static Font gameFontLegacy;
    
    public static Sprite UISprite;
    
    public static GameObject BingoEntryButton;
    public static GameObject BingoPauseCard;
    public static GameObject BingoTeammatesCard;
    
    public static GameObject BingoMainMenu;
    public static GameObject BingoLobbyMenu;
    public static GameObject BingoCardElements;
    
    public static AudioClip GameOverSound;
    
    public static void LoadAssets()
    {
        AssetBundle assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModFolder,"bingo.resource"));
        gameFont = assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        gameFontLegacy = assets.LoadAsset<Font>("VCR_OSD_MONO_LEGACY");
        UISprite = assets.LoadAsset<Sprite>("UISprite");
        BingoEntryButton = assets.LoadAsset<GameObject>("BingoEntryButton");
        BingoPauseCard = assets.LoadAsset<GameObject>("BingoPauseCard");
        BingoTeammatesCard = assets.LoadAsset<GameObject>("BingoTeammateCard");
        
        BingoMainMenu = assets.LoadAsset<GameObject>("BingoMainMenu");
        BingoLobbyMenu = assets.LoadAsset<GameObject>("BingoLobbyMenu");
        BingoCardElements = assets.LoadAsset<GameObject>("BingoCard");
         
        GameOverSound = Addressables.LoadAssetAsync<AudioClip>("Assets/Music/Hits/Versus2Outro.wav").WaitForCompletion();

    }
}