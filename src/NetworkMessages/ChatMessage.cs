using UltraBINGO.Components;
using UltrakillBingoClient;

namespace UltraBINGO.NetworkMessages;

public class ChatMessageSend : SendMessage
{
    public new string messageType = "ChatMessage";

    public bool isGlobal;
    public string username;
    public int gameId;
    public string steamId;
    public string chatMessage;
    public RegisterTicket ticket;
}

public class ChatMessageReceive : MessageResponse
{
    public string username;
    public bool isGlobal;
    public string message;
    public int channelType;
}

public static class ChatMessageReceiveHandler
{
    public static void handle(ChatMessageReceive response)
    {
        MonoSingleton<BingoChatManager>.Instance.updateChatHistory(response);
    }
}

public class ChatWarn : MessageResponse
{
    public int warnLevel;
}

public static class ChatWarnHandler
{
    public static void handle(ChatWarn response)
    {
        MonoSingleton<BingoChatManager>.Instance.handleWarning(response);
    }
}