﻿using System.Collections;
using System.Threading.Tasks;
using UltraBINGO.UI_Elements;
using UltrakillBingoClient;

using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class DisconnectSignal : MessageResponse
{
    public int disconnectCode;
    public string disconnectMessage;
}

public static class DisconnectSignalHandler
{
    public static async void handle(DisconnectSignal response = null)
    {
        Logging.Message("Disconnect message");
        //If the player is in-game, warn them of returning to menu in 5 seconds.
        if(getSceneName() == "Main Menu")
        {
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Disconnected.");
            BingoEncapsulator.BingoCardScreen.SetActive(false);
            BingoEncapsulator.BingoLobbyScreen.SetActive(false);
            BingoEncapsulator.BingoEndScreen.SetActive(false);
            BingoEncapsulator.BingoMenu.SetActive(true);
        }
        else
        {
            GameManager.returningFromBingoLevel = false;
            GameManager.isInBingoLevel = false;
            GameManager.CurrentGame = null;
            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Disconnected.\nLeaving mission in 5 seconds.");
            await Task.Delay(5000);
            SceneHelper.LoadScene("Main Menu");
        }
    }
}

public class DisconnectNotification : MessageResponse
{
    public string username;
}

public static class DisconnectNotificationHandler
{
    public static void handle(DisconnectNotification response)
    {
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(response.username + " has left the game.");
    }
}