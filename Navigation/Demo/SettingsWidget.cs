using Godot;
using Navigation;

public partial class SettingsWidget : Widget
{
    private HSlider _volumeSlider;
    private CheckBox _fullscreenCheckBox;
    private OptionButton _qualityOptionButton;
    private Button _saveButton;
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
        title.Text = "Settings";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 30);
        vbox.AddChild(spacer1);
        
        // Volume Setting
        var volumeLabel = new Label();
        volumeLabel.Text = "Volume:";
        volumeLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(volumeLabel);
        
        _volumeSlider = new HSlider();
        _volumeSlider.MinValue = 0;
        _volumeSlider.MaxValue = 100;
        _volumeSlider.Value = 50;
        _volumeSlider.CustomMinimumSize = new Vector2(300, 30);
        _volumeSlider.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(_volumeSlider);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer2);
        
        // Fullscreen Setting
        _fullscreenCheckBox = new CheckBox();
        _fullscreenCheckBox.Text = "Fullscreen";
        _fullscreenCheckBox.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(_fullscreenCheckBox);
        
        // Spacer
        var spacer3 = new Control();
        spacer3.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer3);
        
        // Quality Setting
        var qualityLabel = new Label();
        qualityLabel.Text = "Graphics Quality:";
        qualityLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(qualityLabel);
        
        _qualityOptionButton = new OptionButton();
        _qualityOptionButton.AddItem("Low");
        _qualityOptionButton.AddItem("Medium");
        _qualityOptionButton.AddItem("High");
        _qualityOptionButton.Selected = 1; // Medium
        _qualityOptionButton.CustomMinimumSize = new Vector2(200, 40);
        _qualityOptionButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(_qualityOptionButton);
        
        // Spacer
        var spacer4 = new Control();
        spacer4.CustomMinimumSize = new Vector2(0, 50);
        vbox.AddChild(spacer4);
        
        // Save Button
        _saveButton = new Button();
        _saveButton.Text = "Save Settings";
        _saveButton.CustomMinimumSize = new Vector2(200, 50);
        _saveButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _saveButton.Pressed += OnSavePressed;
        vbox.AddChild(_saveButton);
        
        // Back Button
        _backButton = new Button();
        _backButton.Text = "Back";
        _backButton.CustomMinimumSize = new Vector2(200, 50);
        _backButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _backButton.Pressed += OnBackPressed;
        vbox.AddChild(_backButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    private void OnSavePressed()
    {
        // Save settings logic would go here
        GD.Print($"Settings saved - Volume: {_volumeSlider.Value}, Fullscreen: {_fullscreenCheckBox.ButtonPressed}, Quality: {_qualityOptionButton.GetItemText(_qualityOptionButton.Selected)}");
        
        // Show a confirmation modal
        Navigator.ShowModal<ConfirmationWidget>(confirmationWidget => 
        {
            confirmationWidget.SetMessage("Settings saved successfully!");
        });
    }
    
    private void OnBackPressed()
    {
        Navigator.Pop();
    }
} 