﻿using TMPro;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO;

public static class UIManager
{
    public static GameObject ultrabingoButtonObject = null;
    public static GameObject ultrabingoEncapsulator = null;
    
    public static void Open()
    {
        //Hide chapter select
        ultrabingoButtonObject.transform.parent.gameObject.SetActive(false);
        BingoEncapsulator.Root.SetActive(true);
        
        /*ultrabingoEncapsulator.transform.parent.gameObject.SetActive(false);
        ultrabingoEncapsulator.SetActive(true);
        GetGameObjectChild(ultrabingoEncapsulator,"UltraBingoMenu").SetActive(true);*/
        
    }
    
    public static void Close()
    {
        //Show chapter select
        ultrabingoEncapsulator.SetActive(false);
        ultrabingoButtonObject.transform.parent.gameObject.SetActive(true);
    }
    
    //Borrowed and repurposed this code from Hydra's BossRush mod, cheers man :D
    public static void SetupElements(CanvasController __instance)
    {
        RectTransform canvasRectTransform = __instance.GetComponent<RectTransform>();
        GameObject chapterSelectObject = canvasRectTransform.Find("Chapter Select").gameObject;
        if (chapterSelectObject == null)
        {
            Logging.Error("Chapter Select object is null");
            return;
        }
        
        RectTransform chapterSelectRectTransform = chapterSelectObject.GetComponent<RectTransform>();
        GameObject sandboxButtonObject = chapterSelectObject.transform.Find("Sandbox").gameObject;

        if (sandboxButtonObject == null)
        {
            Logging.Error("Sandbox button is null");
            return;
        }
        if(ultrabingoButtonObject == null)
        {
            ultrabingoButtonObject = GameObject.Instantiate(sandboxButtonObject, chapterSelectRectTransform);
            ultrabingoButtonObject.name = "UltraBingo Button";
        }
        Button sandboxButton = ultrabingoButtonObject.GetComponent<Button>();

        ColorBlock oldColorBlock = sandboxButton.colors;
        //Have to destroy old button component because of Unity's persistent listener calls.
        //They can't be removed at runtime so the component must be replaced.
        GameObject.DestroyImmediate(sandboxButton);

        Button ultrabingoButton = ultrabingoButtonObject.AddComponent<Button>();
        ultrabingoButton.colors = oldColorBlock;


        RectTransform ultrabingoButtonRectTransform = ultrabingoButtonObject.GetComponent<RectTransform>();

        Vector3 buttonPosition = ultrabingoButtonRectTransform.position;
        buttonPosition.y = 750;
        ultrabingoButtonRectTransform.position = buttonPosition;

        ultrabingoButtonRectTransform.GetComponentInChildren<TextMeshProUGUI>().text = "ULTRABINGO";
        ultrabingoButton.onClick.AddListener(Open);
        
        if(ultrabingoEncapsulator == null)
        {
            ultrabingoEncapsulator = BingoEncapsulator.Init();
            ultrabingoEncapsulator.name = "UltraBingo";
            
        }

        ultrabingoEncapsulator.transform.parent = __instance.transform;
        ultrabingoEncapsulator.SetActive(false);
        
        //ultrabingoLobby = BingoLobby.Init();
        //ultrabingoLobby.transform.parent = ultrabingoEncapsulator.transform;
        //GameObject lobbyMenu = BingoLobby.Init();
        //lobbyMenu.transform.parent = ultrabingoButton.transform;
        
    }
}