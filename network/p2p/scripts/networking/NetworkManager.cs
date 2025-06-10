using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NetworkManager : Node
{
    public static NetworkManager Instance { get; private set; }
    
    [Signal] public delegate void ServerStartedEventHandler();
    [Signal] public delegate void PlayerRegisteredEventHandler(int playerId);
    [Signal] public delegate void PlayerUnregisteredEventHandler(int playerId);
    
    [Export] public int DefaultPort { get; set; } = 7000;
    [Export] public int MaxClients { get; set; } = 32;
    [Export] public string DefaultServerAddress { get; set; } = "127.0.0.1";
    
    
    private MultiplayerSpawner _playerSpawner;
    
    public readonly Dictionary<int, Player> players = new();
    private List<Player> PlayerList => players.Values.ToList();
    
    public Player LocalPlayer => players.ContainsKey(Multiplayer.GetUniqueId()) ? players[Multiplayer.GetUniqueId()] : null;
    public bool IsServer { get; private set; }
    public bool IsClient => !IsServer && GetMultiplayer().HasMultiplayerPeer();
    public new bool IsConnected => GetMultiplayer().HasMultiplayerPeer();
    
    
    public override void _Ready()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            QueueFree();
            return;
        }
        
        _playerSpawner = GetTree().CurrentScene.GetNodeOrNull<MultiplayerSpawner>("MultiplayerSpawner");
        _playerSpawner.Spawned += OnPlayerSpawned;
        _playerSpawner.Despawned += OnPlayerDespawned;
        
        // Don't auto-free on scene changes
        ProcessMode = ProcessModeEnum.Always;
        
        GD.Print("NetworkManager initialized");
        
        // Connect multiplayer events
        GetMultiplayer().PeerConnected += OnPeerConnected;
        GetMultiplayer().PeerDisconnected += OnPeerDisconnected;
        GetMultiplayer().ConnectedToServer += OnConnectedToServer;
        GetMultiplayer().ConnectionFailed += OnConnectionFailed;
        GetMultiplayer().ServerDisconnected += OnServerDisconnected;
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Disconnect();
        }
    }
    
    public Error StartServer(int port = -1)
    {
        if (port == -1)
            port = DefaultPort;
            
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, MaxClients);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to start server: {error}");
            return error;
        }
        
        GetMultiplayer().MultiplayerPeer = peer;
        IsServer = true;
        
        GD.Print($"Server started on port {port}");
        EmitSignal(SignalName.ServerStarted);
        
        // Notify GameManager to spawn host player
        var playerId = GetTree().GetMultiplayer().GetUniqueId();
        RegisterPlayer(playerId, $"Player {playerId}");
        
        return Error.Ok;
    }
    
    // Connect to an existing server as a client
    public Error Connect(string address = "", int port = -1)
    {
        if (string.IsNullOrEmpty(address))
            address = DefaultServerAddress;
        if (port == -1) port = DefaultPort;
            
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(address, port);
        
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to connect to server: {error}");
            return error;
        }
        
        GetMultiplayer().MultiplayerPeer = peer;
        IsServer = false;
        
        GD.Print($"Attempting to connect to {address}:{port}");
        
        return Error.Ok;
    }
    
    // Disconnect from the server or stop the server
    public void Disconnect()
    {
        GetMultiplayer().MultiplayerPeer = null;
        IsServer = false;
        
        GD.Print("Disconnected from server.");
    }
    
    public Player RegisterPlayer(int playerId, string playerName)
    {
        var playerData = new Player
        {
            Id = playerId,
            Name = playerName,
            JoinTime = DateTime.Now
        };
        
        players[playerId] = playerData;
        EmitSignal(SignalName.PlayerRegistered, playerId);
        
        GD.Print($"Player registered: {playerName} (ID: {playerId})");
        
        // Spawn the player if we're the server
        // Player in client will be spawned by the `MultiplayerSpawner`.
        if (GetMultiplayer().IsServer())
        {
            var spawnPosition = new Vector3(players.Count, 0, 0);
            var instance = SpawnPlayer<AvatarController>(playerId, spawnPosition);
            playerData.Instance = instance;
        }

        return playerData;
    }
    
    public void UnregisterPlayer(int playerId)
    {
        if (players.TryGetValue(playerId, out var playerData))
        {
            players.Remove(playerId);
            EmitSignal(SignalName.PlayerUnregistered, playerId);
            
            GD.Print($"Player {playerData.Name} (ID: {playerId}) unregistered.");
        }
    }
    
    public T SpawnPlayer<T>(int playerId, Vector3 spawnPosition = default) where T : Node
    {
        GD.Print($"Spawn player {playerId} at {spawnPosition}");

        // Spawn the player
        var playerScene = GD.Load<PackedScene>("res://scenes/player/Player.tscn");
        var playerInstance = playerScene.Instantiate<T>();
        playerInstance.Name = "Player " + playerId;
        
        // Add to scene first, then set position
        GetTree().CurrentScene.AddChild(playerInstance, true);

        return playerInstance;
    }

    private void OnPlayerSpawned(Node node)
    {
        if (node is AvatarController instance)
        {
            var player = RegisterPlayer(instance.ID, $"Player {instance.ID}");
            player.Instance = instance;
            
            GD.Print($"Player {instance.Name} spawned.");
        }
    }
    
    private void OnPlayerDespawned(Node node)
    {
        
    }
    
    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer connected: {id}");
        
        if (IsServer)
        {
            RegisterPlayer((int)id, $"Player {id}");
        }
    }
    
    private void OnPeerDisconnected(long id)
    {
        GD.Print($"Peer disconnected: {id}");
        
        UnregisterPlayer((int)id);
    }
    
    private void OnConnectedToServer()
    {
        GD.Print("Connected to server successfully");
    }
    
    private void OnConnectionFailed()
    {
        GD.PrintErr("Failed to connect to server");
        Disconnect();
    }
    
    private void OnServerDisconnected()
    {
        GD.Print("Server disconnected");
        Disconnect();
    }
} 