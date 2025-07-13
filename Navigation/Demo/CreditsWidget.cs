using Godot;
using Navigation;

public partial class CreditsWidget : Widget
{
    private Button _backButton;
    
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
        title.Text = "Credits";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer1);
        
        // Credits content
        var creditsText = new RichTextLabel();
        creditsText.Text = """
            [center]UI Navigation Demo[/center]
            
            [b]Developed by:[/b]
            Your Name
            
            [b]Godot Version:[/b]
            4.4
            
            [b]Features:[/b]
            • Stack-based navigation
            • Modal dialogs
            • Data passing between screens
            • Type-safe navigation
            
            [b]Special Thanks:[/b]
            Godot Engine Team
            Unity Navigation (for inspiration)
            
            [center]Made with ❤️ and Godot[/center]
            """;
        creditsText.CustomMinimumSize = new Vector2(350, 300);
        creditsText.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(creditsText);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer2);
        
        // Back Button
        _backButton = new Button();
        _backButton.Text = "Back to Menu";
        _backButton.CustomMinimumSize = new Vector2(200, 50);
        _backButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _backButton.Pressed += OnBackPressed;
        vbox.AddChild(_backButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    private void OnBackPressed()
    {
        Navigator.Pop();
    }
} 