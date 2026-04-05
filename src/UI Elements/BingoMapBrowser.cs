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

    public static string angryCatalogURL = "https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/V2/LevelCatalog.json";
    public static string angryThumbnailURL = "https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/Levels/";

    public static string ultraEditorAPIURL = "https://duviz.xyz/api/ultraeditor/";
    public static string ultraEditorCatalogURL = "fetchlevels";
    public static string ultraEditorLevelURL = "getlevel/";
    public static string ultraEditorLevelImageURL = "getimg/";
    public static string ultraEditorDownloadURL = "downloadlevel/";

    public static GameObject CampaignCategory;
    public static GameObject AngryCategory;
    public static GameObject UltraEditorCategory;
    public static List<GameObject> categoryList = new List<GameObject>();
    
    public static GameObject MapTemplate;
    
    public static bool hasFetched = false;

    public static AngryMapCatalog catalog = null;
    public static List<string> ultraEditorCatalog = null;

    public static TMP_Dropdown mapCategoryDropdown = null;

    //public static List<string> selectedLevels = new List<string>();
    public static Dictionary<string, BingoMapSelectionID> selectedLevels = new Dictionary<string, BingoMapSelectionID>();
    public static List<string> selectedLevelNames = new List<string>();

    public static List<GameObject> campaignLevelCatalog = new List<GameObject>();
    public static List<GameObject> angryLevelCatalog = new List<GameObject>();
    public static List<GameObject> ultraEditorLevelCatalog = new List<GameObject>();

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

    public static void ClearCategories()
    {
        angryLevelCatalog.Clear();
        ultraEditorLevelCatalog.Clear();    
    }

    public static async Task<Texture2D> asyncFetchUltraEditorThumbnail(string imageName, string imageUrl)
    {
        //Start by checking if the file exists.
        if (File.Exists(Path.Combine(Main.ModFolder, "ThumbnailCache", (imageName + ".png"))))
        {
            Logging.Message("Loading " + (imageName + ".png") + " from thumbnail cache");
            byte[] fileData = File.ReadAllBytes(Path.Combine(Main.ModFolder, "ThumbnailCache", (imageName + ".png")));
            Texture2D localFile = new Texture2D(2, 2);
            localFile.LoadImage(fileData);
            return localFile;
        }
        else
        {
            Logging.Warn("Downloading " + imageName);
            using (UnityWebRequest texture = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                texture.SendWebRequest();
                while (!texture.isDone)
                {
                    await Task.Yield();
                }

                if (texture.result == UnityWebRequest.Result.Success)
                {
                    Logging.Message("Saving " + (imageName + ".png") + " to thumbnail cache");
                    byte[] bytes = DownloadHandlerTexture.GetContent(texture).EncodeToPNG();
                    File.WriteAllBytes(Path.Combine(Main.ModFolder, "ThumbnailCache", (imageName + ".png")), bytes);

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

        return null;
    }
    
    public static async Task<Texture2D> FetchThumbnail(string thumbnailGuid)
    {
        
        //Start by checking if the GUID.png file exists.
        if (File.Exists(Path.Combine(Main.ModFolder, "ThumbnailCache", (thumbnailGuid + ".png"))))
        {
            //Logging.Message("Loading " + (thumbnailGuid + ".png") + " from thumbnail cache");
            byte[] fileData = File.ReadAllBytes(Path.Combine(Main.ModFolder, "ThumbnailCache", (thumbnailGuid + ".png")));
            Texture2D localFile = new Texture2D(2, 2);
            localFile.LoadImage(fileData);
            return localFile;

        }
        //If not, download the thumbnail file and cache it to prevent unnecessary redownloading.
        else
        {
            string url = angryThumbnailURL + "/" + thumbnailGuid + "/thumbnail.png";

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
        if (!selectedLevels.ContainsKey(levelId))
        {
            selectedLevels.Add(levelId,levelPanel.GetComponent<BingoMapSelectionID>());
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

    public static async Task<int> fetchAngryCatalog()
    {
        try
        {
            string catalogString = await NetworkManager.FetchCatalog(angryCatalogURL);
            
            catalog = JsonConvert.DeserializeObject<AngryMapCatalog>(catalogString);
            return 0;
        }
        catch(Exception e)
        {
            Logging.Error(e.ToString());
            return -1;
        }
    }

    public static async Task<int> fetchUltraeditorCatalog()
    {
        try
        {
            string numOfUltraeditorMaps = await NetworkManager.FetchCatalog(angryCatalogURL);
            return Int32.Parse(numOfUltraeditorMaps);
        }
        catch (Exception e)
        {
            Logging.Error(e.ToString());
            return -1;
        }
    }

    public static void setupCampaignLevelIds()
    {
        campaignLevelIds = new List<int>();
        
        for(int x = 1; x<34;x++) {campaignLevelIds.Add(x);} //Campaigns
        List<int> encoreIds = new List<int>() { 100, 101 }; //Encore
        List<int> primeIds = new List<int>() { 666, 667 }; //Prime
        
        campaignLevelIds = campaignLevelIds.Concat(encoreIds).Concat(primeIds).ToList();

    }

    public static async Task asyncFetchUltraEditorThumbnails()
    {
        foreach (GameObject ultraeditorLevel in ultraEditorLevelCatalog)
        {
            Texture2D levelImg = await asyncFetchUltraEditorThumbnail(ultraeditorLevel.GetComponent<BingoMapSelectionID>().UltraEditorImageName,
                ultraeditorLevel.GetComponent<BingoMapSelectionID>().UltraEditorImageURL);
            if (levelImg != null)
            {
                GetGameObjectChild(ultraeditorLevel, "BundleImage").GetComponent<Image>().sprite =
                    Sprite.Create(levelImg,
                        new Rect(0, 0, levelImg.width, levelImg.height),
                        new Vector2(0.5f, 0.5f));
            }
        }
    }

    public static async Task asyncFetchAngryThumbnails()
    {
        foreach(GameObject angryLevel in angryLevelCatalog)
        {
            if (angryLevel.name.Contains("Level "))
            {
                continue;
            }
            Texture2D bundleImg = await FetchThumbnail(angryLevel.GetComponent<BingoMapSelectionID>().angryBundleId);
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
        
        SelectedMapsList.GetComponent<TextMeshProUGUI>().text = "";
        selectedMapsCount.GetComponent<TextMeshProUGUI>().text = "";
    }
    
    
    public static async void Setup()
    {
        MapBrowserWindow.SetActive(true);
        if (hasFetched) { return; }
        
        //Start by adding the official campaign levels.
        //Using levelIDs here as we can just call GetMissionName.GetMissionNameOnly to get the name.
        setupCampaignLevelIds();
        GameObject campaignWindow = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(CampaignCategory,"Grid"),"Scroll View"),"Viewport"),"Content");
        GameObject campaignTemplate = GetGameObjectChild(campaignWindow, "MapTemplate");
        foreach (int campaignLevel in campaignLevelIds)
        {

            
            GameObject levelPanel = GameObject.Instantiate(campaignTemplate, campaignTemplate.transform.parent);
            GetGameObjectChild(levelPanel, "BundleName").GetComponent<Text>().text = "CAMPAIGN";
            GetGameObjectChild(levelPanel, "MapName").GetComponent<Text>().text = GetMissionName.GetMissionNameOnly(campaignLevel);
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

            string path = "assets/bingo/lvlimg/campaign/"+GetMissionName.GetSceneName(campaignLevel)+".png";

            Texture2D levelImg = AssetLoader.Assets.LoadAsset<Texture2D>(path);
            Sprite levelSprite = Sprite.Create(levelImg, new Rect(0.0f, 0.0f, levelImg.width, levelImg.height), new Vector2(0.5f, 0.5f), 100.0f);
            GetGameObjectChild(levelPanel, "BundleImage").GetComponent<Image>().sprite = levelSprite;
            levelPanel.name = GetMissionName.GetSceneName(campaignLevel);
            levelPanel.SetActive(true);

            levelPanel.AddComponent<BingoMapSelectionID>();
            levelPanel.GetComponent<BingoMapSelectionID>().levelType = BingoLevelType.Campaign;
            levelPanel.GetComponent<BingoMapSelectionID>().levelName = GetMissionName.GetMissionNameOnly(campaignLevel);
            levelPanel.GetComponent<BingoMapSelectionID>().levelId = GetMissionName.GetSceneName(campaignLevel);
                    
            levelPanel.AddComponent<Button>();
            levelPanel.GetComponent<Button>().onClick.AddListener(delegate
            {
                ToggleMapSelection(ref levelPanel, GetMissionName.GetSceneName(campaignLevel), GetMissionName.GetMissionNameOnly(campaignLevel));
            });
            campaignLevelCatalog.Add(levelPanel);
        }
        
        //Then fetch the Angry maps...
        Logging.Message("Fetching Angry map catalog...");

        int angryFetchResult = await fetchAngryCatalog();
        if (angryFetchResult == 0)
        {
            GameObject angryWindow = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(AngryCategory,"Grid"),"Scroll View"),"Viewport"),"Content");
            GameObject angryTemplate = GetGameObjectChild(angryWindow, "MapTemplate");
            foreach (AngryBundle bundle in catalog.Levels)
            {
                foreach (AngryLevel level in bundle.Levels)
                {
                    
                    GameObject levelPanel = GameObject.Instantiate(angryTemplate, angryTemplate.transform.parent);
                        
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
                    mouseExit.callback.AddListener((data) => { HideMapData(); });

                    levelPanel.AddComponent<BingoMapSelectionID>();
                    levelPanel.GetComponent<BingoMapSelectionID>().levelType = BingoLevelType.Angry;
                    levelPanel.GetComponent<BingoMapSelectionID>().levelName = level.LevelName;
                    levelPanel.GetComponent<BingoMapSelectionID>().levelId = level.LevelId;
                    levelPanel.GetComponent<BingoMapSelectionID>().angryBundleId = bundle.Guid;
                    levelPanel.GetComponent<BingoMapSelectionID>().thumbnailPath = bundle.Guid;

                    levelPanel.name = level.LevelId;
                    levelPanel.SetActive(true);
                        
                    levelPanel.AddComponent<Button>();
                    levelPanel.GetComponent<Button>().onClick.AddListener(delegate { ToggleMapSelection(ref levelPanel, level.LevelId,level.LevelName); }); 
                    angryLevelCatalog.Add(levelPanel);
                }
            }
        }
        
        //And finally the UltraEditor maps.   
        Logging.Message("Fetching UltraEditor map catalog...");
        string fetchURL = ultraEditorAPIURL + ultraEditorCatalogURL;
        int ultraEditorLevelCount = Int16.Parse(await NetworkManager.FetchCatalog(fetchURL));
        if(ultraEditorLevelCount > 0)
        {
            GameObject ultraeditorWindow = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(UltraEditorCategory,"Grid"),"Scroll View"),"Viewport"),"Content");
            GameObject ultraeditorTemplate = GetGameObjectChild(ultraeditorWindow, "MapTemplate");
            for (int x = 0; x < ultraEditorLevelCount - 1; x++)
            {
                string ultrakillLevelData = await NetworkManager.FetchCatalog(ultraEditorAPIURL + ultraEditorLevelURL + x);
                UltraEditorLevelData levelData = JsonConvert.DeserializeObject<UltraEditorLevelData>(ultrakillLevelData);
                levelData.url = ultraEditorAPIURL + ultraEditorLevelURL + x;
                
                GameObject levelPanel = GameObject.Instantiate(ultraeditorTemplate, ultraeditorTemplate.transform.parent);
                ultraEditorLevelCatalog.Add(levelPanel);
                levelPanel.AddComponent<BingoMapSelectionID>();
                
                GetGameObjectChild(levelPanel, "SelectionIndicator").SetActive(false);
                GetGameObjectChild(levelPanel, "BundleName").GetComponent<Text>().text = "ULTRAEDITOR";
                GetGameObjectChild(levelPanel, "MapName").GetComponent<Text>().text = levelData.name;
                levelPanel.GetComponent<BingoMapSelectionID>().levelType = BingoLevelType.UltraEditor;
                levelPanel.GetComponent<BingoMapSelectionID>().levelName = levelData.name;
                levelPanel.GetComponent<BingoMapSelectionID>().levelId = levelData.guid;
                levelPanel.GetComponent<BingoMapSelectionID>().UltraEditorLevelData = ultraEditorAPIURL + ultraEditorDownloadURL + x;
                levelPanel.GetComponent<BingoMapSelectionID>().UltraEditorImageURL = ultraEditorAPIURL + ultraEditorLevelImageURL + x;
                levelPanel.GetComponent<BingoMapSelectionID>().UltraEditorImageName =
                    levelPanel.GetComponent<BingoMapSelectionID>().levelId;
                levelPanel.GetComponent<BingoMapSelectionID>().thumbnailPath = levelData.image;
                
                levelPanel.AddComponent<Button>();
                levelPanel.GetComponent<Button>().onClick.AddListener(delegate
                {
                    ToggleMapSelection(ref levelPanel, levelData.name, levelData.guid);
                });
                
                levelPanel.name = levelPanel.GetComponent<BingoMapSelectionID>().levelId;
                levelPanel.SetActive(true);
                
                ultraEditorLevelCatalog.Add(levelPanel);
            }
        }
        
        Logging.Warn("Loading Angry thumbnails");
        await asyncFetchAngryThumbnails();
        Logging.Warn("Loading UltraEditor thumbnails");
        await asyncFetchUltraEditorThumbnails();
        
        LoadingText.SetActive(false);
        hasFetched = true;
    }

    public static void onCategoryUpdate(int value)
    {
        foreach (GameObject category in categoryList)
        {
            category.SetActive(false);
        }
        categoryList[value].SetActive(true);
    }
    
    public static void Init(ref GameObject MapBrowser)
    {
        LoadingText = GetGameObjectChild(MapBrowser, "DownloadingText");
        
        MapBrowserWindow = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(MapBrowser,"MapsCampaign"),"Grid"),"Scroll View"),"Viewport"),"Content");
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

        mapCategoryDropdown = GetGameObjectChild(GetGameObjectChild(MapBrowser, "MapsDropdown"),"Dropdown").GetComponent<TMP_Dropdown>();
        mapCategoryDropdown.onValueChanged.AddListener(delegate { onCategoryUpdate(mapCategoryDropdown.value); });
        CampaignCategory = GetGameObjectChild(MapBrowser, "MapsCampaign");
        AngryCategory = GetGameObjectChild(MapBrowser, "MapsAngry");
        UltraEditorCategory = GetGameObjectChild(MapBrowser, "MapsUltraEditor");
        categoryList.Clear();
        categoryList.Add(CampaignCategory);
        categoryList.Add(AngryCategory);
        categoryList.Add(UltraEditorCategory);
    }
}