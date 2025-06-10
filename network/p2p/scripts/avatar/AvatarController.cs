using Godot;

public partial class AvatarController : CharacterBody3D
{
    [Signal] public delegate void AvatarLoadedEventHandler();
    
    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public float LookUpLimit { get; set; } = 90.0f;
    [Export] public float LookDownLimit { get; set; } = -90.0f;

    // Synchronization data
    [Export]
    public Vector3 NetworkPosition
    {
        get => _networkPosition;
        set => _networkPosition = value;
    }

    [Export] public Vector3 NetworkRotation { get; set; }
    [Export] public Vector3 NetworkCameraRotation { get; set; }
    [Export] public bool NetworkIsWalking { get; set; }
    [Export] public bool NetworkIsRunning { get; set; }
    [Export] public bool NetworkIsJumping { get; set; }
    
    private Camera3D _camera;
    private Node3D _cameraContainer;
    private AnimationPlayer _animationPlayer;
    private MultiplayerSynchronizer _synchronizer;
    private Node3D _avatarModel;
    
    private Vector3 _velocity;
    private float _cameraRotationX = 0.0f;
    private bool _isLocalPlayer = false;
    
    // Animation states
    private bool _isWalking = false;
    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isInAir = false;
    private Vector3 _networkPosition;


    public int ID { get; private set; } = -1;
    

    public override void _EnterTree()
    {
        base._EnterTree();
        
        var idstr = Name.ToString().Substring(7);
        ID = int.Parse(idstr);
        
        SetMultiplayerAuthority(ID);
        _isLocalPlayer = Multiplayer.GetUniqueId() == ID;
        
        GD.Print($"Avatar {Name} entered tree with ID: {ID} {_isLocalPlayer}");
    }

    public override void _Ready()
    {
        SetupAvatar();
        
        // if (!GetMultiplayer().IsServer())
        // {
        //     _synchronizer.SetProcess(false);
        // }
        Position += GD.Randf() * Vector3.One;
    }
    
    private void SetupAvatar()
    {
        // Get camera container and camera from scene
        _cameraContainer = GetNodeOrNull<Node3D>("CameraContainer");
        if (_cameraContainer != null)
        {
            _camera = _cameraContainer.GetNodeOrNull<Camera3D>("Camera3D");
        }
        
        // Get multiplayer synchronizer from scene
        _synchronizer = GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
        
        // Only enable camera and input for local player
        if (_camera != null)
        {
            if (_isLocalPlayer)
            {
                _camera.Current = true;
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            else
            {
                _camera.Current = false;
            }
        }
        
        GD.Print($"Avatar setup complete for player {ID} (Local: {_isLocalPlayer})");
        EmitSignal(SignalName.AvatarLoaded);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        // GD.Print($"Avatar {ID} Physics Process: {Position}");
        if (!_isLocalPlayer) 
        {
            // Interpolate remote player positions
            InterpolateRemotePlayer(delta);
            return;
        }
        
        if (!IsMultiplayerAuthority())
            return;
        
        // Handle gravity
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
            _isInAir = true;
        }
        else
        {
            if (_isInAir)
            {
                _isInAir = false;
                _isJumping = false;
            }
        }
        
        // Handle input
        HandleMovementInput(delta);
        HandleJumpInput();
        
        // Move and slide
        MoveAndSlide();
        
        // Update network data
        UpdateNetworkData();
        
        // Update animations
        UpdateAnimations();
    }
    
    private void HandleMovementInput(double delta)
    {
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        
        bool isShiftPressed = Input.IsActionPressed("run");
        float currentSpeed = isShiftPressed ? Speed * 1.5f : Speed;
        
        _isWalking = direction != Vector3.Zero;
        _isRunning = _isWalking && isShiftPressed;
        
        if (direction != Vector3.Zero)
        {
            Velocity = new Vector3(direction.X * currentSpeed, Velocity.Y, direction.Z * currentSpeed);
        }
        else
        {
            Velocity = new Vector3(Mathf.MoveToward(Velocity.X, 0, currentSpeed), Velocity.Y, Mathf.MoveToward(Velocity.Z, 0, currentSpeed));
        }
    }
    
    private void HandleJumpInput()
    {
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
            _isJumping = true;
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (!_isLocalPlayer) return;
        
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Rotate camera container (Y axis - left/right)
            if (_cameraContainer != null)
            {
                _cameraContainer.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
                
                // Rotate camera (X axis - up/down)
                _cameraRotationX += -mouseMotion.Relative.Y * MouseSensitivity;
                _cameraRotationX = Mathf.Clamp(_cameraRotationX, Mathf.DegToRad(LookDownLimit), Mathf.DegToRad(LookUpLimit));
                
                if (_camera != null)
                {
                    _camera.Rotation = new Vector3(_cameraRotationX, 0, 0);
                }
            }
        }
        
        // Toggle mouse capture with ESC
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }
    
    private void UpdateNetworkData()
    {
        if (!_isLocalPlayer) return;
        
        NetworkPosition = GlobalPosition;
        NetworkRotation = GlobalRotation;
        if (_cameraContainer != null)
        {
            NetworkCameraRotation = _cameraContainer.Rotation;
        }
        NetworkIsWalking = _isWalking;
        NetworkIsRunning = _isRunning;
        NetworkIsJumping = _isJumping;
    }
    
    private void InterpolateRemotePlayer(double delta)
    {
        // Smoothly interpolate to network position
        float lerpWeight = (float)(delta * 10.0); // Adjust interpolation speed as needed
        GlobalPosition = GlobalPosition.Lerp(NetworkPosition, lerpWeight);
        GlobalRotation = GlobalRotation.Lerp(NetworkRotation, lerpWeight);
        
        if (_cameraContainer != null)
        {
            _cameraContainer.Rotation = _cameraContainer.Rotation.Lerp(NetworkCameraRotation, lerpWeight);
        }
        
        // Update animation states
        _isWalking = NetworkIsWalking;
        _isRunning = NetworkIsRunning;
        _isJumping = NetworkIsJumping;
        
        UpdateAnimations();
    }
    
    private void UpdateAnimations()
    {
        // TODO: Implement animation logic when animation player is available
        // For now, we can change material color based on movement state
        var meshInstance = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        if (meshInstance?.MaterialOverride is StandardMaterial3D material)
        {
            if (_isRunning)
            {
                material.AlbedoColor = Colors.Red; // Running
            }
            else if (_isWalking)
            {
                material.AlbedoColor = Colors.Yellow; // Walking
            }
            else
            {
                material.AlbedoColor = _isLocalPlayer ? Colors.Green : Colors.Blue; // Idle
            }
        }
    }
    
    public void LoadCustomAvatar(string avatarPath)
    {
        GD.Print($"Loading custom avatar from: {avatarPath}");
        // TODO: Implement VRM/GLB/GLTF avatar loading
        
        if (!FileAccess.FileExists(avatarPath))
        {
            GD.PrintErr($"Avatar file not found: {avatarPath}");
            return;
        }
        
        // For now, keep the default avatar
        // In the future, this will load VRM files or other 3D model formats
        // and replace the current avatar model
        
        EmitSignal(SignalName.AvatarLoaded);
    }
    
    public void SetPlayerName(string playerName)
    {
        // TODO: Display player name above avatar
        GD.Print($"Player name set to: {playerName}");
        
        // Create a label above the avatar for the player name
        var nameLabel = GetNodeOrNull<Label3D>("NameLabel");
        if (nameLabel == null)
        {
            nameLabel = new Label3D();
            nameLabel.Name = "NameLabel";
            nameLabel.Position = new Vector3(0, 2.5f, 0);
            nameLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            AddChild(nameLabel);
        }
        nameLabel.Text = playerName;
    }
    
    public void SetPlayerId(int playerId)
    {
        ID = playerId;
        _isLocalPlayer = ID == GetTree().GetMultiplayer().GetUniqueId();
        GD.Print($"Player ID set to: {playerId} (Local: {_isLocalPlayer})");
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }
} 