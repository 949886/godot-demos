using Godot;
using System;

public partial class GaussianBlurDemo : Control
{
    [Export] private ShaderMaterial _gaussianBlurMaterial;
    
    // UI References
    private Label _blurRadiusLabel;
    private HSlider _blurRadiusSlider;
    private Label _samplesLabel;
    private HSlider _samplesSlider;
    private Label _transparencyLabel;
    private HSlider _transparencySlider;
    private Label _statusLabel;
    private Label _qualityLabel;
    private Label _fpsLabel;
    private Label _timeLabel;
    
    // Checkboxes
    private CheckBox _animateBlurCheck;
    private CheckBox _animateObjectsCheck;
    
    // Buttons
    private Button _presetButton1;
    private Button _presetButton2;
    private Button _presetButton3;
    private Button _pulseEffectButton;
    private Button _fadeEffectButton;
    private Button _lowQualityButton;
    private Button _mediumQualityButton;
    private Button _highQualityButton;
    private Button _resetButton;
    
    // Animation variables
    private float _time = 0.0f;
    private bool _animateBlur = false;
    private bool _animateObjects = true;
    private Tween _effectTween;
    
    // Background objects for animation
    private ColorRect _object1;
    private ColorRect _object2;
    private ColorRect _object3;
    private ColorRect _object4;
    
    // Original positions and colors
    private Vector2 _object1OriginalPos;
    private Vector2 _object2OriginalPos;
    private Vector2 _object3OriginalPos;
    private Vector2 _object4OriginalPos;
    private Color _object1OriginalColor;
    private Color _object2OriginalColor;
    private Color _object3OriginalColor;
    private Color _object4OriginalColor;
    
    // Quality presets
    private readonly (float radius, int samples)[] _qualityPresets = {
        (3.0f, 5),   // Low
        (8.0f, 15),  // Medium
        (15.0f, 25)  // High
    };

    public override void _Ready()
    {
        // Get shader material reference
        _gaussianBlurMaterial = GetNode<Panel>("UIElements/MainControlPanel").Material as ShaderMaterial;
        
        // Get UI references
        _blurRadiusLabel = GetNode<Label>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/BlurRadiusContainer/BlurRadiusLabel");
        _blurRadiusSlider = GetNode<HSlider>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/BlurRadiusContainer/BlurRadiusSlider");
        _samplesLabel = GetNode<Label>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/SamplesContainer/SamplesLabel");
        _samplesSlider = GetNode<HSlider>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/SamplesContainer/SamplesSlider");
        _transparencyLabel = GetNode<Label>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/TransparencyContainer/TransparencyLabel");
        _transparencySlider = GetNode<HSlider>("UIElements/MainControlPanel/VBoxContainer/ControlsContainer/TransparencyContainer/TransparencySlider");
        
        _statusLabel = GetNode<Label>("UIElements/InfoPanel/HBoxContainer/StatusContainer/StatusLabel");
        _qualityLabel = GetNode<Label>("UIElements/InfoPanel/HBoxContainer/StatusContainer/QualityLabel");
        _fpsLabel = GetNode<Label>("UIElements/InfoPanel/HBoxContainer/StatsContainer/FPSLabel");
        _timeLabel = GetNode<Label>("UIElements/InfoPanel/HBoxContainer/StatsContainer/TimeLabel");
        
        // Get checkboxes
        _animateBlurCheck = GetNode<CheckBox>("UIElements/AnimationPanel/VBoxContainer/AnimateBlurCheck");
        _animateObjectsCheck = GetNode<CheckBox>("UIElements/AnimationPanel/VBoxContainer/AnimateObjectsCheck");
        
        // Get buttons
        _presetButton1 = GetNode<Button>("UIElements/MainControlPanel/VBoxContainer/ButtonsContainer/PresetButton1");
        _presetButton2 = GetNode<Button>("UIElements/MainControlPanel/VBoxContainer/ButtonsContainer/PresetButton2");
        _presetButton3 = GetNode<Button>("UIElements/MainControlPanel/VBoxContainer/ButtonsContainer/PresetButton3");
        _pulseEffectButton = GetNode<Button>("UIElements/AnimationPanel/VBoxContainer/PulseEffectButton");
        _fadeEffectButton = GetNode<Button>("UIElements/AnimationPanel/VBoxContainer/FadeEffectButton");
        _lowQualityButton = GetNode<Button>("UIElements/ComparisonPanel/VBoxContainer/LowQualityButton");
        _mediumQualityButton = GetNode<Button>("UIElements/ComparisonPanel/VBoxContainer/MediumQualityButton");
        _highQualityButton = GetNode<Button>("UIElements/ComparisonPanel/VBoxContainer/HighQualityButton");
        _resetButton = GetNode<Button>("UIElements/FloatingActionButton");
        
        // Get background objects
        _object1 = GetNode<ColorRect>("BackgroundObjects/Object1");
        _object2 = GetNode<ColorRect>("BackgroundObjects/Object2");
        _object3 = GetNode<ColorRect>("BackgroundObjects/Object3");
        _object4 = GetNode<ColorRect>("BackgroundObjects/Object4");
        
        // Store original positions and colors
        _object1OriginalPos = _object1.Position;
        _object2OriginalPos = _object2.Position;
        _object3OriginalPos = _object3.Position;
        _object4OriginalPos = _object4.Position;
        _object1OriginalColor = _object1.Color;
        _object2OriginalColor = _object2.Color;
        _object3OriginalColor = _object3.Color;
        _object4OriginalColor = _object4.Color;
        
        // Connect signals
        ConnectSignals();
        
        // Create tween for effects
        _effectTween = CreateTween();
        _effectTween.SetLoops();
        
        GD.Print("Gaussian Blur Demo initialized!");
    }

    private void ConnectSignals()
    {
        // Slider signals
        _blurRadiusSlider.ValueChanged += OnBlurRadiusChanged;
        _samplesSlider.ValueChanged += OnSamplesChanged;
        _transparencySlider.ValueChanged += OnTransparencyChanged;
        
        // Checkbox signals
        _animateBlurCheck.Toggled += OnAnimateBlurToggled;
        _animateObjectsCheck.Toggled += OnAnimateObjectsToggled;
        
        // Button signals
        _presetButton1.Pressed += () => ApplyBlurPreset(5.0f, 10);
        _presetButton2.Pressed += () => ApplyBlurPreset(15.0f, 15);
        _presetButton3.Pressed += () => ApplyBlurPreset(30.0f, 20);
        
        _pulseEffectButton.Pressed += OnPulseEffectPressed;
        _fadeEffectButton.Pressed += OnFadeEffectPressed;
        
        _lowQualityButton.Pressed += () => ApplyQualityPreset(0);
        _mediumQualityButton.Pressed += () => ApplyQualityPreset(1);
        _highQualityButton.Pressed += () => ApplyQualityPreset(2);
        
        _resetButton.Pressed += OnResetPressed;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        
        // Update time display
        var minutes = (int)(_time / 60);
        var seconds = (int)(_time % 60);
        _timeLabel.Text = $"Runtime: {minutes:D2}:{seconds:D2}";
        
        // Update FPS display
        _fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
        
        // Animate background objects if enabled
        if (_animateObjects)
        {
            AnimateBackgroundObjects();
        }
        
        // Animate blur if enabled
        if (_animateBlur && _gaussianBlurMaterial != null)
        {
            var baseRadius = (float)_blurRadiusSlider.Value;
            var animatedRadius = baseRadius + Mathf.Sin(_time * 2.0f) * (baseRadius * 0.3f);
            _gaussianBlurMaterial.SetShaderParameter("blur_radius", animatedRadius);
        }
    }

    private void AnimateBackgroundObjects()
    {
        // Animate object positions
        var offset1 = new Vector2(
            Mathf.Sin(_time * 0.4f) * 60,
            Mathf.Cos(_time * 0.3f) * 40
        );
        var offset2 = new Vector2(
            Mathf.Sin(_time * 0.6f) * 50,
            Mathf.Cos(_time * 0.5f) * 70
        );
        var offset3 = new Vector2(
            Mathf.Sin(_time * 0.5f) * 80,
            Mathf.Cos(_time * 0.7f) * 35
        );
        var offset4 = new Vector2(
            Mathf.Sin(_time * 0.3f) * 45,
            Mathf.Cos(_time * 0.4f) * 55
        );
        
        _object1.Position = _object1OriginalPos + offset1;
        _object2.Position = _object2OriginalPos + offset2;
        _object3.Position = _object3OriginalPos + offset3;
        _object4.Position = _object4OriginalPos + offset4;
        
        // Animate colors
        var colorPhase = _time * 0.6f;
        _object1.Color = new Color(
            _object1OriginalColor.R,
            _object1OriginalColor.G + Mathf.Sin(colorPhase) * 0.2f,
            _object1OriginalColor.B + Mathf.Cos(colorPhase * 0.8f) * 0.2f,
            _object1OriginalColor.A
        );
        _object2.Color = new Color(
            _object2OriginalColor.R + Mathf.Sin(colorPhase * 0.9f) * 0.2f,
            _object2OriginalColor.G,
            _object2OriginalColor.B + Mathf.Cos(colorPhase * 1.1f) * 0.2f,
            _object2OriginalColor.A
        );
        _object3.Color = new Color(
            _object3OriginalColor.R + Mathf.Sin(colorPhase * 1.2f) * 0.2f,
            _object3OriginalColor.G + Mathf.Cos(colorPhase * 0.7f) * 0.2f,
            _object3OriginalColor.B,
            _object3OriginalColor.A
        );
        _object4.Color = new Color(
            _object4OriginalColor.R,
            _object4OriginalColor.G + Mathf.Sin(colorPhase * 0.8f) * 0.15f,
            _object4OriginalColor.B + Mathf.Cos(colorPhase * 1.3f) * 0.15f,
            _object4OriginalColor.A
        );
    }

    private void OnBlurRadiusChanged(double value)
    {
        if (_gaussianBlurMaterial != null && !_animateBlur)
        {
            _gaussianBlurMaterial.SetShaderParameter("blur_radius", (float)value);
        }
        _blurRadiusLabel.Text = $"Blur Radius: {value:F1}";
    }

    private void OnSamplesChanged(double value)
    {
        if (_gaussianBlurMaterial != null)
        {
            _gaussianBlurMaterial.SetShaderParameter("blur_samples", (int)value);
            UpdateQualityLabel((int)value);
        }
        _samplesLabel.Text = $"Blur Samples: {(int)value}";
    }

    private void OnTransparencyChanged(double value)
    {
        if (_gaussianBlurMaterial != null)
        {
            _gaussianBlurMaterial.SetShaderParameter("transparency", (float)value);
        }
        _transparencyLabel.Text = $"Transparency: {value:F2}";
    }

    private void OnAnimateBlurToggled(bool buttonPressed)
    {
        _animateBlur = buttonPressed;
        _statusLabel.Text = buttonPressed ? "Blur Animation Active" : "Gaussian Blur Active";
    }

    private void OnAnimateObjectsToggled(bool buttonPressed)
    {
        _animateObjects = buttonPressed;
        if (!buttonPressed)
        {
            // Reset objects to original positions and colors
            _object1.Position = _object1OriginalPos;
            _object2.Position = _object2OriginalPos;
            _object3.Position = _object3OriginalPos;
            _object4.Position = _object4OriginalPos;
            _object1.Color = _object1OriginalColor;
            _object2.Color = _object2OriginalColor;
            _object3.Color = _object3OriginalColor;
            _object4.Color = _object4OriginalColor;
        }
    }

    private void ApplyBlurPreset(float radius, int samples)
    {
        _blurRadiusSlider.Value = radius;
        _samplesSlider.Value = samples;
        
        if (_gaussianBlurMaterial != null)
        {
            _gaussianBlurMaterial.SetShaderParameter("blur_radius", radius);
            _gaussianBlurMaterial.SetShaderParameter("blur_samples", samples);
        }
        
        _statusLabel.Text = $"Applied preset: {radius}px radius, {samples} samples";
    }

    private void ApplyQualityPreset(int presetIndex)
    {
        if (presetIndex >= 0 && presetIndex < _qualityPresets.Length)
        {
            var preset = _qualityPresets[presetIndex];
            ApplyBlurPreset(preset.radius, preset.samples);
            
            string qualityName = presetIndex switch
            {
                0 => "Low",
                1 => "Medium",
                2 => "High",
                _ => "Unknown"
            };
            
            _statusLabel.Text = $"Applied {qualityName} quality preset";
        }
    }

    private void OnPulseEffectPressed()
    {
        if (_gaussianBlurMaterial == null) return;
        
        _effectTween.Kill();
        _effectTween = CreateTween();
        _effectTween.SetLoops(3);
        
        var currentRadius = (float)_blurRadiusSlider.Value;
        var maxRadius = Mathf.Min(currentRadius * 2.5f, 40.0f);
        
        _effectTween.TweenMethod(
            Callable.From<float>(value => _gaussianBlurMaterial.SetShaderParameter("blur_radius", value)),
            currentRadius, maxRadius, 0.8f
        );
        _effectTween.TweenMethod(
            Callable.From<float>(value => _gaussianBlurMaterial.SetShaderParameter("blur_radius", value)),
            maxRadius, currentRadius, 0.8f
        );
        
        _statusLabel.Text = "Pulse effect playing...";
    }

    private void OnFadeEffectPressed()
    {
        if (_gaussianBlurMaterial == null) return;
        
        _effectTween.Kill();
        _effectTween = CreateTween();
        
        var currentTransparency = (float)_transparencySlider.Value;
        
        _effectTween.TweenMethod(
            Callable.From<float>(value => _gaussianBlurMaterial.SetShaderParameter("transparency", value)),
            currentTransparency, 0.0f, 1.0f
        );
        _effectTween.TweenMethod(
            Callable.From<float>(value => _gaussianBlurMaterial.SetShaderParameter("transparency", value)),
            0.0f, currentTransparency, 1.0f
        );
        
        _statusLabel.Text = "Fade effect playing...";
    }

    private void OnResetPressed()
    {
        // Reset all controls to default values
        _blurRadiusSlider.Value = 8.0f;
        _samplesSlider.Value = 15;
        _transparencySlider.Value = 0.85f;
        
        _animateBlurCheck.ButtonPressed = false;
        _animateObjectsCheck.ButtonPressed = true;
        
        // Reset shader parameters
        if (_gaussianBlurMaterial != null)
        {
            _gaussianBlurMaterial.SetShaderParameter("blur_radius", 8.0f);
            _gaussianBlurMaterial.SetShaderParameter("blur_samples", 15);
            _gaussianBlurMaterial.SetShaderParameter("transparency", 0.85f);
        }
        
        // Reset animation states
        _animateBlur = false;
        _animateObjects = true;
        
        _statusLabel.Text = "Settings reset to defaults";
        UpdateQualityLabel(15);
    }

    private void UpdateQualityLabel(int samples)
    {
        string quality = samples switch
        {
            <= 7 => "Low",
            <= 15 => "Medium",
            <= 20 => "High",
            _ => "Ultra"
        };
        
        _qualityLabel.Text = $"Quality: {quality} ({samples} samples)";
    }
} 