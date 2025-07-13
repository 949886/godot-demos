using Godot;
using Navigation;
using System.Threading.Tasks;

/// <summary>
/// Advanced example demonstrating complex navigation patterns
/// </summary>
public partial class AdvancedExample : Node
{
    private Navigator _navigator;
    
    public override void _Ready()
    {
        // Initialize navigator
        _navigator = new Navigator();
        _navigator.Name = "Advanced Navigator";
        AddChild(_navigator);
        
        // Subscribe to events
        _navigator.onPushed += OnScreenPushed;
        _navigator.onPopped += OnScreenPopped;
        
        // Start with main menu
        var mainMenu = new MainMenuWidget();
        Navigator.RootWidget = mainMenu;
        
        // Demonstrate advanced patterns after a delay
        CallDeferred(nameof(DemonstrateAdvancedPatterns));
    }
    
    private async void DemonstrateAdvancedPatterns()
    {
        await Task.Delay(2000); // Wait 2 seconds
        
        GD.Print("=== Advanced Navigation Patterns Demo ===");
        
        // 1. Demonstrate data passing with async/await
        await DemonstrateDataPassing();
        
        // 2. Demonstrate modal stacking
        await DemonstrateModalStacking();
        
        // 3. Demonstrate navigation history
        await DemonstrateNavigationHistory();
    }
    
    private async Task DemonstrateDataPassing()
    {
        GD.Print("1. Data Passing Demo");
        
        // Push gameplay with data
        var gameplayTask = Navigator.Push<GameplayWidget>(gameplay => 
        {
            GD.Print("Gameplay widget initialized with data");
        });
        
        await Task.Delay(1000);
        
        // Show modal with data
        var modalTask = Navigator.ShowModal<PauseWidget>(pause => 
        {
            pause.SetScore(150);
            GD.Print("Pause modal initialized with score: 150");
        });
        
        await Task.Delay(1000);
        
        // Pop modal
        Navigator.Pop();
        await Task.Delay(500);
        
        // Pop gameplay with result
        Navigator.Pop(250);
        await Task.Delay(500);
        
        GD.Print("Data passing demo completed");
    }
    
    private async Task DemonstrateModalStacking()
    {
        GD.Print("2. Modal Stacking Demo");
        
        // Push gameplay
        Navigator.Push<GameplayWidget>();
        await Task.Delay(500);
        
        // Show first modal
        Navigator.ShowModal<PauseWidget>();
        await Task.Delay(500);
        
        // Show second modal on top
        Navigator.ShowModal<ConfirmationWidget>(confirmation => 
        {
            confirmation.SetMessage("This is a modal on top of another modal!");
        });
        await Task.Delay(1000);
        
        // Pop modals one by one
        Navigator.Pop(); // Close confirmation
        await Task.Delay(500);
        
        Navigator.Pop(); // Close pause
        await Task.Delay(500);
        
        Navigator.Pop(); // Close gameplay
        await Task.Delay(500);
        
        GD.Print("Modal stacking demo completed");
    }
    
    private async Task DemonstrateNavigationHistory()
    {
        GD.Print("3. Navigation History Demo");
        
        // Build a navigation stack
        Navigator.Push<GameplayWidget>();
        await Task.Delay(300);
        
        Navigator.Push<SettingsWidget>();
        await Task.Delay(300);
        
        Navigator.Push<CreditsWidget>();
        await Task.Delay(300);
        
        Navigator.Push<InventoryWidget>();
        await Task.Delay(300);
        
        // Demonstrate PopUntil
        GD.Print("Using PopUntil to go back to Settings");
        Navigator.PopUntil<SettingsWidget>();
        await Task.Delay(1000);
        
        // Demonstrate BackTo
        GD.Print("Using BackTo to go back to Gameplay");
        var gameplay = Navigator.BackTo<GameplayWidget>();
        await Task.Delay(1000);
        
        // Demonstrate PopToRoot
        GD.Print("Using PopToRoot to return to main menu");
        Navigator.PopToRoot();
        await Task.Delay(500);
        
        GD.Print("Navigation history demo completed");
    }
    
    private void OnScreenPushed(Route route)
    {
        GD.Print($"Screen pushed: {route.To.GetType().Name}");
    }
    
    private void OnScreenPopped(Route route)
    {
        GD.Print($"Screen popped: {route.To.GetType().Name}");
    }
}

/// <summary>
/// Example of a custom widget with complex data flow
/// </summary>
public partial class AdvancedGameplayWidget : Widget
{
    private int _level = 1;
    private float _health = 100.0f;
    private string _playerName = "Player";
    
    public override void _Ready()
    {
        base._Ready();
        
        // Create UI
        var vbox = new VBoxContainer();
        vbox.Size = Vector2.Zero;
        vbox.AnchorTop = 0;
        vbox.AnchorBottom = 1;
        vbox.AnchorLeft = 0;
        vbox.AnchorRight = 1;
        AddChild(vbox);
        
        var title = new Label();
        title.Text = "Advanced Gameplay";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);
        
        var levelLabel = new Label();
        levelLabel.Text = $"Level: {_level}";
        vbox.AddChild(levelLabel);
        
        var healthLabel = new Label();
        healthLabel.Text = $"Health: {_health}";
        vbox.AddChild(healthLabel);
        
        var nameLabel = new Label();
        nameLabel.Text = $"Player: {_playerName}";
        vbox.AddChild(nameLabel);
        
        var nextLevelButton = new Button();
        nextLevelButton.Text = "Next Level";
        nextLevelButton.Pressed += OnNextLevel;
        vbox.AddChild(nextLevelButton);
        
        var backButton = new Button();
        backButton.Text = "Back";
        backButton.Pressed += OnBack;
        vbox.AddChild(backButton);
    }
    
    public void Initialize(int level, float health, string playerName)
    {
        _level = level;
        _health = health;
        _playerName = playerName;
    }
    
    private void OnNextLevel()
    {
        _level++;
        Navigator.Push<AdvancedGameplayWidget>(nextLevel => 
        {
            nextLevel.Initialize(_level, _health, _playerName);
        });
    }
    
    private void OnBack()
    {
        Navigator.Pop(new { Level = _level, Health = _health, PlayerName = _playerName });
    }
} 