using Godot;
using Navigation;

public partial class ConfirmationWidget : Widget
{
    private Label _messageLabel;
    private Button _okButton;
    private string _message = "Operation completed successfully!";
    
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
        vbox.CustomMinimumSize = new Vector2(300, 200);
        AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "Confirmation";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(title);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer1);
        
        // Message Label
        _messageLabel = new Label();
        _messageLabel.Text = _message;
        _messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _messageLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _messageLabel.CustomMinimumSize = new Vector2(250, 80);
        _messageLabel.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(_messageLabel);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer2);
        
        // OK Button
        _okButton = new Button();
        _okButton.Text = "OK";
        _okButton.CustomMinimumSize = new Vector2(100, 40);
        _okButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _okButton.Pressed += OnOkPressed;
        vbox.AddChild(_okButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    public void SetMessage(string message)
    {
        _message = message;
        if (_messageLabel != null)
        {
            _messageLabel.Text = message;
        }
    }
    
    private void OnOkPressed()
    {
        Navigator.Pop();
    }
} 