using Godot;
using Navigation;

public partial class SimpleTest : Node
{
    public override void _Ready()
    {
        // Create a simple test to see if anything shows up
        var testLabel = new Label();
        testLabel.Text = "Simple Test - If you can see this, the scene is working!";
        testLabel.Position = new Vector2(100, 100);
        AddChild(testLabel);
        
        // Try to create navigator
        var navigator = new Navigator();
        navigator.Name = "Test Navigator";
        AddChild(navigator);
        
        GD.Print("Simple test initialized!");
        GD.Print($"Navigator created: {navigator != null}");
        GD.Print($"Canvas layer: {navigator.canvasLayer != null}");
    }
} 