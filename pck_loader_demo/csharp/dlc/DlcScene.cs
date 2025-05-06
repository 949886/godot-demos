using Godot;
using System;

public partial class DlcScene : Control
{
    private bool _animationPlaying = false;
    private Vector2 _originalPosition;
    private Tween _tween;
    
    // UI elements
    private Button _animationButton;
    private VBoxContainer _container;
    private Panel _panel;
    
    public override void _Ready()
    {
        // Get UI elements
        _container = GetNode<VBoxContainer>("Panel/VBoxContainer");
        _animationButton = GetNode<Button>("Panel/VBoxContainer/AnimationButton");
        _panel = GetNode<Panel>("Panel");
        
        // Store original position
        _originalPosition = _container.Position;
        
        // Connect signals
        _animationButton.Pressed += OnAnimationButtonPressed;
        
        GD.Print("DLC Scene (C#) loaded and ready!");
    }
    
    private void OnAnimationButtonPressed()
    {
        if (_animationPlaying)
            return;
            
        _animationPlaying = true;
        
        // Create a new tween for animation
        _tween = CreateTween()
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
            
        _tween.TweenProperty(_container, "position", _originalPosition + new Vector2(0, -50), 0.5);
        _tween.TweenProperty(_container, "position", _originalPosition, 0.5);
        
        // Change color animation
        var colorTween = CreateTween()
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
            
        var stylebox = (StyleBoxFlat)_panel.GetThemeStylebox("panel");
        var originalColor = stylebox.BgColor;
        var newColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);
        
        // First color change (original to new)
        colorTween.TweenMethod(new Callable(this, nameof(UpdatePanelColor)), 
                              originalColor, 
                              newColor, 
                              0.5);
                              
        // Second color change (new to original)
        colorTween.TweenMethod(new Callable(this, nameof(UpdatePanelColor)), 
                              newColor, 
                              originalColor, 
                              0.5);
        
        // When the tween is completed, reset the flag
        _tween.Finished += () => _animationPlaying = false;
    }
    
    // This method is called by the tween to update the panel color
    private void UpdatePanelColor(Color color)
    {
        var stylebox = (StyleBoxFlat)_panel.GetThemeStylebox("panel").Duplicate();
        stylebox.BgColor = color;
        _panel.AddThemeStyleboxOverride("panel", stylebox);
    }
} 