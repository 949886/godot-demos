using Godot;
using Navigation;

public partial class PauseWidget : Widget
{
    private Label _scoreLabel;
    private Button _resumeButton;
    private Button _restartButton;
    private Button _quitButton;
    private int _currentScore;
    
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
        vbox.CustomMinimumSize = new Vector2(300, 400);
        AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "Game Paused";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);
        
        // Score Label
        _scoreLabel = new Label();
        _scoreLabel.Text = "Score: 0";
        _scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _scoreLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_scoreLabel);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer1);
        
        // Resume Button
        _resumeButton = new Button();
        _resumeButton.Text = "Resume";
        _resumeButton.CustomMinimumSize = new Vector2(200, 50);
        _resumeButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _resumeButton.Pressed += OnResumePressed;
        vbox.AddChild(_resumeButton);
        
        // Restart Button
        _restartButton = new Button();
        _restartButton.Text = "Restart";
        _restartButton.CustomMinimumSize = new Vector2(200, 50);
        _restartButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _restartButton.Pressed += OnRestartPressed;
        vbox.AddChild(_restartButton);
        
        // Quit Button
        _quitButton = new Button();
        _quitButton.Text = "Quit to Menu";
        _quitButton.CustomMinimumSize = new Vector2(200, 50);
        _quitButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _quitButton.Pressed += OnQuitPressed;
        vbox.AddChild(_quitButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    public void SetScore(int score)
    {
        _currentScore = score;
        _scoreLabel.Text = $"Score: {score}";
    }
    
    private void OnResumePressed()
    {
        Navigator.Pop();
    }
    
    private void OnRestartPressed()
    {
        Navigator.PopToRoot();
        Navigator.Push<GameplayWidget>();
    }
    
    private void OnQuitPressed()
    {
        Navigator.PopToRoot();
    }
} 