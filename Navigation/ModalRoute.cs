using System;
using Godot;
using static Godot.Node;

namespace Navigation
{
    public class ModalRoute<T>: Route<T> where T: Widget, new()
    {
        public bool maskDismissible = true;
        public Color maskColor = new Color(0, 0, 0, 0.5f);
        public Vector2 offset = Vector2.Zero;
        
        // Constructor with all parameters
        public ModalRoute(Action<T> builder = null, bool maskDismissible = true, Color? maskColor = null, Vector2 offset = default) : base(builder)
        {
            this.maskDismissible = maskDismissible;
            this.maskColor = maskColor ?? this.maskColor;
            this.offset = offset;
        }
        
        public override void OnPush()
        {
            var widget = To;
            
            // Disable the previous widget
            if (From != null)
            {
                From.ProcessMode = ProcessModeEnum.Disabled;
                From.Visible = false;
            }
            
            // Add modal mask
            var mask = new ModalMask();
            mask.color = maskColor;
            mask.MouseFilter = maskDismissible ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
            widget.AddChild(mask);
            widget.MoveChild(mask, 0);
            
            // Set stretch
            var rect = widget;
            if (rect != null)
            {
                rect.Size = Vector2.Zero;
                rect.AnchorTop = 0;
                rect.AnchorBottom = 1;
                rect.AnchorLeft = 0;
                rect.AnchorRight = 1;
                rect.Position += offset;
            }
        }
        
        public override void OnPop()
        {
            if (From != null)
            {
                From.Visible = true;
                From.ProcessMode = ProcessModeEnum.Inherit;
            }
        }
    }
} 