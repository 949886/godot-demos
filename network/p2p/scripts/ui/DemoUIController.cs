using Godot;

public partial class DemoUIController : Control
{
    private Button _hostButton;
    private Button _joinButton;
    private LineEdit _addressInput;
    private Label _statusLabel;
    private VBoxContainer _playerList;
    private VBoxContainer _mainMenu;
    
    private NetworkManager _networkManager;
    
    public override void _Ready()
    {
        // Get UI references
        _mainMenu = GetNode<VBoxContainer>("MainMenu");
        _hostButton = GetNode<Button>("MainMenu/HostButton");
        _joinButton = GetNode<Button>("MainMenu/JoinButton");
        _addressInput = GetNode<LineEdit>("MainMenu/AddressInput");
        _statusLabel = GetNode<Label>("StatusLabel");
        _playerList = GetNode<VBoxContainer>("PlayerList");
        
        // Connect button signals
        _hostButton.Pressed += OnHostPressed;
        _joinButton.Pressed += OnJoinPressed;
        
        // Get manager references
        _networkManager = GetNode<NetworkManager>("/root/DemoScene/NetworkManager");
        
        // Connect to network events
        if (_networkManager != null)
        {
            _networkManager.ServerStarted += OnServerStarted;
            _networkManager.PlayerRegistered += OnPlayerRegistered;
        }
        
        Multiplayer.ConnectedToServer += OnClientConnected;
        Multiplayer.ConnectionFailed += OnClientDisconnected;
        Multiplayer.PeerConnected += OnPlayerJoined;
        Multiplayer.PeerDisconnected += OnPlayerLeft;
    }

    private void OnPlayerRegistered(int playerid)
    {
        AddPlayerToList(playerid);
    }

    private void OnHostPressed()
    {
        if (_networkManager == null) return;
        
        UpdateStatus("Starting server...");
        _hostButton.Disabled = true;
        _joinButton.Disabled = true;
        
        var result = _networkManager.StartServer();
        if (result != Error.Ok)
        {
            UpdateStatus($"Failed to start server: {result}");
            _hostButton.Disabled = false;
            _joinButton.Disabled = false;
        }
    }
    
    private void OnJoinPressed()
    {
        if (_networkManager == null) return;
        
        var address = _addressInput.Text.Trim();
        if (string.IsNullOrEmpty(address))
        {
            address = "127.0.0.1";
        }
        
        UpdateStatus($"Connecting to {address}...");
        _hostButton.Disabled = true;
        _joinButton.Disabled = true;
        
        var result = _networkManager.Connect(address);
        if (result != Error.Ok)
        {
            UpdateStatus($"Failed to connect: {result}");
            _hostButton.Disabled = false;
            _joinButton.Disabled = false;
        }
    }
    
    private void OnServerStarted()
    {
        UpdateStatus("Server started! Waiting for players...");
        _mainMenu.Visible = false;
        
        // Add local player to list
        AddPlayerToList(GetMultiplayer().GetUniqueId());
    }
    
    private void OnClientConnected()
    {
        UpdateStatus("Connected to server!");
        _mainMenu.Visible = false;
    }
    
    private void OnClientDisconnected()
    {
        UpdateStatus("Disconnected from server");
        _mainMenu.Visible = true;
        _hostButton.Disabled = false;
        _joinButton.Disabled = false;
        
        // Clear player list
        ClearPlayerList();
    }
    
    private void OnPlayerJoined(long playerId)
    {
        GD.Print($"UI: Player {playerId} joined, Local: {playerId == GetMultiplayer().GetUniqueId()}");
        
        if (playerId == GetMultiplayer().GetUniqueId())
        {
            AddPlayerToList(playerId);
        }
        else
        {
            GD.Print($"UI: Skipping local player {playerId} for player list");
        }
    }
    
    private void OnPlayerLeft(long playerId)
    {
        RemovePlayerFromList(playerId);
    }
    
    private void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
        GD.Print($"Status: {message}");
    }
    
    private void AddPlayerToList(long playerId)
    {
        var playerLabel = new Label();
        playerLabel.Name = $"Player_{playerId}";
        playerLabel.Text = $"{playerLabel.Name} (ID: {playerId})";
        _playerList.AddChild(playerLabel);
    }
    
    private void RemovePlayerFromList(long playerId)
    {
        var playerNode = _playerList.GetNodeOrNull($"Player_{playerId}");
        if (playerNode != null)
        {
            playerNode.QueueFree();
        }
    }
    
    private void ClearPlayerList()
    {
        foreach (Node child in _playerList.GetChildren())
        {
            if (child.Name != "PlayerListTitle")
            {
                child.QueueFree();
            }
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        // ESC to disconnect and return to menu
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_networkManager != null && _networkManager.IsConnected)
            {
                _networkManager.Disconnect();
            }
        }
    }
} 