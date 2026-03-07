using System.Collections.Generic;
using System.IO;
using TMPro;
using UltraBINGO.Components;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoCardPauseMenu
{
    public static Dictionary<string,Color> teamColors = new Dictionary<string, Color>()
    {
        {"NONE",new Color(1,1 ,1,1)},
        {"Red",new Color(1,0,0,1)},
        {"Green",new Color(0,1,0,1)},
        {"Blue",new Color(0,0,1,1)},
        {"Yellow",new Color(1,1,0,1)},
    };
    
    public static GameObject Root;
    public static GameObject Grid;
    
    public static GameObject inGamePanel;
    
    public static GameObject DescriptorText;
    
    public static Outline pingedMap = null;
    
    public static void onMouseEnterLevelSquare(PointerEventData data)
    {
        string fallbackThumbnailPath = "assets/bingo/lvlimg/unknown.png";
        
        string angryLevelName = data.pointerEnter.gameObject.GetComponent<BingoLevelData>().levelName.ToLower();
        string campaignLevelName = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name].levelId.ToLower();
        
        Sprite levelSprite = null;

        //If using hide level names setting, use default thumbnail
        if (GameManager.CurrentGame.gameSettingsArray["hideLevelNames"] == 1)
        {
            Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(fallbackThumbnailPath);    
            levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            switch (data.pointerEnter.gameObject.GetComponent<BingoLevelData>().bingoLevelType)
            {
                case (BingoLevelType.Campaign):
                {
                    string path = "assets/bingo/lvlimg/campaign/" + campaignLevelName + ".png";
            
                    if(!AssetLoader.Assets.Contains(path))
                    {
                        path = fallbackThumbnailPath;
                    }
        
                    Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);    
                    levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
                    break;
                }
                case (BingoLevelType.Angry):
                {
                    string angryBundleId = data.pointerEnter.gameObject.GetComponent<BingoLevelData>().angryParentBundle;
                    string path = Path.Combine(Main.ModFolder, "ThumbnailCache", (angryBundleId + ".png"));
                    
                    if (File.Exists(path))
                    {
                        byte[] fileData = File.ReadAllBytes(Path.Combine(Main.ModFolder, "ThumbnailCache", (angryBundleId + ".png")));
                        Texture2D localFile = new Texture2D(2, 2);
                        localFile.LoadImage(fileData);
                
                        levelSprite =  Sprite.Create(localFile, new Rect(0.0f, 0.0f, localFile.width, localFile.height), new Vector2(0.5f, 0.5f), 100.0f);
                    }
                    else
                    {
                        path = fallbackThumbnailPath;
                        Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);    
                        levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
                        break;
                    }
                    
                    break;
                }
            }
 
        }
        
        GetGameObjectChild(Root,"SelectedLevelImage").GetComponent<Image>().overrideSprite = levelSprite;
        
        bool canReroll = !data.pointerEnter.gameObject.GetComponent<BingoLevelData>().isClaimed
                         && !MonoSingleton<BingoVoteManager>.Instance.voteOngoing;
        
        GameLevel level = GameManager.CurrentGame.grid.levelTable[data.pointerEnter.gameObject.name];
        
        //Hide level name and thumbnail if playing with the corresponding modifier
        level.levelName = GameManager.CurrentGame.gameSettingsArray["hideLevelNames"] == 1 ? "???" : level.levelName;
        
        //Hide level time to beat if playing with the corresponding modifier
        string timeToBeatString = GameManager.CurrentGame.gameSettingsArray["hidePlayerTimes"] == 1 ? "" : getFormattedTime(level.timeToBeat);
        
        GetGameObjectChild(GetGameObjectChild(Root,"SelectedLevel"),"Text (TMP)").GetComponent<TextMeshProUGUI>().text = level.levelName
            + (level.claimedBy != "NONE" ? "\n<color=orange>" + timeToBeatString + "</color>" : "")
            + (canReroll ? "\n<color=orange>R: Start a reroll vote</color>" : "");
        
        GetGameObjectChild(Root,"SelectedLevel").SetActive(true);
        GetGameObjectChild(Root,"SelectedLevelImage").SetActive(true);
        
        
    }
    
    public static void ShowBingoCardInPauseMenu(ref OptionsManager __instance)
    {
        Game currentGame = GameManager.CurrentGame;
        GameObject templateSquare = GetGameObjectChild(GetGameObjectChild(Root,"Card"),"Image");
        templateSquare.SetActive(false);
        
        for (int x = 0; x < currentGame.grid.size; x++)
        {
            for (int y = 0; y < currentGame.grid.size; y++)
            {
                GameObject levelSquare = GameObject.Instantiate(templateSquare,templateSquare.transform.parent.transform);
                levelSquare.name = x+"-"+y;
                GameLevel levelObject = GameManager.CurrentGame.grid.levelTable[levelSquare.name];
                
                //Set up BingoLevelData
                BingoLevelData bld = levelSquare.AddComponent<BingoLevelData>();
                bld.levelName = levelObject.levelName;
                bld.angryParentBundle = levelObject.angryParentBundle;
                bld.angryLevelId = levelObject.levelId;
                bld.bingoLevelType = levelObject.LevelType;
                bld.column = x;
                bld.row = y;
                bld.isClaimed = (levelObject.claimedBy != "NONE");
                bld.claimedTeam = levelObject.claimedBy;
                
                levelSquare.AddComponent<BingoLevelSquare>();
                levelSquare.GetComponent<Image>().color = teamColors[currentGame.grid.levelTable[x+"-"+y].claimedBy];
                
                if(GameManager.CurrentRow == x && GameManager.CurrentColumn == y)
                {
                    levelSquare.AddComponent<Outline>();
                    levelSquare.GetComponent<Outline>().effectColor = Color.magenta;
                    levelSquare.GetComponent<Outline>().effectDistance = new Vector2(2f,-2f);
                }
                
                levelSquare.AddComponent<EventTrigger>();
                EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                mouseEnter.eventID = EventTriggerType.PointerEnter;
                mouseEnter.callback.AddListener((data) =>
                {
                    onMouseEnterLevelSquare((PointerEventData)data);
                });
                levelSquare.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                levelSquare.SetActive(true);
            }
        }
        
        //Center the grid based on grid size.
        GameObject card = GetGameObjectChild(Root,"Card");
        
        switch(GameManager.CurrentGame.grid.size)
        {
            case 3:
            {
                card.transform.localPosition = new Vector3(-65f,170f,0f);
                card.transform.localScale = new Vector3(1.25f,1.25f,1.25f);
                break;
            }
            case 4:
            {
                card.transform.localPosition = new Vector3(-82.5f,185f,0f);
                card.transform.localScale = new Vector3(1.1f,1.1f,1.1f);
                break;
            }
            default: {break;}
        }
        
    }
    
    public static GameObject Init(ref OptionsManager __instance)
    {
        if(Root == null)
        {
            Root = GameObject.Instantiate(AssetLoader.BingoPauseCard,__instance.pauseMenu.gameObject.transform);
        }
        Root.name = "BingoPauseCard";
        Grid = GetGameObjectChild(Root,"Card");
        Grid.GetComponent<GridLayoutGroup>().constraintCount = GameManager.CurrentGame.grid.size;
        
        DescriptorText = GetGameObjectChild(GetGameObjectChild(Root,"Descriptor"),"Text (TMP)");
        
        Root.SetActive(true);
        
        return Root;
    }
}