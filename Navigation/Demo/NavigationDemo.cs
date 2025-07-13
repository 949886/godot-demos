using Godot;
using Navigation;

public partial class NavigationDemo : Node
{
    private Navigator _navigator;
    
    public override void _Ready()
    {
        // Create the navigator
        _navigator = new Navigator();
        _navigator.Name = "UI Navigator";
        AddChild(_navigator);
        
        // Set up the navigator
        _navigator.escToPop = true;
        _navigator.focusAutomatically = true;
        
        // Wait for the navigator to be ready before setting the root widget
        CallDeferred(nameof(InitializeMainMenu));
        
        // Subscribe to navigation events for debugging
        _navigator.onPushed += OnWidgetPushed;
        _navigator.onPopped += OnWidgetPopped;
        
        GD.Print("Navigation Demo initialized!");
        GD.Print("Press ESC to go back, or use the navigation buttons.");
    }
    
    private void InitializeMainMenu()
    {
        // Create the main menu widget
        var mainMenu = new MainMenuWidget();
        
        // Add it to the canvas layer
        _navigator.canvasLayer.AddChild(mainMenu);
        
        // Set it as the root widget
        _navigator.rootWidget = mainMenu;
        
        // Make sure it's visible
        mainMenu.Visible = true;
        
        GD.Print("Main menu initialized!");
        GD.Print($"Main menu visible: {mainMenu.Visible}");
        GD.Print($"Main menu parent: {mainMenu.GetParent()}");
    }
    
    private void OnWidgetPushed(Route route)
    {
        GD.Print($"Widget pushed: {route.To.GetType().Name}");
    }
    
    private void OnWidgetPopped(Route route)
    {
        GD.Print($"Widget popped: {route.To.GetType().Name}");
    }
} 