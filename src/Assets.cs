using System.IO;
using TMPro;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UltraBINGO;

public static class AssetLoader
{
    public static AssetBundle Assets;
    
    public static TMP_FontAsset GameFont;
    public static Font GameFontLegacy;
    
    public static Sprite UISprite;
    
    public static GameObject BingoEntryButton;
    public static GameObject BingoPauseCard;
    public static GameObject BingoMainMenu;
    public static GameObject BingoLobbyMenu;
    public static GameObject BingoCardElements;
    public static GameObject BingoEndScreen;
    public static GameObject BingoSetTeams;
    public static GameObject BingoCardButtonTemplate;
    public static GameObject BingoGameBrowser;
    public static GameObject BingoVotePanel;
    public static GameObject BingoDominationTimer;
    public static GameObject BingoChat;

    public static GameObject BingoMapSelection;
    
    public static GameObject BingoInGameGridPanel;
    public static GameObject BingoLockedPanel;
    public static GameObject BingoUnallowedModsPanel;
    
    public static AudioClip GameOverSound;
    
    public static void LoadAssets()
    {
        Assets = AssetBundle.LoadFromFile(Path.Combine(Main.ModFolder,"bingo.resource"));
        
        GameFont = Assets.LoadAsset<TMP_FontAsset>("VCR_OSD_MONO_EXTENDED_TMP");
        GameFontLegacy = Assets.LoadAsset<Font>("VCR_OSD_MONO_LEGACY");
        
        BingoEntryButton = Assets.LoadAsset<GameObject>("BingoEntryButton");
        BingoPauseCard = Assets.LoadAsset<GameObject>("BingoPauseCard");
        BingoMainMenu = Assets.LoadAsset<GameObject>("BingoMainMenu");
        BingoLobbyMenu = Assets.LoadAsset<GameObject>("BingoLobbyMenu");
        BingoCardElements = Assets.LoadAsset<GameObject>("BingoCard");
        BingoEndScreen = Assets.LoadAsset<GameObject>("BingoEndScreen");
        BingoSetTeams = Assets.LoadAsset<GameObject>("BingoSetTeams");
        BingoCardButtonTemplate = Assets.LoadAsset<GameObject>("BingoGridButtonTemplate");
        BingoGameBrowser = Assets.LoadAsset<GameObject>("BingoMatchBrowser");
        BingoLockedPanel = Assets.LoadAsset<GameObject>("BingoLocked");
        BingoUnallowedModsPanel = Assets.LoadAsset<GameObject>("BingoUnallowedMods");
        BingoInGameGridPanel = Assets.LoadAsset<GameObject>("BingoInGameGrid");
        BingoDominationTimer = Assets.LoadAsset<GameObject>("BingoDominationTimer");
        BingoChat = Assets.LoadAsset<GameObject>("BingoChat");
        BingoMapSelection = Assets.LoadAsset<GameObject>("BingoMapSelection"); //Full map selection
         
        UISprite = Assets.LoadAsset<Sprite>("UISprite");
        GameOverSound = Assets.LoadAsset<AudioClip>("gameEnd");
        
        BingoVotePanel = Assets.LoadAsset<GameObject>("BingoVotePanel");
    }
}