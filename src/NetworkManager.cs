using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using BepInEx.Configuration;
using Newtonsoft.Json;
using UltraBINGO;
using UltraBINGO.NetworkMessages;
using UltraBINGO.UI_Elements;
using WebSocketSharp;

using static UltraBINGO.CommonFunctions;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
using Object = System.Object;

namespace UltrakillBingoClient;

public enum State
{
    NORMAL,
    INMENU,
    INLOBBY,
    INBROWSER,
    INGAME
}

public enum AsyncAction
{
    None,
    Host,
    Join,
    ModCheck,
    RetrySend,
    ReconnectGame,
    FetchGames
}

public class SendMessage
{
    public string messageType;
}

public class MessageResponse
{
    public string messageType;
}
public class PlayerNotification
{
    public string messageType;
}

public static class NetworkManager
{
    public static AsyncAction pendingAction = AsyncAction.None;
    public static string pendingPassword = "";
    public static VerifyModRequest pendingVmr = null;
    public static string QueuedMessage = "";
    public static State currentState = State.NORMAL;
    
    public static ConfigEntry<string> serverURLConfig;
    public static ConfigEntry<string> serverPortConfig;
    public static ConfigEntry<string> lastRankUsedConfig;
    
    public static string serverURL;
    
    private static readonly HttpClient Client = new HttpClient();
    
    public static string serverCatalogURL;
    public static string serverMapPoolCatalogURL;
    
    public static bool modlistCheckDone = false;
    public static bool modlistCheckPassed = false;
    private static string steamTicket;
    
    public static string requestedRank = "";
    
    static WebSocket ws;
    static Timer heartbeatTimer;
    
    public static int maxReconnectionAttempts = 3;
    public static int currentReconnection = 0;
    
    
    public static void setState(State newState)
    {
        currentState = newState;
    }
    
    public static void HandleAsyncConnect()
    {
        SetupHeartbeat();
        switch (pendingAction)
        {
            case AsyncAction.Host:
            {
                CreateRoom();
                break;  
            }

            case AsyncAction.Join:
            {
                JoinGame(pendingPassword);
                break; 
            }

            case AsyncAction.ModCheck:
            {
                SendEncodedMessage(JsonConvert.SerializeObject(pendingVmr));
                break;
            }

            case AsyncAction.RetrySend:
            {
                SendEncodedMessage(JsonConvert.SerializeObject(QueuedMessage));
                QueuedMessage = "";
                break;
            }

            case AsyncAction.ReconnectGame:
            {
                Logging.Warn("Requesting to reconnect");
                ReconnectRequest rr = new ReconnectRequest();
                rr.steamId = Steamworks.SteamClient.SteamId.ToString();
                rr.roomId = GameManager.CurrentGame.gameId;
                rr.ticket = CreateRegisterTicket();
                SendEncodedMessage(JsonConvert.SerializeObject(rr));
                break;
            }
            
            case AsyncAction.FetchGames:
            {
                FetchGamesRequest fgr = new FetchGamesRequest();
                SendEncodedMessage(JsonConvert.SerializeObject(fgr));
                break;
            }
                
            default: break;
        }
    }
    
    public static void SendModCheck(VerifyModRequest vmr)
    {
        pendingAction = AsyncAction.ModCheck;
        pendingVmr = vmr;
        ConnectWebSocket();
    }
    
    public static string GetSteamTicket()
    {
        return steamTicket;
    }
    public static void SetSteamTicket(string ticket)
    {
        steamTicket = ticket;
    }
    
    //Fetch the bingo map catalog from the server.
    public static async Task<string> FetchCatalog(string urlToRequest)
    {
        string url = urlToRequest;
        try
        {
            string responseTomlRaw = await Client.GetStringAsync(url);
            return responseTomlRaw;
        }
        catch (Exception e)
        {
            Logging.Error("Something went wrong while fetching from the URL");
            Logging.Error(e.Message);
            return null;
        }
    }
    
    //Check if the WebSocket connection to the server is active and alive.
    public static bool IsConnectionUp()
    {
        return ws.IsAlive;
    }
    
    //Init and setup the WebSocket connection.
    public static void Initialise(string url, string port,bool isDev=false)
    {
        serverURL = isDev ? "ws://127.0.0.1:2052" : "ws://" + url + ":" + port;
        
        serverMapPoolCatalogURL = isDev ? "http://127.0.0.1/bingoMapPool.toml" : "http://"+ url + "/bingoMapPool.toml";
        
        serverCatalogURL = isDev ? "http://127.0.0.1/bingoCatalog.toml" : "http://"+ url + "/bingoCatalog.toml"; 

        ws = new WebSocket (serverURL);
        ws.EnableRedirection = true;
        ws.WaitTime = TimeSpan.FromSeconds(15);
        
        ws.OnOpen += (sender,e) => { HandleAsyncConnect(); };
        ws.OnMessage += (sender,e) => { onMessageRecieved(e); };
        ws.OnError += (sender,e) => { HandleError(e); };
        ws.OnClose += (sender,e) =>
        {
            if(e.WasClean) { Logging.Message("Disconnected cleanly from server"); }
            else
            {
                Logging.Error("Network connection error.");
                Logging.Error(e.Reason);
            }
        };
    }
    
    public static async void TryReconnect()
    {
        pendingAction = AsyncAction.ReconnectGame;
        ConnectWebSocket();
    }
    
    //Handle any errors that happen with the WebSocket connection.
    public static async void HandleError(ErrorEventArgs e)
    {
        Logging.Error("Network error happened");
        if(ws.IsAlive)
        {
            ws.CloseAsync();
        }
        
        else
        {
            // If player is on main menu
            switch(currentState)
            {
                case State.NORMAL:
                {
                    Logging.Error("Unable to perform startup connection to server");
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to connect to Baphomet's Bingo server.");
                    break;
                }
                
                case State.INMENU:
                {
                    Logging.Error("Unable to connect to server to join/host game");
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to contact server.");
                    BingoMainMenu.UnlockUI();
                    break;
                }
                case State.INLOBBY:
                {
                    Logging.Error("Network error happened in lobby");
                    Logging.Error(e.Message);
                    MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connection to the lobby was lost. Returning to menu.");
                    
                    GameManager.ClearGameVariables();
                    setState(State.INMENU);
            
                    BingoEncapsulator.BingoCardScreen.SetActive(false);
                    BingoEncapsulator.BingoLobbyScreen.SetActive(false);
                    BingoEncapsulator.BingoEndScreen.SetActive(false);
                    BingoEncapsulator.BingoMenu.SetActive(true);
                    
                    BingoLobby.UnlockUI();
                    
                    break;
                }
                case State.INGAME:
                {
                    Logging.Error("Network error happened in-game");
                    //Handle in-game reconnection attempts here
                    if(GameManager.IsInBingoLevel)
                    {
                        currentReconnection++;
            
                        if(currentReconnection > maxReconnectionAttempts)
                        {
                            currentReconnection = 0;

                            GameManager.ClearGameVariables();                
                            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Failed to reconnect. Exitting game in 5 seconds.");
                            NetworkManager.setState(State.NORMAL);
                
                            await Task.Delay(5000);
                            SceneHelper.LoadScene("Main Menu");
                
                        }
                        else
                        {
                            MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("Connection to the game was lost.\nAttempting reconnection... <color=orange>("+currentReconnection+"/"+maxReconnectionAttempts+")</color>");
                            await Task.Delay(2000);
            
                            TryReconnect();
                        }
                    }
                    break;
                }
                case State.INBROWSER:
                {
                    Logging.Error("Network error happened in-browser");
                    if(!BingoBrowser.fetchDone) {BingoBrowser.DisplayError();}
                    else {BingoBrowser.UnlockUI();}
                    
                    break;
                }
                default: break;
            }
        }
    }
    
    //Decode base64 messages recieved from the server.
    public static string DecodeMessage(string encodedMessage)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(encodedMessage);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
    
    //Encode and send base64 messages to the server.
    public static void SendEncodedMessage(string jsonToEncode)
    {
        if(!ws.IsAlive)
        {
            Logging.Warn("Queuing message & retrying connection");
            QueuedMessage = jsonToEncode;
            pendingAction = AsyncAction.RetrySend;
            ws.ConnectAsync();
            return;
        }
        
        byte[] encodedBytes = System.Text.Encoding.UTF8.GetBytes(jsonToEncode);
        string encodedJson = System.Convert.ToBase64String(encodedBytes);
        
        ws.SendAsync(encodedJson,new Action<bool>((completed) =>
        {
            if(!completed){Logging.Warn( "Async not sent");}
        }));
    }
    
    public static RegisterTicket CreateRegisterTicket()
    {
        RegisterTicket rt = new RegisterTicket();
        rt.steamId = Steamworks.SteamClient.SteamId.ToString();
        rt.steamTicket = GetSteamTicket();
        rt.steamUsername = Steamworks.SteamClient.Name;
        rt.gameId = GameManager.CurrentGame.gameId;
        
        return rt;
    }
    
    public static void RegisterConnection()
    {
        Logging.Message("Registering connection with server");
        RegisterTicket rt = CreateRegisterTicket();
        SendEncodedMessage(JsonConvert.SerializeObject(rt));
    }
    
    //Connect the WebSocket to the server.
    public static void ConnectWebSocket()
    {
        if(!ws.IsAlive)
        {
            ws.ConnectAsync();
        }
    }
    
    //Disconnect WebSocket.
    public static void DisconnectWebSocket(ushort code=1000,string reason="Disconnect reason not specified")
    {
        ws.Close(code,reason);
    }
    
    //Setup WebSocket heartbeat.
    public static void SetupHeartbeat()
    {
        heartbeatTimer = new Timer(10*1000); //Ping once every 10 seconds
        heartbeatTimer.Elapsed += SendPing;
        heartbeatTimer.AutoReset = true;
        heartbeatTimer.Enabled = true;
    }
    
    //Ping the WebSocket server to keep the connection alive.
    public static void SendPing(Object source, ElapsedEventArgs e)
    {
        ws.Ping();
    }
    
    //Create a new bingo game room.
    public static void CreateRoom()
    {
        CreateRoomRequest crr = new CreateRoomRequest();
        
        crr.roomName = "TestRoom";
        crr.roomPassword = "password";
        crr.maxPlayers = 8;
        crr.pRankRequired = false;
        
        crr.hostSteamName = sanitiseUsername(Steamworks.SteamClient.Name);
        crr.hostSteamId = Steamworks.SteamClient.SteamId.ToString();
        crr.rank = (GameManager.hasRankAccess ? requestedRank : "");
        
        SendEncodedMessage(JsonConvert.SerializeObject(crr));
    }
    
    public static void JoinGame(string password)
    {
        JoinRoomRequest jrr = new JoinRoomRequest();
        jrr.password = password;
        jrr.username = sanitiseUsername(Steamworks.SteamClient.Name);
        jrr.steamId = Steamworks.SteamClient.SteamId.ToString();
        jrr.rank = (GameManager.hasRankAccess ? requestedRank : "");
        SendEncodedMessage(JsonConvert.SerializeObject(jrr));
    }
    
    public static void SendStartGameSignal(int roomId)
    {
        StartGameRequest gameRequest = new StartGameRequest();
        gameRequest.roomId = roomId;
        gameRequest.ticket = CreateRegisterTicket();
        gameRequest.selectedMapIds = BingoMapBrowser.selectedLevels;
        
        SendEncodedMessage(JsonConvert.SerializeObject(gameRequest));
    }
    
    public static void SubmitRun(SubmitRunRequest srr)
    {
        SendEncodedMessage(JsonConvert.SerializeObject(srr));
    }
    
    public static void SendLeaveGameRequest(int roomId)
    {
        LeaveGameRequest leaveRequest = new LeaveGameRequest();
        leaveRequest.username = sanitiseUsername(Steamworks.SteamClient.Name);
        leaveRequest.steamId = Steamworks.SteamClient.SteamId.ToString();
        leaveRequest.roomId = roomId;
        
        SendEncodedMessage(JsonConvert.SerializeObject(leaveRequest));
    }
    
    public static void KickPlayer(string steamId)
    {
        KickPlayer kp = new KickPlayer();
        kp.gameId = GameManager.CurrentGame.gameId;
        kp.playerToKick = steamId;
        kp.ticket = CreateRegisterTicket();
        
        SendEncodedMessage(JsonConvert.SerializeObject(kp));
    }
    
    public static void RequestGames()
    {
        pendingAction = AsyncAction.FetchGames;
        ws.ConnectAsync();
    }
    
    
    //Handle all incoming messages received from the server.
    public static void onMessageRecieved(MessageEventArgs e)
    {
        Main.Queue(() =>
        {
            EncapsulatedMessage em = JsonConvert.DeserializeObject<EncapsulatedMessage>(DecodeMessage(e.Data));
        switch(em.header)
        {
            case "CreateRoomResponse":
            {
                CreateRoomResponse response = JsonConvert.DeserializeObject<CreateRoomResponse>(em.contents);
                CreateRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomResponse":
            {
                JoinRoomResponse response = JsonConvert.DeserializeObject<JoinRoomResponse>(em.contents);
                JoinRoomResponseHandler.handle(response);
                break;
            }
            case "JoinRoomNotification":
            {
                PlayerJoiningMessage response = JsonConvert.DeserializeObject<PlayerJoiningMessage>(em.contents);
                PlayerJoiningResponseHandler.handle(response);
                break;
            }
            case "UpdateTeamsNotif":
            {
                UpdateTeamsNotification response = JsonConvert.DeserializeObject<UpdateTeamsNotification>(em.contents);
                UpdateTeamsNotificationHandler.handle(response);
                break;
            }
            case "RoomUpdate":
            {
                Logging.Message("Room settings have updated");
                UpdateRoomSettingsNotification response = JsonConvert.DeserializeObject<UpdateRoomSettingsNotification>(em.contents);
                UpdateRoomSettingsHandler.handle(response);
                break;
            }
            case "StartGame":
            {
                StartGameResponse sgr = JsonConvert.DeserializeObject<StartGameResponse>(em.contents);
                StartGameResponseHandler.handle(sgr);
                break;
            }
            case "LevelClaimed":
            {
                LevelClaimNotification response = JsonConvert.DeserializeObject<LevelClaimNotification>(em.contents);
                LevelClaimHandler.handle(response);
                break;
            }
            case "ServerDisconnection":
            {
                DisconnectSignal response = JsonConvert.DeserializeObject<DisconnectSignal>(em.contents);
                DisconnectSignalHandler.handle(response);
                break;
            }
            case "DisconnectNotification":
            {
                DisconnectNotification response = JsonConvert.DeserializeObject<DisconnectNotification>(em.contents);
                DisconnectNotificationHandler.handle(response);
                break;
            }
            case "TimeoutNotification":
            {
                TimeoutSignal response = JsonConvert.DeserializeObject<TimeoutSignal>(em.contents);
                TimeoutSignalHandler.handle(response);
                break;
            }
            case "NewHostNotification":
            {
                HostMigration response = JsonConvert.DeserializeObject<HostMigration>(em.contents);
                HostMigrationHandler.handle(response);
                break;
            }
            case "ReconnectResponse":
            {
                ReconnectResponse response = JsonConvert.DeserializeObject<ReconnectResponse>(em.contents);
                ReconnectResponseHandler.handle(response);
                break;
            }
            case "GameEnd":
            {
                Logging.Message("Game over!");
                EndGameSignal response = JsonConvert.DeserializeObject<EndGameSignal>(em.contents);
                EndGameSignalHandler.handle(response);
                break;
            }
            case "CheatNotification":
            {
                CheatNotification response = JsonConvert.DeserializeObject<CheatNotification>(em.contents);
                CheatNotificationHandler.handle(response);
                break;
            }
            case "ModVerificationResponse":
            {
                ModVerificationResponse response = JsonConvert.DeserializeObject<ModVerificationResponse>(em.contents);
                ModVerificationHandler.handle(response);
                break;
            }
            case "KickNotification":
            {
                KickNotification response =JsonConvert.DeserializeObject<KickNotification>(em.contents);
                KickNotificationHandler.handle(response);
                break;
            }
            case "Kicked":
            {
                MonoSingleton<HudMessageReceiver>.Instance.SendHudMessage("You were kicked from the game.");
                NetworkManager.DisconnectWebSocket();
                KickHandler.handle();
                break;
            }
            case "FetchGamesResponse":
            {
                FetchGamesResponse response = JsonConvert.DeserializeObject<FetchGamesResponse>(em.contents);
                FetchGamesReponseHandler.handle(response);
                break;
            }
            case "RerollVote":
            {
                RerollVoteNotification response = JsonConvert.DeserializeObject<RerollVoteNotification>(em.contents);
                RerollVoteNotificationHandler.handle(response);
                break;
            }
            case "RerollSuccess":
            {
                RerollSuccessNotification response = JsonConvert.DeserializeObject<RerollSuccessNotification>(em.contents);
                RerollSuccessNotificationHandler.handle(response);
                break;
            }
            case "RerollExpire":
            {
                RerollExpireNotification response = JsonConvert.DeserializeObject<RerollExpireNotification>(em.contents);
                RerollExpireNotificationHandler.handle(response);
                break;
            }
            case "MapPing":
            {
                MapPingNotification response = JsonConvert.DeserializeObject<MapPingNotification>(em.contents);
                MapPingNotificationHandler.handle(response);
                break;
            }
            case "Pong":
            {
                //No need to do anything here, ping/pong just keeps the connection alive
                break;
            }
            case "ChatMessage":
            {
                ChatMessageReceive response = JsonConvert.DeserializeObject<ChatMessageReceive>(em.contents);
                ChatMessageReceiveHandler.handle(response);
                break;
            }
            case "ChatWarn":
            {
                ChatWarn response = JsonConvert.DeserializeObject<ChatWarn>(em.contents);
                ChatWarnHandler.handle(response);
                break;
            }
            case "MapPools":
            {
                MapPoolResponse response = JsonConvert.DeserializeObject<MapPoolResponse>(em.contents);
                MapPoolResponseHandler.handle(response);
                break;
            }
            default: {Logging.Warn("Unknown or unimplemented packet received from server ("+em.header+"), discarding");break;}
        } });
    }
}