// Created by Copilot on 2026-03-10.
// Virtual Button for mobile touch input in Godot 4.x.

using Godot;
using System;

namespace VirtualJoystickPlugin
{
    /// <summary>
    /// A virtual button for mobile touch input.
    /// Can be mapped to Godot input actions and supports visual feedback.
    /// </summary>
    [Tool]
    [GlobalClass]
    public partial class VirtualButton : Control
    {
        #region Exported Properties

        [ExportGroup("Input")]

        [Export] public string Action { get; set; } = "";

        [ExportGroup("Appearance")]

        [Export] public Texture2D NormalTexture { get; set; }
        [Export] public Texture2D PressedTexture { get; set; }

        [Export]
        public float ButtonRadius
        {
            get => _buttonRadius;
            set { _buttonRadius = Mathf.Max(value, 10f); _UpdateMinimumSize(); QueueRedraw(); }
        }

        [Export] public Color NormalColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        [Export] public Color PressedColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 0.9f);
        [Export] public Color IconColor { get; set; } = Colors.White;

        [Export] public string Label { get; set; } = "";
        [Export] public int LabelFontSize { get; set; } = 20;

        [Export(PropertyHint.Range, "0,1,0.01")]
        public float PressedScale { get; set; } = 0.9f;

        #endregion

        #region Signals

        [Signal] public delegate void ButtonDownEventHandler();
        [Signal] public delegate void ButtonUpEventHandler();

        #endregion

        #region Private Fields

        private float _buttonRadius = 40f;
        private bool _isPressed;
        private int _touchIndex = -1;

        #endregion

        #region Public API

        public bool IsPressed => _isPressed;

        #endregion

        #region Lifecycle

        public override void _Ready()
        {
            _UpdateMinimumSize();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touchEvent)
            {
                _HandleTouch(touchEvent);
            }
            else if (@event is InputEventScreenDrag dragEvent)
            {
                _HandleDrag(dragEvent);
            }
        }

        public override void _Draw()
        {
            var center = Size / 2f;
            var currentRadius = _isPressed ? _buttonRadius * PressedScale : _buttonRadius;
            var currentColor = _isPressed ? PressedColor : NormalColor;

            if (_isPressed && PressedTexture != null)
            {
                _DrawTexture(PressedTexture, center, currentRadius, currentColor);
            }
            else if (NormalTexture != null)
            {
                _DrawTexture(NormalTexture, center, currentRadius, currentColor);
            }            else
            {
                // Draw default circle button with antialiasing
                DrawCircle(center, currentRadius, currentColor, true, -1f, true);

                // Outer ring with antialiasing
                var ringColor = new Color(currentColor.R + 0.15f, currentColor.G + 0.15f, currentColor.B + 0.15f, currentColor.A);
                DrawArc(center, currentRadius, 0f, Mathf.Tau, 128, ringColor, 2f, true);

                // Inner highlight
                if (_isPressed)
                {
                    var highlightColor = new Color(1f, 1f, 1f, 0.15f);
                    DrawCircle(center, currentRadius * 0.7f, highlightColor, true, -1f, true);
                }
            }

            // Draw label text
            if (!string.IsNullOrEmpty(Label))
            {
                _DrawLabel(center);
            }
        }

        public override Vector2 _GetMinimumSize()
        {
            return new Vector2(_buttonRadius * 2f, _buttonRadius * 2f);
        }

        #endregion

        #region Input Handling

        private void _HandleTouch(InputEventScreenTouch touch)
        {
            if (touch.Pressed)
            {
                if (_isPressed) return;

                var localPos = touch.Position - GlobalPosition;
                var center = Size / 2f;
                if (localPos.DistanceTo(center) <= _buttonRadius)
                {
                    _Press(touch.Index);
                }
            }
            else
            {
                if (touch.Index == _touchIndex)
                {
                    _Release();
                }
            }
        }

        private void _HandleDrag(InputEventScreenDrag drag)
        {
            if (drag.Index != _touchIndex) return;

            // Release if finger leaves button area (with some tolerance)
            var localPos = drag.Position - GlobalPosition;
            var center = Size / 2f;
            if (localPos.DistanceTo(center) > _buttonRadius * 1.5f)
            {
                _Release();
            }
        }

        private void _Press(int index)
        {
            _touchIndex = index;
            _isPressed = true;

            if (!Engine.IsEditorHint() && !string.IsNullOrEmpty(Action) && InputMap.HasAction(Action))
            {
                Input.ActionPress(Action);
            }

            EmitSignal(SignalName.ButtonDown);
            QueueRedraw();
        }

        private void _Release()
        {
            _isPressed = false;
            _touchIndex = -1;

            if (!Engine.IsEditorHint() && !string.IsNullOrEmpty(Action) && InputMap.HasAction(Action))
            {
                Input.ActionRelease(Action);
            }

            EmitSignal(SignalName.ButtonUp);
            QueueRedraw();
        }

        #endregion

        #region Drawing Helpers

        private void _DrawTexture(Texture2D texture, Vector2 center, float radius, Color modulate)
        {
            var texSize = texture.GetSize();
            var scale = (radius * 2f) / Mathf.Max(texSize.X, texSize.Y);
            var drawPos = center - texSize * scale / 2f;
            DrawTextureRect(texture, new Rect2(drawPos, texSize * scale), false, modulate);
        }

        private void _DrawLabel(Vector2 center)
        {
            var font = ThemeDB.FallbackFont;
            if (font == null) return;

            var textSize = font.GetStringSize(Label, HorizontalAlignment.Center, -1, LabelFontSize);
            var textPos = center - new Vector2(textSize.X / 2f, -textSize.Y / 4f);
            DrawString(font, textPos, Label, HorizontalAlignment.Center, -1, LabelFontSize, IconColor);        }

        private void _UpdateMinimumSize()
        {
            CustomMinimumSize = _GetMinimumSize();
        }

        #endregion

        #region Cleanup

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete && !Engine.IsEditorHint())
            {
                if (!string.IsNullOrEmpty(Action) && InputMap.HasAction(Action) && Input.IsActionPressed(Action))
                {
                    Input.ActionRelease(Action);
                }
            }
        }

        #endregion
    }
}
