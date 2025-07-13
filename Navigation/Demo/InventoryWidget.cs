using Godot;
using Navigation;

public partial class InventoryWidget : Widget
{
    private Label _scoreLabel;
    private ItemList _itemList;
    private Button _useItemButton;
    private Button _backButton;
    private int _playerScore;
    
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
        title.Text = "Inventory";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 28);
        vbox.AddChild(title);
        
        // Score Label
        _scoreLabel = new Label();
        _scoreLabel.Text = "Player Score: 0";
        _scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _scoreLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(_scoreLabel);
        
        // Spacer
        var spacer1 = new Control();
        spacer1.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer1);
        
        // Item List
        _itemList = new ItemList();
        _itemList.AddItem("Health Potion (Restores 50 HP)");
        _itemList.AddItem("Mana Potion (Restores 30 MP)");
        _itemList.AddItem("Strength Elixir (+10 Attack)");
        _itemList.AddItem("Magic Scroll (Fireball Spell)");
        _itemList.AddItem("Gold Coin (Worth 100 gold)");
        _itemList.CustomMinimumSize = new Vector2(350, 200);
        _itemList.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        vbox.AddChild(_itemList);
        
        // Spacer
        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 20);
        vbox.AddChild(spacer2);
        
        // Use Item Button
        _useItemButton = new Button();
        _useItemButton.Text = "Use Selected Item";
        _useItemButton.CustomMinimumSize = new Vector2(200, 50);
        _useItemButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _useItemButton.Pressed += OnUseItemPressed;
        vbox.AddChild(_useItemButton);
        
        // Back Button
        _backButton = new Button();
        _backButton.Text = "Back to Game";
        _backButton.CustomMinimumSize = new Vector2(200, 50);
        _backButton.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _backButton.Pressed += OnBackPressed;
        vbox.AddChild(_backButton);
        
        // Center the VBox
        vbox.Position = (GetViewport().GetVisibleRect().Size - vbox.CustomMinimumSize) / 2;
    }
    
    public void SetPlayerScore(int score)
    {
        _playerScore = score;
        _scoreLabel.Text = $"Player Score: {score}";
    }
    
    private void OnUseItemPressed()
    {
        var selectedIndices = _itemList.GetSelectedItems();
        if (selectedIndices.Length > 0)
        {
            var selectedItem = _itemList.GetItemText(selectedIndices[0]);
            GD.Print($"Using item: {selectedItem}");
            
            // Show a confirmation modal
            Navigator.ShowModal<ConfirmationWidget>(confirmationWidget => 
            {
                confirmationWidget.SetMessage($"Used: {selectedItem}");
            });
        }
    }
    
    private void OnBackPressed()
    {
        Navigator.Pop();
    }
} 