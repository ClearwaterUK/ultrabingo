using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Tommy;
using UltraBINGO;
using UltraBINGO.Components;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

public class BingoMapPoolSelection
{
    public static GameObject FetchText;
    public static GameObject SelectedMapsTotal;
    
    public static GameObject MapContainer;
    public static GameObject MapContainerDescription;
    
    public static GameObject MapContainerDescriptionTitle;
    public static GameObject MapContainerDescriptionDesc;
    public static GameObject MapContainerDescriptionNumMaps;
    public static GameObject MapContainerDescriptionMapList;
    
    public static GameObject MapList;
    public static GameObject MapListButtonTemplate;
    
    public static GameObject Back;
    public static GameObject Custom;
    
    public static int NumOfMapsTotal = 0;
    
    public static HashSet<int> SelectedIds = new HashSet<int>();
    public static List<GameObject> MapPoolButtons = new List<GameObject>();
    public static List<MapPoolContainer> AvailableMapPools = new List<MapPoolContainer>();
    public static bool HasAlreadyFetched = false;
    
    public static void ReturnToLobby()
    {
        BingoEncapsulator.BingoMappoolSelectionMenu.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
    }

    public static void DisplayMapSelection()
    {
        BingoEncapsulator.BingoMappoolSelectionMenu.SetActive(false);
        BingoEncapsulator.BingoMapSelection.SetActive(true);
        BingoMapBrowser.Setup();
    }
    
    public static void ClearList(bool force=false)
    {
        SelectedIds.Clear();
        if(getSceneName() != "Main Menu" || force)
        {
            MapPoolButtons.Clear();
            AvailableMapPools.Clear();
            HasAlreadyFetched = false;
        }
    }
    
    public static void UpdateNumber()
    {
        int gridSize = GameManager.CurrentGame.gameSettingsArray["gridSize"];
        int requiredMaps = gridSize*gridSize;
        
        SelectedMapsTotal.GetComponent<TextMeshProUGUI>().text = "Total maps in pool: " + ((NumOfMapsTotal > requiredMaps) ? "<color=green>" : "<color=orange>")
            + NumOfMapsTotal+"</color>"
            + "/"
            + requiredMaps;
    }
    
    public static void ToggleMapPool(ref GameObject mapPool)
    {
        mapPool.GetComponent<MapPoolData>().Toggle();
        bool isEnabled = mapPool.GetComponent<MapPoolData>().mapPoolEnabled;
        
        GetGameObjectChild(mapPool,"Image").GetComponent<Image>().color = (isEnabled ? new Color(1,1,1,1) :new Color(1,1,1,0));
        
        NumOfMapsTotal += mapPool.GetComponent<MapPoolData>().mapPoolNumOfMaps*(isEnabled ? 1 : -1);
        UpdateNumber();
        
        int mapPoolId = mapPool.GetComponent<MapPoolData>().mapPoolId;
        if(isEnabled && !SelectedIds.Contains(mapPoolId))
        {
            SelectedIds.Add(mapPoolId);
        }
        else if (!isEnabled && SelectedIds.Contains(mapPoolId))
        {
            SelectedIds.Remove(mapPoolId);
        }
        
        UpdateMapPool ump = new UpdateMapPool();
        ump.gameId = GameManager.CurrentGame.gameId;
        ump.mapPoolIds = SelectedIds.ToList();
        ump.ticket = NetworkManager.CreateRegisterTicket();
        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ump));
    }
    
    public static void ShowMapPoolData(PointerEventData data)
    {
        //Small sanity check to prevent exceptions
        if(data.pointerEnter.gameObject.name != "Image" && data.pointerEnter.gameObject.name != "Text")
        {
            MapContainerDescription.transform.parent.gameObject.SetActive(false);
            MapPoolData poolData = data.pointerEnter.transform.gameObject.GetComponent<MapPoolData>();
            
            MapContainerDescriptionTitle.GetComponent<TextMeshProUGUI>().text = "-- <color=orange>" + poolData.mapPoolName + "</color> --";
            MapContainerDescriptionDesc.GetComponent<TextMeshProUGUI>().text = poolData.mapPoolDescription;
            MapContainerDescriptionNumMaps.GetComponent<TextMeshProUGUI>().text = "Number of maps: <color=orange>" + poolData.mapPoolNumOfMaps + "</color>";
            
            string mapString = "";
            foreach(string map in poolData.mapPoolMapList)
            {
                mapString += map + "\n";
            }

            MapContainerDescriptionMapList.GetComponent<TextMeshProUGUI>().text = mapString;
            MapContainerDescriptionMapList.SetActive(true);
            MapContainerDescription.transform.parent.gameObject.SetActive(true);
        }
    }
    
    public static void HideMapPoolData()
    {
        MapContainerDescription.SetActive(false);
    }

    public static void setupMapPools(List<MapPool> mapPools)
    {
        foreach (MapPool m in mapPools)
        {
            GameObject newMapPool = GameObject.Instantiate(MapListButtonTemplate,MapListButtonTemplate.transform.parent);
                    
            MapPoolData poolData = newMapPool.AddComponent<MapPoolData>();
            poolData.mapPoolId = m.MapPoolId;
            poolData.mapPoolName = m.MapPoolName;
            poolData.mapPoolDescription = m.MapPoolDescription;
            poolData.mapPoolNumOfMaps = m.MapPoolLevelCount;
            poolData.mapPoolMapList = new List<string>() { "" };

            GetGameObjectChild(newMapPool, "Text").GetComponent<Text>().text = m.MapPoolName;
                    
            newMapPool.AddComponent<EventTrigger>();
            EventTrigger.Entry mouseEnter = new EventTrigger.Entry();
            mouseEnter.eventID = EventTriggerType.PointerEnter;
            mouseEnter.callback.AddListener((data) =>
            {
                ShowMapPoolData((PointerEventData)data);
            });
            newMapPool.GetComponent<EventTrigger>().triggers.Add(mouseEnter);
                
            EventTrigger.Entry mouseExit = new EventTrigger.Entry();
            mouseExit.eventID = EventTriggerType.PointerExit;
            mouseExit.callback.AddListener((data) =>
            {
                HideMapPoolData();
            });
                    
            newMapPool.GetComponent<Button>().onClick.AddListener(delegate
            {
                ToggleMapPool(ref newMapPool);
            });
                    
            MapPoolButtons.Add(newMapPool);
            newMapPool.SetActive(true);
        }
        HasAlreadyFetched = true;
        FetchText.SetActive(false);
        MapContainer.SetActive(true);
    }

    public static void Init(ref GameObject MapSelection)
    {
        FetchText = GetGameObjectChild(MapSelection,"FetchText");
        
        MapContainer = GetGameObjectChild(MapSelection,"MapContainer");
        SelectedMapsTotal = GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolList"),"SelectedMapsTotal");
        
        MapContainerDescription = GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolDescription"),"Contents");

        MapContainerDescriptionTitle = GetGameObjectChild(MapContainerDescription,"Title");
        MapContainerDescriptionDesc = GetGameObjectChild(MapContainerDescription,"Description");
        MapContainerDescriptionNumMaps = GetGameObjectChild(MapContainerDescription,"NumMaps");
        MapContainerDescriptionMapList = GetGameObjectChild(GetGameObjectChild(MapContainerDescription,"MapsList"),"MapName");
        
        MapList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(MapContainer,"MapPoolList"),"Scroll View"),"Scroll Rect"),"Content"),"List");
        MapListButtonTemplate = GetGameObjectChild(MapList,"MapListButton");
        Back = GetGameObjectChild(MapSelection,"Back");
        Back.GetComponent<Button>().onClick.AddListener(delegate
        {
            ReturnToLobby();
        });
        
        Custom = GetGameObjectChild(MapSelection, "CustomMappool");
        Custom.GetComponent<Button>().onClick.AddListener(delegate
        {
            DisplayMapSelection();
        });
    }
}