using Godot;
using Navigation;

public partial class MainMenuWidget : Widget
{
    private Button _playButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _quitButton;
    
    public override void _Ready()
    {
        base._Ready();
        
        // Set up the widget itself
        Size = Vector2.Zero;
        AnchorTop = 0;
        AnchorBottom = 1;
        AnchorLeft = 0;
        AnchorRight = 1;
        
        // Create UI layout
        var vbox = new VBoxContainer();
        vbox.Size = Vector2.Zero;
        vbox.AnchorTop = 0;
        vbox.AnchorBottom = 1;
        vbox.AnchorLeft = 0;
        vbox.AnchorRight = 1;
        vbox.CustomMinimumSize = new Vector2(300, 400);
        vbox.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "UI Navigation Demo";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 32);
        vbox.AddChild(title);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 50);
        vbox.AddChild(spacer1);
        
        // Play Button
        _playButton = new Button();
        _playButton.Text = "Play Game";
        _playButton.CustomMinimumSize = new Vector2(200, 50);
        _playButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _playButton.Pressed += OnPlayPressed;
        vbox.AddChild(_playButton);
        
        // Settings Button
        _settingsButton = new Button();
        _settingsButton.Text = "Settings";
        _settingsButton.CustomMinimumSize = new Vector2(200, 50);
        _settingsButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _settingsButton.Pressed += OnSettingsPressed;
        vbox.AddChild(_settingsButton);
        
        // Credits Button
        _creditsButton = new Button();
        _creditsButton.Text = "Credits";
        _creditsButton.CustomMinimumSize = new Vector2(200, 50);
        _creditsButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _creditsButton.Pressed += OnCreditsPressed;
        vbox.AddChild(_creditsButton);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 50);
        vbox.AddChild(spacer2);
        
        // Quit Button
        _quitButton = new Button();
        _quitButton.Text = "Quit";
        _quitButton.CustomMinimumSize = new Vector2(200, 50);
        _quitButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _quitButton.Pressed += OnQuitPressed;
        vbox.AddChild(_quitButton);
    }
    
    private void OnPlayPressed()
    {
        Navigator.Push<GameplayWidget>();
    }
    
    private void OnSettingsPressed()
    {
        Navigator.Push<SettingsWidget>();
    }
    
    private void OnCreditsPressed()
    {
        Navigator.Push<CreditsWidget>();
    }
    
    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
} 