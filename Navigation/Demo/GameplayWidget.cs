using Godot;
using Navigation;

public partial class GameplayWidget : Widget
{
    private Button _pauseButton;
    private Button _inventoryButton;
    private Button _backToMenuButton;
    private Label _scoreLabel;
    private int _score = 0;
    
    public override void _Ready()
    {
        base._Ready();
        
        // Create UI layout
        var vbox = new VBoxContainer();
        vbox.Size = Vector2.Zero;
        vbox.AnchorTop = 0;
        vbox.AnchorBottom = 1;
        vbox.AnchorLeft = 0;
        vbox.AnchorRight = 1;
        vbox.CustomMinimumSize = new Vector2(400, 500);
        AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "Gameplay Screen";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);
        
        // Score Label
        _scoreLabel = new Label();
        _scoreLabel.Text = "Score: 0";
        _scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _scoreLabel.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(_scoreLabel);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer1);
        
        // Add Score Button
        var addScoreButton = new Button();
        addScoreButton.Text = "Add Score (+10)";
        addScoreButton.CustomMinimumSize = new Vector2(200, 50);
        addScoreButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        addScoreButton.Pressed += OnAddScorePressed;
        vbox.AddChild(addScoreButton);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer2);
        
        // Pause Button
        _pauseButton = new Button();
        _pauseButton.Text = "Pause Game";
        _pauseButton.CustomMinimumSize = new Vector2(200, 50);
        _pauseButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _pauseButton.Pressed += OnPausePressed;
        vbox.AddChild(_pauseButton);
        
        // Inventory Button
        _inventoryButton = new Button();
        _inventoryButton.Text = "Open Inventory";
        _inventoryButton.CustomMinimumSize = new Vector2(200, 50);
        _inventoryButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _inventoryButton.Pressed += OnInventoryPressed;
        vbox.AddChild(_inventoryButton);
        
        // Spacer
        var spacer3 = new Control();
        spacer3.CustomMinimumSize = new Vector2(0, 50);
        vbox.AddChild(spacer3);
        
        // Back to Menu Button
        _backToMenuButton = new Button();
        _backToMenuButton.Text = "Back to Menu";
        _backToMenuButton.CustomMinimumSize = new Vector2(200, 50);
        _backToMenuButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _backToMenuButton.Pressed += OnBackToMenuPressed;
        vbox.AddChild(_backToMenuButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    private void OnAddScorePressed()
    {
        _score += 10;
        _scoreLabel.Text = $"Score: {_score}";
    }
    
    private void OnPausePressed()
    {
        Navigator.ShowModal<PauseWidget>(pauseWidget => 
        {
            pauseWidget.SetScore(_score);
        });
    }
    
    private void OnInventoryPressed()
    {
        Navigator.Push<InventoryWidget>(inventoryWidget => 
        {
            inventoryWidget.SetPlayerScore(_score);
        });
    }
    
    private void OnBackToMenuPressed()
    {
        Navigator.PopToRoot(_score);
    }
} 