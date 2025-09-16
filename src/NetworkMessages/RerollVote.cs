using UltraBINGO.Components;
using UltrakillBingoClient;
using static UltraBINGO.CommonFunctions;

namespace UltraBINGO.NetworkMessages;

public class RerollRequest : SendMessage
{
    public new string messageType = "RerollRequest";
        
    public RegisterTicket steamTicket;
    public string steamId;
    public int gameId;
    public int row;
    public int column;
}

public class RerollExpireNotification : MessageResponse
{
    public string mapName;
}

public static class RerollExpireNotificationHandler
{
    public static void handle(RerollExpireNotification response)
    {
        string msg = "Vote to reroll <color=orange>" + response.mapName + "</color> has expired.";
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
    }
}

public class RerollVoteNotification : MessageResponse
{
    public string mapName;
    public string voteStarter;
    public string voteStarterSteamId;
    public string numVotes;
    public int votesRequired;
    public int notifType;
    public int timer;
}

public static class RerollVoteNotificationHandler
{
    public static void handle(RerollVoteNotification response)
    {
        
        string type = (response.notifType == 0 ? " started a vote to reroll " : " voted to reroll ");
        
        string msg = response.voteStarter + type + "<color=orange>" + response.mapName + "</color>. ("+response.numVotes+"/"+response.votesRequired+" votes)";
        
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
        
        if(response.notifType == 0)
        {
            MonoSingleton<BingoVoteManager>.Instance.Start();
            MonoSingleton<BingoVoteManager>.Instance.startVote(response.voteStarterSteamId,response.timer+1,response.mapName,response.votesRequired);
        }
        else
        {
            MonoSingleton<BingoVoteManager>.Instance.addVote();
        }
        
        GameManager.voteData = new VoteData(true,MonoSingleton<BingoVoteManager>.Instance.hasVoted,MonoSingleton<BingoVoteManager>.Instance.voteThreshold,MonoSingleton<BingoVoteManager>.Instance.currentVotes,MonoSingleton<BingoVoteManager>.Instance.map,MonoSingleton<BingoVoteManager>.Instance.timeRemaining);
    }
}

public class RerollSuccessNotification : MessageResponse
{
    public string oldMapId;
    public string oldMapName;
    public GameLevel mapData;
    
    public int locationX;
    public int locationY;
}

public static class RerollSuccessNotificationHandler
{
    public static void handle(RerollSuccessNotification response)
    {
        MonoSingleton<BingoVoteManager>.Instance.stopVote();
        string msg = "VOTE SUCCESSFUL - <color=orange>"+ response.oldMapName + " </color>has rerolled to <color=orange>" +response.mapData.levelName+"</color>.";
        if(response.oldMapId == getSceneName()) {msg += "\nChanging levels in 5 seconds...";}
        MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage(msg);
        GameManager.SwapRerolledMap(response.oldMapId,response.mapData,response.locationX,response.locationY);
    }
}