using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO;
using UltraBINGO.Components;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

public class BingoMapBrowser
{
    public static GameObject LoadingText;
    public static GameObject MapBrowserWindow;

    public static GameObject SelectedMapsList;
    public static GameObject selectedMapsCount;
    
    public static GameObject BackButton;
    public static GameObject FinishButton;

    public static string catalogURL = "https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/V2/LevelCatalog.json";
    public static string thumbnailURL = "https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/Levels/";
    
    public static GameObject MapTemplate;
    
    public static bool hasFetched = false;

    public static AngryMapCatalog catalog = null;

    public static List<string> selectedLevels = new List<string>();
    public static List<string> selectedLevelNames = new List<string>();

    public static List<GameObject> levelCatalog = new List<GameObject>();

    public static List<int> campaignLevelIds;
    
    public static void ReturnToLobby()
    {
        BingoEncapsulator.BingoMapSelection.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
    }

    public static void ShowMapData(PointerEventData pointerEventData, AngryBundle parentBundle=null, AngryLevel level=null)
    {
        
    }

    public static void HideMapData()
    {
        
    }
    
    public static async Task<Texture2D> FetchThumbnail(string thumbnailGuid)
    {
        
        //Start by checking if the GUID.png file exists.
        if (File.Exists(Path.Combine(Main.ModFolder, "ThumbnailCache", (thumbnailGuid + ".png"))))
        {
            Logging.Message("Loading " + (thumbnailGuid + ".png") + " from thumbnail cache");
            byte[] fileData = File.ReadAllBytes(Path.Combine(Main.ModFolder, "ThumbnailCache", (thumbnailGuid + ".png")));
            Texture2D localFile = new Texture2D(2, 2);
            localFile.LoadImage(fileData);
            return localFile;

        }
        //If not, download the thumbnail file and cache it to prevent unnecessary redownloading.
        else
        {
            string url = thumbnailURL + "/" + thumbnailGuid + "/thumbnail.png";

            using (UnityWebRequest texture = UnityWebRequestTexture.GetTexture(url))
            { 
                texture.SendWebRequest();
                while (!texture.isDone) { await Task.Yield();}
            
                if (texture.result == UnityWebRequest.Result.Success)
                {
                    Logging.Message("Saving " + (thumbnailGuid + ".png") + " to thumbnail cache");
                    byte[] bytes = DownloadHandlerTexture.GetContent(texture).EncodeToPNG();
                    File.WriteAllBytes(Path.Combine(Main.ModFolder,"ThumbnailCache",(thumbnailGuid + ".png")),bytes);
                    
                    return DownloadHandlerTexture.GetContent(texture);
                }
                else
                {
                    Logging.Error("Error while trying to download image from thumbnail catalog!");
                    Logging.Error(texture.responseCode.ToString());
                    return null;
                }
            }
        }
    }

    public static void UpdateSelectedMaps()
    {
        int gridSize = GameManager.CurrentGame.gameSettingsArray["gridSize"];
        
        int requiredNumOfMaps = (3 + gridSize) * (3 + gridSize);

        SelectedMapsList.GetComponent<TextMeshProUGUI>().text = string.Join("\n", selectedLevelNames);
        selectedMapsCount.GetComponent<TextMeshProUGUI>().text = (selectedLevels.Count >= requiredNumOfMaps ? "<color=green>" : "<color=orange>")
            + selectedLevels.Count + "</color>/<color=orange>" + requiredNumOfMaps + "</color>";
    }

    public static void ToggleMapSelection(ref GameObject levelPanel, string levelId, string levelName)
    {
        if (!selectedLevels.Contains(levelId))
        {
            selectedLevels.Add(levelId);
            GetGameObjectChild(levelPanel, "SelectionIndicator").SetActive(true);
            selectedLevelNames.Add(levelName);
        }
        else
        {
            selectedLevels.Remove(levelId);
            GetGameObjectChild(levelPanel, "SelectionIndicator").SetActive(false);
            selectedLevelNames.Remove(levelName);
        }

        UpdateSelectedMaps();
    }

    public static async Task<int> fetchCatalog()
    {
        try
        {
            string catalogString = await NetworkManager.FetchCatalog(catalogURL);
            
            catalog = JsonConvert.DeserializeObject<AngryMapCatalog>(catalogString);
            return 0;
        }
        catch(Exception e)
        {
            Logging.Error(e.ToString());
            return -1;
        }
    }

    public static void setupCampaignLevelIds()
    {
        campaignLevelIds = new List<int>();
        
        for(int x = 1; x<30;x++) {campaignLevelIds.Add(x);} //Campaigns
        List<int> encoreIds = new List<int>() { 100, 101 }; //Encore
        List<int> primeIds = new List<int>() { 666, 667 }; //Prime
        
        campaignLevelIds = campaignLevelIds.Concat(encoreIds).Concat(primeIds).ToList();

    }

    public static async Task asyncFetchCustomThumbnails()
    {
        foreach(GameObject angryLevel in levelCatalog)
        {
            if (angryLevel.name.Contains("Level "))
            {
                continue;
            }
            Texture2D bundleImg = await FetchThumbnail(angryLevel.GetComponent<BingoMapSelectionID>().bundleId);
            if (bundleImg != null)
            {
                GetGameObjectChild(angryLevel, "BundleImage").GetComponent<Image>().sprite =
                    Sprite.Create(bundleImg,
                        new Rect(0, 0, bundleImg.width, bundleImg.height),
                        new Vector2(0.5f, 0.5f));
            }
        }
    }

    public static void ResetListPosition()
    {
        foreach (GameObject tab in levelCatalog)
        {
            if (tab != null)
            {
                GameObject.Destroy(tab);
            }
        }

        levelCatalog = new List<GameObject>();

        SelectedMapsList.GetComponent<TextMeshProUGUI>().text = "";
        selectedMapsCount.GetComponent<TextMeshProUGUI>().text = "";
    }
    
    public static async void Setup()
    {
        if (!hasFetched)
        {
            Logging.Message("Fetching Angry map catalog...");

            int fetchResult = await fetchCatalog();
            if (fetchResult == 0 && !hasFetched)
            {
                MapBrowserWindow.SetActive(true);
                
                //Start by adding the official campaign levels.
                //Using levelIDs here as we can just call GetMissionName.GetMissionNameOnly to get the name.
                setupCampaignLevelIds();
                
                foreach (int campaignLevel in campaignLevelIds)
                {
                    GameObject levelPanel = GameObject.Instantiate(MapTemplate, MapTemplate.transform.parent);
                    GetGameObjectChild(levelPanel, "BundleName").GetComponent<Text>().text = "CAMPAIGN";

                    GetGameObjectChild(levelPanel, "MapName").GetComponent<Text>().text =
                        GetMissionName.GetMissionNameOnly(campaignLevel);
                    GetGameObjectChild(levelPanel, "SelectionIndicator").SetActive(false);
                    
                    EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                    mouseEnter.eventID = EventTriggerType.PointerEnter;
                    mouseEnter.callback.AddListener((data) =>
                    {
                        ShowMapData((PointerEventData)data);
                    });
                    levelPanel.AddComponent<EventTrigger>();
                    levelPanel.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
                    EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                    mouseExit.eventID = EventTriggerType.PointerExit;
                    mouseExit.callback.AddListener((data) => { HideMapData(); });

                    levelPanel.AddComponent<Button>();
                    levelPanel.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ToggleMapSelection(ref levelPanel, GetMissionName.GetSceneName(campaignLevel), GetMissionName.GetMissionNameOnly(campaignLevel));
                    });

                    string path = "assets/bingo/lvlimg/campaign/"+GetMissionName.GetSceneName(campaignLevel)+".png";

                    Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);
                    Sprite levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
                    GetGameObjectChild(levelPanel, "BundleImage").GetComponent<Image>().sprite = levelSprite;
                    levelPanel.name = GetMissionName.GetSceneName(campaignLevel);
                    levelPanel.SetActive(true);
                    levelCatalog.Add(levelPanel);
                }
                
                //Then show all the Angry levels...
                foreach (AngryBundle bundle in catalog.Levels)
                {
                    foreach (AngryLevel level in bundle.Levels)
                    {
                        GameObject levelPanel = GameObject.Instantiate(MapTemplate, MapTemplate.transform.parent);
                        
                        GetGameObjectChild(levelPanel, "BundleName").GetComponent<Text>().text = bundle.Name;
                        
                        GetGameObjectChild(levelPanel, "MapName").GetComponent<Text>().text = level.LevelName;
                        GetGameObjectChild(levelPanel, "SelectionIndicator").SetActive(false);
                        
                        EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
                        mouseEnter.eventID = EventTriggerType.PointerEnter;
                        mouseEnter.callback.AddListener((data) => { ShowMapData((PointerEventData)data,bundle,level); });
                        levelPanel.AddComponent<EventTrigger>();
                        levelPanel.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
                        EventTrigger.Entry mouseExit = new EventTrigger.Entry();
                        mouseExit.eventID = EventTriggerType.PointerExit;
                        mouseExit.callback.AddListener((data) =>
                        {
                            HideMapData();
                        });

                        levelPanel.AddComponent<Button>();
                        levelPanel.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            ToggleMapSelection(ref levelPanel, level.LevelId,level.LevelName);
                        });

                        levelPanel.AddComponent<BingoMapSelectionID>();
                        levelPanel.GetComponent<BingoMapSelectionID>().bundleId = bundle.Guid;

                        levelPanel.name = level.LevelId;
                        levelPanel.SetActive(true);
                        levelCatalog.Add(levelPanel);
                        
                    }
                }
                
                LoadingText.SetActive(false);
                hasFetched = true;

                await asyncFetchCustomThumbnails();
            }
        }
    }
    
    public static void Init(ref GameObject MapBrowser)
    {
        LoadingText = GetGameObjectChild(MapBrowser, "DownloadingText");
        
        MapBrowserWindow = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(MapBrowser,"Maps"),"Grid"),"Scroll View"),"Viewport"),"Content");
        MapTemplate = GetGameObjectChild(MapBrowserWindow, "MapTemplate");

        BackButton = GetGameObjectChild(MapBrowser, "Back");
        BackButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            ReturnToLobby();
        });
        FinishButton = GetGameObjectChild(MapBrowser, "Finish");
        FinishButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            GameManager.isUsingCustomMappool = true;
            GameManager.customMappool = selectedLevels;
            ReturnToLobby();
        });

        SelectedMapsList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(MapBrowser,"Summary"),"SelectedMaps"),"List");
        selectedMapsCount = GetGameObjectChild(GetGameObjectChild(MapBrowser,"Summary"),"TotalMapsNumber");
        selectedMapsCount.GetComponent<TextMeshProUGUI>().text = "<color=orange>0</color>/<color=orange>"+"0"+"</color>";
    }
}