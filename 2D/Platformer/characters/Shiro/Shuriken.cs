using Godot;
using System;

public partial class Shuriken : Area2D
{
    [Export] public float Speed { get; set; } = 800f;
    [Export] public float Lifespan { get; set; } = 5.0f;
    [Export] public float AfterimageInterval { get; set; } = 0.05f;
    [Export] public float AfterimageDuration { get; set; } = 0.2f;
    [Export] public Color AfterimageColor { get; set; } = new Color(0.1f, 0.5f, 1f, 0.6f);

    public Vector2 Direction { get; set; } = Vector2.Right;

    private bool _stuck = false;
    private float _afterimageTimer = 0f;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");

        // Face the correct direction based on throw direction
        if (Direction.X < 0)
        {
            _sprite.FlipH = true;
        }
        else
        {
            _sprite.FlipH = false;
        }

        // Connect collision signal
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_stuck)
            return;

        float dt = (float)delta;
        
        // Move
        Position += Direction * Speed * dt;

        // Spawn afterimages
        _afterimageTimer -= dt;
        if (_afterimageTimer <= 0f)
        {
            SpawnAfterimage();
            _afterimageTimer = AfterimageInterval;
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_stuck) return;

        // Check if hitting environment (StaticBody2D or TileMapLayer)
        if (body is StaticBody2D || body is TileMapLayer || body is TileMap)
        {
            StickToSurface();
        }
    }

    private void StickToSurface()
    {
        _stuck = true;

        // Disable further collisions
        SetDeferred(Area2D.PropertyName.Monitoring, false);
        SetDeferred(Area2D.PropertyName.Monitorable, false);

        // Start stick lifespan timer
        GetTree().CreateTimer(Lifespan).Timeout += () =>
        {
            if (IsInstanceValid(this))
                QueueFree();
        };
    }

    private void SpawnAfterimage()
    {
        if (_sprite == null || _sprite.Texture == null) return;

        var ghost = new Sprite2D
        {
            Texture = _sprite.Texture,
            FlipH = _sprite.FlipH,
            GlobalPosition = _sprite.GlobalPosition,
            Modulate = AfterimageColor,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };

        // Add to main scene tree so it stays behind when shuriken moves/dies
        GetTree().CurrentScene.AddChild(ghost);

        var tween = ghost.CreateTween();
        tween.TweenProperty(ghost, "modulate:a", 0.0f, AfterimageDuration);
        tween.TweenCallback(Callable.From(() => ghost.QueueFree()));
    }
}
