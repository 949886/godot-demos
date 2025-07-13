using Godot;

namespace Navigation
{
    public partial class ModalMask : Control
    {
        public Color color = new Color(0, 0, 0, 0.5f);
        
        public override void _Ready()
        {
            // Set stretch
            Size = Vector2.Zero;
            AnchorTop = 0;
            AnchorBottom = 1;
            AnchorLeft = 0;
            AnchorRight = 1;
            
            // Set mouse filter to stop events
            MouseFilter = MouseFilterEnum.Stop;
        }
        
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Close modal
                Navigator.Pop();
            }
        }
        
        public override void _Draw()
        {
            // Draw the mask color
            DrawRect(GetRect(), color);
        }
    }
} 