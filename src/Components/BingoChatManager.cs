using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UltraBINGO.NetworkMessages;
using UltrakillBingoClient;
using UnityEngine;
using UnityEngine.UI;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.Components;

public class BingoChatManager : MonoSingleton<BingoChatManager>
{
    public Queue<GameObject> ChatHistory = new Queue<GameObject>();
    
    public GameObject ChatWindow;
    private GameObject ChatTemplate;
    
    public TMP_InputField messageContents;

    public TextMeshProUGUI ChannelIndicator;

    public bool isGlobalMessage = true;

    public bool ready = false;
        
    public void Bind(GameObject bindingObject)
    {
        ChatWindow = bindingObject;
        ChatTemplate = GetGameObjectChild(GetGameObjectChild(ChatWindow, "Chat"), "Example");
        
        messageContents = GetGameObjectChild(ChatWindow,"MessageInput").GetComponent<TMP_InputField>();

        ChannelIndicator = GetGameObjectChild(GetGameObjectChild(ChatWindow, "ChannelIndicator"), "Text")
            .GetComponent<TextMeshProUGUI>();
        
        //Hide the channel switcher if game hasn't started and we're not in-game yet
        if(!GameManager.IsInBingoLevel)
        {
            ChannelIndicator.transform.parent.gameObject.SetActive(false);
            messageContents.transform.localPosition = new Vector3(-17.5f, -87.5f, 0f);
            messageContents.placeholder.GetComponent<TextMeshProUGUI>().text = "Type a message...";
        }

        messageContents.onEndEdit.AddListener(delegate
        {
            if (messageContents.text.Length > 0 && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
            {
                if(GameManager.canUseChat) { SendChatMessage(messageContents.text);}
                messageContents.text = "";
            }
        });
        
        clearChatHistory();
        if (!GameManager.canUseChat)
        {
            messageContents.gameObject.SetActive(false);
            ChannelIndicator.transform.parent.gameObject.SetActive(false);
        }
        ready = true;
    }

    public void updateChatPanel()
    {
        if (!GameManager.canUseChat)
        {
            messageContents.gameObject.SetActive(false);
            ChannelIndicator.transform.parent.gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        if (ready)
        {
            if (messageContents.isFocused && Input.GetKeyUp(KeyCode.Tab) && GameManager.IsInBingoLevel)
            {
                messageContents.text = messageContents.text.Replace("\t", "");
                isGlobalMessage = !isGlobalMessage;
            }

            if (GameManager.IsInBingoLevel)
            {
                switch (isGlobalMessage)
                {
                    case true:
                    {
                        ChannelIndicator.text = "<color=orange>[ALL]</color>";
                        break;
                    }
                    case false:
                    {
                        ChannelIndicator.text = "<color=red>[TEAM]</color>";
                        break;
                    }
                }
            }
        }
    }

    public void clearChatHistory()
    {
        while (ChatHistory.Count > 0)
        {
            GameObject msg = ChatHistory.Dequeue();
            GameObject.Destroy(msg);
        }
    }

    public void SendChatMessage(string message)
    {
        ChatMessageSend cms = new ChatMessageSend();
        cms.isGlobal = isGlobalMessage;
        cms.username = sanitiseUsername(Steamworks.SteamClient.Name);
        cms.gameId = GameManager.CurrentGame.gameId;
        cms.steamId = Steamworks.SteamClient.SteamId.ToString();
        cms.chatMessage = message;
        cms.ticket = NetworkManager.CreateRegisterTicket();
        
        NetworkManager.SendEncodedMessage(JsonConvert.SerializeObject(cms));

    }

    public void handleWarning(ChatWarn warnLevel)
    {
        string message = "Offensive and derogatory terms are disallowed.";
        switch (warnLevel.warnLevel)
        {
            case 0:
            {
                message += "<color=orange>Further attempts at usage will result in a chat ban.</color>";
                break;
            }
            case 1:
            {
                message += "<color=red>THIS IS YOUR FINAL WARNING.</color>";
                break;
            }
            case 2:
            {
                message = "<color=red>Your access to Baphomet's Bingo chat has been revoked.</color>";
                break;
            }
        }

        ChatMessageReceive warnMessage = new ChatMessageReceive();
        warnMessage.username = "SYSTEM";
        warnMessage.message = message;
        updateChatHistory(warnMessage);
        if (warnLevel.warnLevel == 2)
        {
            messageContents.gameObject.SetActive(false);
            GameManager.canUseChat = false;
        }
    }
    
    public void updateChatHistory(ChatMessageReceive messageData)
    {
        if(ChatHistory.Count >= 4)
        {
            GameObject oldestMessage = ChatHistory.Dequeue();
            GameObject.Destroy(oldestMessage);
        }
        
        GameObject newMessage = GameObject.Instantiate(ChatTemplate, ChatTemplate.transform.parent);
        newMessage.name = "ChatMessage";

        string msg = "";
        if (messageData.username == "SYSTEM") { msg += "[<color=red>SYSTEM</color>] " + messageData.message; }
        else { msg += (messageData.channelType == 0 ? "[<color=orange>ALL</color>]" : "[<color="+GameManager.CurrentTeam.ToLower()+">TEAM</color>]") + messageData.username + ": " + messageData.message; }

        GetGameObjectChild(newMessage, "Contents").GetComponent<Text>().text = msg;
            
        newMessage.SetActive(true);
        ChatHistory.Enqueue(newMessage);
        
    }
}