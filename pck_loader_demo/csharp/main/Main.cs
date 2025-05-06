using Godot;
using FileAccess = Godot.FileAccess;

public partial class Main : Control
{
    private string _loadedPckPath = "";
    private string _dlcScenePath = "res://pck_loader_demo/csharp/dlc/DlcScene.tscn";
    private Node _dlcInstance = null;
    private string _pckFilePath;
    
    // UI elements
    private Button _loadPckButton;
    private Button _instantiateDlcButton;
    private Button _unloadPckButton;
    private Label _statusLabel;
    private Control _dlcContainer;
    
    public override void _Ready()
    {
        // Get the user:// directory path
        _pckFilePath = OS.GetUserDataDir() + "/dlc_content_cs.pck";
        
        // Get UI elements
        _loadPckButton = GetNode<Button>("Panel/VBoxContainer/LoadPCKButton");
        _instantiateDlcButton = GetNode<Button>("Panel/VBoxContainer/InstantiateDLCButton");
        _unloadPckButton = GetNode<Button>("Panel/VBoxContainer/UnloadPCKButton");
        _statusLabel = GetNode<Label>("Panel/VBoxContainer/StatusLabel");
        _dlcContainer = GetNode<Control>("DLCContainer");
        
        // Connect signals
        _loadPckButton.Pressed += OnLoadPckButtonPressed;
        _instantiateDlcButton.Pressed += OnInstantiateDlcButtonPressed;
        _unloadPckButton.Pressed += OnUnloadPckButtonPressed;
        
        // Copy the PCK file from the project to the user directory if needed
        CopyPckFileIfNeeded();
    }
    
    private void CopyPckFileIfNeeded()
    {
        if (!FileAccess.FileExists(_pckFilePath))
        {
            string localPck = "res://pck_loader_demo/csharp/dlc/dlc_content_cs.pck";
            if (FileAccess.FileExists(localPck))
            {
                using var sourceFile = FileAccess.Open(localPck, FileAccess.ModeFlags.Read);
                using var destFile = FileAccess.Open(_pckFilePath, FileAccess.ModeFlags.Write);
                
                var content = sourceFile.GetBuffer((long)sourceFile.GetLength());
                destFile.StoreBuffer(content);
                
                GD.Print("Copied PCK file to user directory");
            }
        }
    }
    
    private void OnLoadPckButtonPressed()
    {
        if (_loadedPckPath != "")
        {
            UpdateStatus("PCK is already loaded!");
            return;
        }
        
        if (!FileAccess.FileExists(_pckFilePath))
        {
            UpdateStatus("ERROR: PCK file not found!");
            return;
        }
        
        // Attempt to load the PCK file
        bool success = ProjectSettings.LoadResourcePack(_pckFilePath);
        
        if (success)
        {
            _loadedPckPath = _pckFilePath;
            UpdateStatus("PCK loaded successfully!");
            _loadPckButton.Disabled = true;
            _instantiateDlcButton.Disabled = false;
            _unloadPckButton.Disabled = false;
        }
        else
        {
            UpdateStatus("ERROR: Failed to load PCK!");
        }
    }
    
    private void OnInstantiateDlcButtonPressed()
    {
        if (_loadedPckPath == "")
        {
            UpdateStatus("ERROR: No PCK loaded!");
            return;
        }
        
        if (_dlcInstance != null)
        {
            UpdateStatus("DLC is already instantiated!");
            return;
        }
        
        // Check if the scene exists in the loaded PCK
        if (ResourceLoader.Exists(_dlcScenePath))
        {
            var sceneResource = ResourceLoader.Load<PackedScene>(_dlcScenePath);
            if (sceneResource != null)
            {
                _dlcInstance = sceneResource.Instantiate();
                _dlcContainer.AddChild(_dlcInstance);
                UpdateStatus("DLC scene instantiated!");
            }
            else
            {
                UpdateStatus("ERROR: Could not load DLC scene!");
            }
        }
        else
        {
            UpdateStatus("ERROR: DLC scene not found in PCK!");
        }
    }
    
    private void OnUnloadPckButtonPressed()
    {
        if (_loadedPckPath == "")
        {
            UpdateStatus("No PCK is loaded!");
            return;
        }
        
        // Remove any instantiated content
        if (_dlcInstance != null)
        {
            _dlcInstance.QueueFree();
            _dlcInstance = null;
        }
        
        // In Godot 4, there's no direct API to unload a PCK
        // The best practice is to restart the scene/game
        // For this demo, we'll simulate unloading by disabling the buttons
        _loadedPckPath = "";
        _loadPckButton.Disabled = false;
        _instantiateDlcButton.Disabled = true;
        _unloadPckButton.Disabled = true;
        UpdateStatus("PCK has been 'unloaded' (simulated)");
    }
    
    private void UpdateStatus(string message)
    {
        _statusLabel.Text = "Status: " + message;
        GD.Print(message);
    }
} 