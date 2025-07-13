using Godot;
using System;

public partial class FrostedGlassDemo : Control
{
    [Export] private ShaderMaterial _frostedGlassMaterial;
    
    // UI References
    private Label _blurLabel;
    private HSlider _blurSlider;
    private Label _transparencyLabel;
    private HSlider _transparencySlider;
    private Label _timeLabel;
    private Label _statusLabel;
    private CheckBox _animationCheckBox;
    private CheckBox _frostEffectCheckBox;
    private Button _animatedEffectButton;
    private Button _changeBlurButton;
    private Button _toggleFrostButton;
    
    // Animation variables
    private float _time = 0.0f;
    private bool _animationEnabled = true;
    private bool _frostEffectEnabled = true;
    private Tween _effectTween;
    
    // Background shapes for animation
    private ColorRect _circle1;
    private ColorRect _circle2;
    private ColorRect _circle3;
    
    // Original positions for animation
    private Vector2 _circle1OriginalPos;
    private Vector2 _circle2OriginalPos;
    private Vector2 _circle3OriginalPos;

    public override void _Ready()
    {
        // Get shader material reference
        _frostedGlassMaterial = GetNode<Panel>("UIElements/MainPanel").Material as ShaderMaterial;
        
        // Get UI references
        _blurLabel = GetNode<Label>("UIElements/MainPanel/VBoxContainer/SliderContainer/BlurLabel");
        _blurSlider = GetNode<HSlider>("UIElements/MainPanel/VBoxContainer/SliderContainer/BlurSlider");
        _transparencyLabel = GetNode<Label>("UIElements/MainPanel/VBoxContainer/SliderContainer/TransparencyLabel");
        _transparencySlider = GetNode<HSlider>("UIElements/MainPanel/VBoxContainer/SliderContainer/TransparencySlider");
        _timeLabel = GetNode<Label>("UIElements/BottomBar/HBoxContainer/TimeLabel");
        _statusLabel = GetNode<Label>("UIElements/BottomBar/HBoxContainer/StatusLabel");
        _animationCheckBox = GetNode<CheckBox>("UIElements/SidePanel/VBoxContainer/CheckBox");
        _frostEffectCheckBox = GetNode<CheckBox>("UIElements/SidePanel/VBoxContainer/CheckBox2");
        
        // Get button references
        _animatedEffectButton = GetNode<Button>("UIElements/MainPanel/VBoxContainer/ButtonContainer/Button1");
        _changeBlurButton = GetNode<Button>("UIElements/MainPanel/VBoxContainer/ButtonContainer/Button2");
        _toggleFrostButton = GetNode<Button>("UIElements/MainPanel/VBoxContainer/ButtonContainer/Button3");
        
        // Get animated shapes
        _circle1 = GetNode<ColorRect>("AnimatedShapes/Circle1");
        _circle2 = GetNode<ColorRect>("AnimatedShapes/Circle2");
        _circle3 = GetNode<ColorRect>("AnimatedShapes/Circle3");
        
        // Store original positions
        _circle1OriginalPos = _circle1.Position;
        _circle2OriginalPos = _circle2.Position;
        _circle3OriginalPos = _circle3.Position;
        
        // Connect signals
        _blurSlider.ValueChanged += OnBlurSliderChanged;
        _transparencySlider.ValueChanged += OnTransparencySliderChanged;
        _animationCheckBox.Toggled += OnAnimationToggled;
        _frostEffectCheckBox.Toggled += OnFrostEffectToggled;
        
        _animatedEffectButton.Pressed += OnAnimatedEffectPressed;
        _changeBlurButton.Pressed += OnChangeBlurPressed;
        _toggleFrostButton.Pressed += OnToggleFrostPressed;
        
        // Create tween for effects
        _effectTween = CreateTween();
        _effectTween.SetLoops();
        
        GD.Print("Frosted Glass Demo initialized!");
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        
        // Update time display
        var minutes = (int)(_time / 60);
        var seconds = (int)(_time % 60);
        _timeLabel.Text = $"{minutes:D2}:{seconds:D2}";
        
        // Animate background shapes if enabled
        if (_animationEnabled)
        {
            AnimateBackgroundShapes();
        }
        
        // Animate shader parameters subtly
        if (_frostedGlassMaterial != null && _frostEffectEnabled)
        {
            var distortionValue = 0.015f + Mathf.Sin(_time * 0.5f) * 0.005f;
            _frostedGlassMaterial.SetShaderParameter("distortion", distortionValue);
        }
    }

    private void AnimateBackgroundShapes()
    {
        // Animate circle positions
        var offset1 = new Vector2(
            Mathf.Sin(_time * 0.3f) * 50,
            Mathf.Cos(_time * 0.4f) * 30
        );
        var offset2 = new Vector2(
            Mathf.Sin(_time * 0.5f) * 40,
            Mathf.Cos(_time * 0.3f) * 60
        );
        var offset3 = new Vector2(
            Mathf.Sin(_time * 0.4f) * 60,
            Mathf.Cos(_time * 0.6f) * 40
        );
        
        _circle1.Position = _circle1OriginalPos + offset1;
        _circle2.Position = _circle2OriginalPos + offset2;
        _circle3.Position = _circle3OriginalPos + offset3;
        
        // Animate colors
        var colorPhase = _time * 0.8f;
        _circle1.Color = new Color(
            1.0f,
            0.5f + Mathf.Sin(colorPhase) * 0.3f,
            0.2f + Mathf.Cos(colorPhase * 0.7f) * 0.3f,
            0.8f
        );
        _circle2.Color = new Color(
            0.2f + Mathf.Sin(colorPhase * 0.8f) * 0.3f,
            1.0f,
            0.5f + Mathf.Cos(colorPhase * 1.2f) * 0.3f,
            0.7f
        );
        _circle3.Color = new Color(
            0.5f + Mathf.Sin(colorPhase * 1.1f) * 0.3f,
            0.2f + Mathf.Cos(colorPhase * 0.9f) * 0.3f,
            1.0f,
            0.6f
        );
    }

    private void OnBlurSliderChanged(double value)
    {
        if (_frostedGlassMaterial != null)
        {
            _frostedGlassMaterial.SetShaderParameter("blur_strength", (float)value);
            _blurLabel.Text = $"Blur Strength: {value:F1}";
        }
    }

    private void OnTransparencySliderChanged(double value)
    {
        if (_frostedGlassMaterial != null)
        {
            _frostedGlassMaterial.SetShaderParameter("transparency", (float)value);
            _transparencyLabel.Text = $"Transparency: {value:F2}";
        }
    }

    private void OnAnimationToggled(bool buttonPressed)
    {
        _animationEnabled = buttonPressed;
        _statusLabel.Text = buttonPressed ? "Animation Active" : "Animation Paused";
    }

    private void OnFrostEffectToggled(bool buttonPressed)
    {
        _frostEffectEnabled = buttonPressed;
        if (_frostedGlassMaterial != null)
        {
            var frostAmount = buttonPressed ? 0.25f : 0.0f;
            _frostedGlassMaterial.SetShaderParameter("frost_amount", frostAmount);
        }
        _statusLabel.Text = buttonPressed ? "Frost Effect Active" : "Frost Effect Disabled";
    }

    private void OnAnimatedEffectPressed()
    {
        if (_frostedGlassMaterial == null) return;
        
        // Create animated blur effect
        _effectTween.Kill();
        _effectTween = CreateTween();
        _effectTween.SetLoops(3);
        
        _effectTween.TweenMethod(
            Callable.From<float>(value => _frostedGlassMaterial.SetShaderParameter("blur_strength", value)),
            3.0f, 8.0f, 1.0f
        );
        _effectTween.TweenMethod(
            Callable.From<float>(value => _frostedGlassMaterial.SetShaderParameter("blur_strength", value)),
            8.0f, 3.0f, 1.0f
        );
        
        _statusLabel.Text = "Animated Effect Playing...";
    }

    private void OnChangeBlurPressed()
    {
        // Cycle through different blur levels
        var currentBlur = (float)_frostedGlassMaterial.GetShaderParameter("blur_strength");
        float newBlur = currentBlur switch
        {
            <= 2.0f => 5.0f,
            <= 5.0f => 8.0f,
            <= 8.0f => 1.0f,
            _ => 3.0f
        };
        
        _blurSlider.Value = newBlur;
        _frostedGlassMaterial.SetShaderParameter("blur_strength", newBlur);
        _blurLabel.Text = $"Blur Strength: {newBlur:F1}";
        _statusLabel.Text = $"Blur changed to {newBlur:F1}";
    }

    private void OnToggleFrostPressed()
    {
        _frostEffectEnabled = !_frostEffectEnabled;
        _frostEffectCheckBox.ButtonPressed = _frostEffectEnabled;
        OnFrostEffectToggled(_frostEffectEnabled);
    }
} 