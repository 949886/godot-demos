extends Area2D

@export var speed := 1000.0
@export var lifespan := 5.0
@export var afterimage_interval := 0.033334
@export var afterimage_duration := 0.25
@export var afterimage_color := Color(0.1, 0.5, 1.0, 0.6)

var direction := Vector2.RIGHT
var is_stuck := false
var stick_normal := Vector2.ZERO

var _afterimage_timer := 0.0
var _sprite: Sprite2D

func _ready() -> void:
	_sprite = get_node("Sprite2D")
	rotation = direction.angle()

func _physics_process(delta: float) -> void:
	if is_stuck:
		return

	var movement := direction * speed * delta
	var space_state := get_world_2d().direct_space_state
	var query := PhysicsRayQueryParameters2D.create(global_position, global_position + movement, collision_mask)
	query.collide_with_areas = false
	query.collide_with_bodies = true

	var result: Dictionary = space_state.intersect_ray(query)
	if not result.is_empty():
		var collider: Variant = result["collider"]
		if collider is StaticBody2D or collider is TileMap or collider is TileMapLayer:
			global_position = result["position"] as Vector2
			stick_normal = result["normal"] as Vector2
			_stick_to_surface()
			return

	position += movement

	_afterimage_timer -= delta
	if _afterimage_timer <= 0.0:
		_spawn_afterimage()
		_afterimage_timer = afterimage_interval

func set_direction(value: Vector2) -> void:
	direction = value.normalized()
	rotation = direction.angle()

func _stick_to_surface() -> void:
	is_stuck = true
	set_deferred("monitoring", false)
	set_deferred("monitorable", false)
	get_tree().create_timer(lifespan).timeout.connect(func():
		if is_instance_valid(self):
			queue_free()
	)

func _spawn_afterimage() -> void:
	if _sprite == null or _sprite.texture == null:
		return

	var ghost := Sprite2D.new()
	ghost.texture = _sprite.texture
	ghost.global_position = _sprite.global_position
	ghost.rotation = rotation
	ghost.modulate = afterimage_color
	ghost.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	get_tree().current_scene.add_child(ghost)

	var tween := ghost.create_tween()
	tween.tween_property(ghost, "modulate:a", 0.0, afterimage_duration)
	tween.finished.connect(func(): ghost.queue_free())
