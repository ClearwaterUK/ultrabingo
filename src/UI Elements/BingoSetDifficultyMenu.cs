using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltraEditor.Libraries;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.UI_Elements;

public static class BingoSetDifficultyMenu
{
    public static TMP_Dropdown BaseDifficulty;
    
    public static GameObject PlayerList;
    public static GameObject PlayerTemplate;

    public static Button Cancel;
    public static Button Finish;

    public static Dictionary<string, int> difficultyOverrides = new Dictionary<string, int>();

    public static void UpdateDifficulties()
    {
        DifficultySettings ds = new DifficultySettings();
        ds.gameId = GameManager.CurrentGame.gameId;
        ds.baseDifficulty = BaseDifficulty.value;
        ds.difficultyOverride = difficultyOverrides;
        ds.ticket = NetworkManager.CreateRegisterTicket();
        
        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(ds));
        ReturnToLobbyMenu();
    }
    
    public static void Init(ref GameObject BingoSetDifficulty)
    {
        BaseDifficulty = GetGameObjectChild(GetGameObjectChild(BingoSetDifficulty, "BaseDifficulty"),"Dropdown").GetComponent<TMP_Dropdown>();

        PlayerList = GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(GetGameObjectChild(BingoSetDifficulty, "PlayerList"),"Grid"),"Scroll View"),"Viewport"),"Content");
        
        PlayerTemplate = GetGameObjectChild(PlayerList,"PlayerTemplate");
        
        Cancel = GetGameObjectChild(BingoSetDifficulty,"Cancel").GetComponent<Button>();
        Cancel.onClick.AddListener(delegate
        {
            ReturnToLobbyMenu();
        });
        Finish = GetGameObjectChild(BingoSetDifficulty,"Finish").GetComponent<Button>();
        Finish.onClick.AddListener(delegate
        {
            UpdateDifficulties();
            ReturnToLobbyMenu();
        });
        
    }

    public static void Setup()
    {
        Dictionary<string,Player> playerList = GameManager.CurrentGame.currentPlayers;
        List<string> playerSteamIds = playerList.Keys.ToList();
        
        //Clear out before applying anything
        difficultyOverrides.Clear();
        foreach (string id in playerSteamIds)
        {
            foreach(Transform child in PlayerTemplate.parent.transform)
            {
                if(child.gameObject.name != "PlayerTemplate")
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        //Then create buttons for each player current connected to the game.
        foreach(string id in playerSteamIds)
        {
            string playerName = GameManager.CurrentGame.currentPlayers[id].username;
            GameObject playerDifficultyOverride = GameObject.Instantiate(PlayerTemplate,PlayerTemplate.parent.transform);
            playerDifficultyOverride.name = id;
            GetGameObjectChild(playerDifficultyOverride,"Text").GetComponent<TextMeshProUGUI>().text = playerName;
            TMP_Dropdown selectedDifficulty = GetGameObjectChild(playerDifficultyOverride, "Dropdown").GetComponent<TMP_Dropdown>();
            
            //Set default override for each player to 0 (no override)
            difficultyOverrides.Add(id,0);
            selectedDifficulty.onValueChanged.AddListener(delegate
            {
                difficultyOverrides[id] = selectedDifficulty.value;
            });
            
            playerDifficultyOverride.SetActive(true);
        }
    }
    
    public static void ReturnToLobbyMenu()
    {
        BingoEncapsulator.BingoSetDifficulty.SetActive(false);
        BingoEncapsulator.BingoLobbyScreen.SetActive(true);
    }
}