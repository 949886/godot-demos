extends CharacterBody2D

#region Exports

@export var animated_sprite: AnimatedSprite2D
@export var animation_player: AnimationPlayer
@export var animation_tree: AnimationTree

var _pending_throw_angle = null

#endregion

#region Movement Parameters

@export_group("Movement")
@export var move_speed: float = 200.0
@export var acceleration: float = 1200.0
@export var friction: float = 1000.0
@export var air_acceleration: float = 600.0
@export var air_friction: float = 200.0

#endregion

#region Jump Parameters

@export_group("Jump")
@export var enable_double_jump: bool = true
@export var jump_velocity: float = -400.0
@export var double_jump_velocity: float = -350.0
@export var gravity: float = 980.0
@export var max_fall_speed: float = 600.0
@export var jump_cut_multiplier: float = 0.5

#endregion

#region Dash Parameters

@export_group("Dash")
@export var dash_speed: float = 1000.0
@export var dash_duration: float = 0.15
@export var dash_cooldown: float = 0.8
@export var max_dash_charges: int = 2
@export var afterimage_fade_duration: float = 0.3
@export var afterimage_color: Color = Color(0.4, 0.8, 1.0, 0.6)

#endregion

#region Wall Jump Parameters

@export_group("Wall Jump")
@export var enable_wall_jump: bool = true
@export var wall_slide_gravity: float = 150.0
@export var wall_jump_horizontal_speed: float = 300.0
@export var wall_jump_vertical_speed: float = -380.0

#endregion

#region Attack Parameters

@export_group("Attack")
@export var heavy_attack_hold_time: float = 0.3
@export var jump_attack_lift: float = 150.0
@export var shuriken_scene: PackedScene
@export var shuriken_spawn_offset: Vector2 = Vector2(10, -15)

@export_group("Flying Thunder God")
@export var teleport_afterimage_count: int = 6
@export var teleport_afterimage_fade_duration: float = 0.16
@export var teleport_afterimage_color: Color = Color(1.0, 0.35, 0.35, 0.72)
@export var teleport_flash_color: Color = Color(1.0, 0.18, 0.18, 0.95)
@export var teleport_flash_core_color: Color = Color(1.0, 1.0, 1.0, 0.98)
@export var teleport_flash_width: float = 14.0
@export var teleport_flash_duration: float = 0.05
@export var teleport_spark_count: int = 18
@export var teleport_spark_scatter: float = 18.0
@export var teleport_spark_duration: float = 0.16
@export var teleport_arrival_offset: float = 12.0

#endregion

#region State Machine

enum State {
	Idle,
	IdleToRun,
	Run,
	RunToIdle,
	Jump,
	JumpToFall,
	DoubleJump,
	Fall,
	Landing,
	FallToIdle,
	Attack1,
	Attack2,
	Attack3,
	HeavyAttack,
	Dash,
	WallSlide,
	JumpAttack,
	Throw,
	AirThrow,
	Die
}

var _current_state: State = State.Idle
var _facing_direction: int = 1 # 1 = right, -1 = left
var _has_double_jump: bool = true
var _has_jump_attack: bool = true

# Dash tracking
var _dash_charges: int
var _dash_recharge_timer: float
var _dash_timer: float

# Attack tracking
var _attack_press_time: float = 0.0
var _attack_button_held: bool = false
var _combo_requested: bool = false
var _heavy_attack_triggered: bool = false

# Wall slide tracking
var _wall_direction: int = 0 # -1 = wall on left, 1 = wall on right

# Animations that should loop (all others play once)
const LOOPING_ANIMATIONS = ["idle", "run", "fall"]

var _active_shuriken: Node2D = null
var _rng = RandomNumberGenerator.new()

var dash_charges: int:
	get:
		return _dash_charges

var dash_recharge_progress: float:
	get:
		if _dash_charges >= max_dash_charges or dash_cooldown <= 0.0:
			return 0.0
		return clampf(_dash_recharge_timer / dash_cooldown, 0.0, 1.0)

#endregion

func _ready() -> void:
	# Auto-find nodes by relative path if not assigned via export
	if not animated_sprite: animated_sprite = get_node_or_null("AnimatedSprite2D")
	if not animation_player: animation_player = get_node_or_null("AnimationPlayer")
	if not animation_tree: animation_tree = get_node_or_null("AnimationTree")

	# Disable AnimationTree — it overrides AnimationPlayer.Play() calls.
	if animation_tree:
		animation_tree.active = false

	if animation_player:
		animation_player.animation_finished.connect(_on_animation_finished)

	_dash_charges = max_dash_charges
	_rng.randomize()

	# Start in idle
	change_state(State.Idle)

func _physics_process(delta: float) -> void:
	var dt = delta

	# Recharge dash charges
	if _dash_charges < max_dash_charges:
		_dash_recharge_timer -= dt
		if _dash_recharge_timer <= 0.0:
			_dash_charges += 1
			_dash_recharge_timer = dash_cooldown

	# Press throw again while a shuriken exists to teleport to it.
	if Input.is_action_just_pressed("throw") and try_flying_thunder_god_teleport():
		return

	# Handle state-specific logic
	match _current_state:
		State.Idle: process_idle(dt)
		State.IdleToRun: process_idle_to_run(dt)
		State.Run: process_run(dt)
		State.RunToIdle: process_run_to_idle(dt)
		State.Jump: process_jump(dt)
		State.JumpToFall: process_jump_to_fall(dt)
		State.DoubleJump: process_double_jump(dt)
		State.Fall: process_fall(dt)
		State.Landing: process_landing(dt)
		State.WallSlide: process_wall_slide(dt)
		State.JumpAttack: process_jump_attack(dt)
		State.Throw: process_throw(dt)
		State.AirThrow: process_air_throw(dt)
		State.Attack1, State.Attack2, State.Attack3: process_attack(dt)
		State.HeavyAttack: process_heavy_attack(dt)
		State.Dash: process_dash(dt)

	move_and_slide()

func _input(event: InputEvent) -> void:
	# Track attack button press/release for heavy attack detection
	if event.is_action_pressed("attack"):
		_attack_press_time = 0.0
		_attack_button_held = true
		_heavy_attack_triggered = false
	elif event.is_action_released("attack"):
		_attack_button_held = false

#region State Processors

func process_idle(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, true)

	# Heavy attack detection (hold mouse)
	if _attack_button_held:
		_attack_press_time += dt
		if _attack_press_time >= heavy_attack_hold_time and not _heavy_attack_triggered:
			_heavy_attack_triggered = true
			change_state(State.HeavyAttack)
			return

	# Short press attack — trigger on button release
	if Input.is_action_just_released("attack") and not _heavy_attack_triggered:
		change_state(State.Attack1)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.Throw)
		return

	if Input.is_action_just_pressed("jump") and is_on_floor():
		# Drop through one-way platform: S + Space
		if Input.is_action_pressed("move_down") and try_drop_through_platform():
			change_state(State.Fall)
			return
		change_state(State.Jump)
		return

	# Fall off edge
	if not is_on_floor():
		change_state(State.Fall)
		return

	var input_dir = get_move_input()
	if abs(input_dir) > 0.1:
		update_facing(input_dir)
		change_state(State.IdleToRun)

func process_idle_to_run(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, true)

	if not is_on_floor():
		change_state(State.Fall)
		return

	if Input.is_action_just_pressed("jump"):
		change_state(State.Jump)
		return

func process_run(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, true)

	# Heavy attack detection
	if _attack_button_held:
		_attack_press_time += dt
		if _attack_press_time >= heavy_attack_hold_time and not _heavy_attack_triggered:
			_heavy_attack_triggered = true
			change_state(State.HeavyAttack)
			return

	if Input.is_action_just_released("attack") and not _heavy_attack_triggered:
		change_state(State.Attack1)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.Throw)
		return

	if not is_on_floor():
		change_state(State.Fall)
		return

	if Input.is_action_just_pressed("jump"):
		# Drop through one-way platform: S + Space
		if Input.is_action_pressed("move_down") and try_drop_through_platform():
			change_state(State.Fall)
			return
		change_state(State.Jump)
		return

	var input_dir = get_move_input()
	if abs(input_dir) < 0.1:
		change_state(State.RunToIdle)
		return

	update_facing(input_dir)

func process_run_to_idle(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, true)

	if not is_on_floor():
		change_state(State.Fall)
		return

	if Input.is_action_just_pressed("jump"):
		change_state(State.Jump)
		return

	var input_dir = get_move_input()
	if abs(input_dir) > 0.1:
		update_facing(input_dir)
		change_state(State.IdleToRun)

func process_jump(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	# Variable jump height
	if Input.is_action_just_released("jump") and velocity.y < 0:
		velocity.y *= jump_cut_multiplier

	if Input.is_action_just_pressed("jump") and _has_double_jump and enable_double_jump:
		change_state(State.DoubleJump)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.AirThrow)
		return

	if Input.is_action_just_pressed("attack") and _has_jump_attack:
		change_state(State.JumpAttack)
		return

	# Transition to fall when starting to descend
	if velocity.y > 0:
		change_state(State.JumpToFall)
		return

	if is_on_floor():
		change_state(State.Landing)

func process_jump_to_fall(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	if Input.is_action_just_pressed("jump") and _has_double_jump and enable_double_jump:
		change_state(State.DoubleJump)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.AirThrow)
		return

	if Input.is_action_just_pressed("attack") and _has_jump_attack:
		change_state(State.JumpAttack)
		return

	if is_on_floor():
		change_state(State.Landing)
		return

	detect_wall_slide()

func process_double_jump(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	if Input.is_action_just_released("jump") and velocity.y < 0:
		velocity.y *= jump_cut_multiplier

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.AirThrow)
		return

	if Input.is_action_just_pressed("attack") and _has_jump_attack:
		change_state(State.JumpAttack)
		return

	if velocity.y > 0:
		change_state(State.Fall)
		return

	if is_on_floor():
		change_state(State.Landing)

func process_fall(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	if Input.is_action_just_pressed("jump") and _has_double_jump and enable_double_jump:
		change_state(State.DoubleJump)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.AirThrow)
		return

	if Input.is_action_just_pressed("attack") and _has_jump_attack:
		change_state(State.JumpAttack)
		return

	if is_on_floor():
		change_state(State.Landing)
		return

	# Wall slide detection
	detect_wall_slide()

func process_wall_slide(dt: float) -> void:
	# Slow gravity while on wall
	velocity.y = min(velocity.y + wall_slide_gravity * dt, wall_slide_gravity)
	velocity.x = 0

	# Face away from wall
	update_facing(-_wall_direction)

	# Wall jump
	if Input.is_action_just_pressed("jump"):
		velocity.x = -_wall_direction * wall_jump_horizontal_speed
		velocity.y = wall_jump_vertical_speed
		_has_double_jump = true # Restore double jump
		update_facing(-_wall_direction)
		change_state(State.Jump)
		return

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		change_state(State.Dash)
		return

	if Input.is_action_just_pressed("throw"):
		change_state(State.AirThrow)
		return

	# Let go of wall
	var input_dir = get_move_input()
	var still_on_wall = is_on_wall() and \
					((_wall_direction == -1 and input_dir < -0.1) or (_wall_direction == 1 and input_dir > 0.1))

	if not still_on_wall:
		change_state(State.Fall)
		return

	if is_on_floor():
		change_state(State.Landing)

func process_landing(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, true)

	# Allow player to cancel landing animation with movement or jump
	if Input.is_action_just_pressed("jump") and is_on_floor():
		change_state(State.Jump)
		return

	var input_dir = get_move_input()
	if abs(input_dir) > 0.1:
		update_facing(input_dir)
		change_state(State.IdleToRun)
		return

func process_attack(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, is_on_floor())

	# Dash cancels attack — leave afterimage
	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		spawn_afterimage()
		change_state(State.Dash)
		return

	# Jump cancels attack — leave afterimage
	if Input.is_action_just_pressed("jump") and is_on_floor():
		spawn_afterimage()
		change_state(State.Jump)
		return

	# Buffer combo input during attack animation
	if Input.is_action_just_released("attack") and not _heavy_attack_triggered:
		_combo_requested = true

func process_heavy_attack(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, is_on_floor())

	# Dash cancels heavy attack — leave afterimage
	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		spawn_afterimage()
		change_state(State.Dash)
		return

	# Jump cancels heavy attack — leave afterimage
	if Input.is_action_just_pressed("jump") and is_on_floor():
		spawn_afterimage()
		change_state(State.Jump)
		return

func process_jump_attack(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		spawn_afterimage()
		change_state(State.Dash)
		return

	if is_on_floor():
		change_state(State.Landing)
		return

func process_throw(dt: float) -> void:
	apply_gravity(dt)
	apply_friction(dt, is_on_floor())

	# Dash cancels throw
	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		spawn_afterimage()
		change_state(State.Dash)
		return

	if not is_on_floor():
		change_state(State.AirThrow)
		return

func process_air_throw(dt: float) -> void:
	apply_gravity(dt)
	apply_movement(dt, false)

	# Dash cancels air throw
	if Input.is_action_just_pressed("dash") and _dash_charges > 0:
		spawn_afterimage()
		change_state(State.Dash)
		return

	if is_on_floor():
		change_state(State.Landing)
		return

func instantiate_shuriken(override_angle = null) -> void:
	if not shuriken_scene:
		push_error("Shuriken Scene is null! You need to drag 'Shuriken.tscn' into the 'Shuriken Scene' property in the Inspector on your character!")
		return

	if is_instance_valid(_active_shuriken):
		_active_shuriken.queue_free()

	var shuriken = shuriken_scene.instantiate()
	get_tree().current_scene.add_child(shuriken)

	var flip_offset = Vector2(shuriken_spawn_offset.x * _facing_direction, shuriken_spawn_offset.y)
	shuriken.global_position = global_position + flip_offset

	var direction: Vector2
	if override_angle != null:
		direction = Vector2.RIGHT.rotated(override_angle)
	else:
		var mouse_pos = get_global_mouse_position()
		direction = (mouse_pos - shuriken.global_position).normalized()

	if shuriken.has_method("set_direction"):
		shuriken.set_direction(direction)
	else:
		shuriken.set("direction", direction)

	shuriken.rotation = direction.angle()
	_active_shuriken = shuriken
	shuriken.tree_exiting.connect(func(): if _active_shuriken == shuriken: _active_shuriken = null)

func process_dash(dt: float) -> void:
	_dash_timer -= dt

	# Override velocity during dash (no gravity)
	velocity.x = _facing_direction * dash_speed
	velocity.y = 0

	if _dash_timer <= 0.0:
		# Kill dash momentum so the character doesn't slide
		velocity = Vector2.ZERO

		if is_on_floor():
			change_state(State.Idle)
		else:
			change_state(State.Fall)

#endregion

#region State Transitions

func change_state(new_state: State) -> void:
	var previous_state = _current_state
	_current_state = new_state

	match new_state:
		State.Idle:
			play_animation("idle")
			_has_double_jump = true
			_has_jump_attack = true

		State.IdleToRun:
			play_animation("idle_to_run")

		State.Run:
			play_animation("run")

		State.RunToIdle:
			play_animation("run_to_idle")

		State.Jump:
			# Wall jump already sets velocity, only set jump velocity for ground jumps
			if previous_state != State.WallSlide:
				velocity.y = jump_velocity
			play_animation("jump")

		State.JumpToFall:
			play_animation("jump_to_fall")

		State.DoubleJump:
			_has_double_jump = false
			velocity.y = double_jump_velocity
			play_animation("double_jump")

		State.Fall:
			play_animation("fall")

		State.Landing:
			_has_double_jump = true
			_has_jump_attack = true
			play_animation("landing")

		State.FallToIdle:
			play_animation("fall_to_idle")

		State.Attack1:
			_combo_requested = false
			play_animation("attack1")

		State.Attack2:
			_combo_requested = false
			play_animation("attack2")

		State.Attack3:
			_combo_requested = false
			play_animation("attack3")

		State.HeavyAttack:
			play_animation("heavy_attack")

		State.Dash:
			_dash_charges -= 1
			_dash_recharge_timer = dash_cooldown
			_dash_timer = dash_duration
			play_animation("dash")

		State.JumpAttack:
			_has_jump_attack = false
			velocity.y = -jump_attack_lift
			play_animation("jump_attack")

		State.Throw:
			if _pending_throw_angle != null:
				update_facing(cos(_pending_throw_angle))
				play_animation("shuriken")
				instantiate_shuriken(_pending_throw_angle)
				_pending_throw_angle = null
			else:
				var mouse_pos = get_global_mouse_position()
				update_facing(mouse_pos.x - global_position.x)
				play_animation("shuriken")
				instantiate_shuriken()

		State.AirThrow:
			if _pending_throw_angle != null:
				update_facing(cos(_pending_throw_angle))
				play_animation("shuriken_air")
				instantiate_shuriken(_pending_throw_angle)
				_pending_throw_angle = null
			else:
				var mouse_pos = get_global_mouse_position()
				update_facing(mouse_pos.x - global_position.x)
				play_animation("shuriken_air")
				instantiate_shuriken()

		State.WallSlide:
			_has_jump_attack = true
			play_animation("fall") # Reuse fall animation for wall slide

#endregion

#region Flying Thunder God

func try_flying_thunder_god_teleport() -> bool:
	if not is_instance_valid(_active_shuriken):
		return false

	var start_pos = global_position
	var target_pos = _active_shuriken.global_position

	if _active_shuriken.get("is_stuck"):
		target_pos += _active_shuriken.get("stick_normal") * teleport_arrival_offset

	spawn_teleport_trail(start_pos, target_pos)
	spawn_teleport_flash(start_pos, target_pos)

	global_position = target_pos
	velocity = Vector2.ZERO

	var delta_x = target_pos.x - start_pos.x
	if abs(delta_x) > 0.01:
		update_facing(delta_x)

	if is_instance_valid(_active_shuriken):
		_active_shuriken.queue_free()

	change_state(State.Idle if is_on_floor() else State.Fall)
	return true

func spawn_teleport_trail(from: Vector2, to: Vector2) -> void:
	if not animated_sprite or not animated_sprite.sprite_frames:
		return

	var texture = animated_sprite.sprite_frames.get_frame_texture(animated_sprite.animation, animated_sprite.frame)
	if not texture: return

	var count = max(2, teleport_afterimage_count)
	for i in range(count):
		var t = float(i) / (count - 1)
		var ghost_color = teleport_afterimage_color
		ghost_color.a *= (1.0 - t * 0.6)

		var ghost = Sprite2D.new()
		ghost.texture = texture
		ghost.flip_h = animated_sprite.flip_h
		ghost.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		ghost.global_position = from.lerp(to, t)
		ghost.modulate = ghost_color

		get_tree().current_scene.add_child(ghost)

		var tween = ghost.create_tween()
		tween.tween_property(ghost, "modulate:a", 0.0, teleport_afterimage_fade_duration)
		tween.finished.connect(func(): ghost.queue_free())

func spawn_teleport_flash(from: Vector2, to: Vector2) -> void:
	var diff = to - from
	if diff.length_squared() < 0.0001: return

	var dir = diff.normalized()
	var normal = dir.orthogonal()
	var flash_root = Node2D.new()
	get_tree().current_scene.add_child(flash_root)

	# Outer glow layers for impact.
	var glow_wide_color = teleport_flash_color
	glow_wide_color.a = 0.35
	var glow_mid_color = teleport_flash_color
	glow_mid_color.a = 0.72

	# Sharp wedge profile: needle-like start and much thicker destination.
	var glow_wide = create_flash_line(from + normal * 2.0, to + normal * 2.0, teleport_flash_width * 2.9, glow_wide_color, 0.04, 2.1)
	var glow_mid = create_flash_line(from - normal * 1.5, to - normal * 1.5, teleport_flash_width * 2.0, glow_mid_color, 0.045, 2.25)
	var core = create_flash_line(from, to, teleport_flash_width * 0.21, teleport_flash_core_color, 0.03, 3.8)
	var core_hot = create_flash_line(from, to, teleport_flash_width * 0.11, Color.WHITE, 0.02, 4.2)

	flash_root.add_child(glow_wide)
	flash_root.add_child(glow_mid)
	flash_root.add_child(core)
	flash_root.add_child(core_hot)

	spawn_teleport_sparks(flash_root, from, to, dir, normal)

	var tween = flash_root.create_tween().set_parallel(true)
	tween.tween_property(flash_root, "modulate:a", 0.0, teleport_flash_duration)
	tween.tween_property(glow_wide, "width", 0.0, teleport_flash_duration)
	tween.tween_property(glow_mid, "width", 0.0, teleport_flash_duration)
	tween.tween_property(core, "width", 0.0, teleport_flash_duration)
	tween.tween_property(core_hot, "width", 0.0, teleport_flash_duration)
	tween.set_parallel(false)
	tween.finished.connect(func(): flash_root.queue_free())

func create_flash_line(from: Vector2, to: Vector2, w: float, color: Color, start_scale: float, end_scale: float) -> Line2D:
	var line = Line2D.new()
	line.width = w
	line.default_color = color
	line.antialiased = true

	var curve = Curve.new()
	curve.add_point(Vector2(0, max(0.01, start_scale)))
	curve.add_point(Vector2(1, max(0.01, end_scale)))
	line.width_curve = curve

	line.add_point(from)
	line.add_point(to)
	return line

func spawn_teleport_sparks(parent: Node2D, from: Vector2, to: Vector2, dir: Vector2, normal: Vector2) -> void:
	for i in range(teleport_spark_count):
		var t = _rng.randf()
		var side = _rng.randf_range(-teleport_spark_scatter, teleport_spark_scatter)
		var center = from.lerp(to, t) + normal * side

		var length = _rng.randf_range(8, 20)
		var angle = _rng.randf_range(-0.9, 0.9)
		var spark_dir = dir.rotated(angle)
		var p1 = center - spark_dir * (length * 0.5)
		var p2 = center + spark_dir * (length * 0.5)

		var spark_color = teleport_flash_core_color
		spark_color.a = _rng.randf_range(0.6, 1.0)
		var spark = create_flash_line(p1, p2, _rng.randf_range(1.3, 3.0), spark_color, 1.0, 1.0)
		parent.add_child(spark)

		var tw = spark.create_tween().set_parallel(true)
		tw.tween_property(spark, "modulate:a", 0.0, teleport_spark_duration)
		tw.tween_property(spark, "width", 0.0, teleport_spark_duration)

#endregion

#region Afterimage Effect

func spawn_afterimage() -> void:
	if not animated_sprite: return

	# Create a ghost sprite at the current position
	var ghost = Sprite2D.new()
	var texture = animated_sprite.sprite_frames.get_frame_texture(animated_sprite.animation, animated_sprite.frame)
	ghost.texture = texture
	ghost.flip_h = animated_sprite.flip_h
	ghost.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST

	# Position the ghost in world space
	ghost.global_position = animated_sprite.global_position
	ghost.modulate = afterimage_color

	# Add to the scene tree (as sibling of root, so it doesn't move with character)
	get_tree().current_scene.add_child(ghost)

	# Fade out and remove
	var tween = ghost.create_tween()
	tween.tween_property(ghost, "modulate:a", 0.0, afterimage_fade_duration)
	tween.finished.connect(func(): ghost.queue_free())

#endregion

#region Animation Handling

func play_animation(anim_name: String) -> void:
	if not animation_player: return

	# Set correct loop mode before playing:
	# Only idle, run, fall should loop. Everything else plays once.
	if animation_player.has_animation(anim_name):
		var anim = animation_player.get_animation(anim_name)
		var should_loop = anim_name in LOOPING_ANIMATIONS
		anim.loop_mode = Animation.LOOP_LINEAR if should_loop else Animation.LOOP_NONE

	animation_player.play(anim_name)

func _on_animation_finished(anim_name: StringName) -> void:
	var name = str(anim_name)

	match name:
		# Transition animations → advance to next state
		"idle_to_run":
			if _current_state == State.IdleToRun: change_state(State.Run)
		"run_to_idle":
			if _current_state == State.RunToIdle: change_state(State.Idle)
		"jump_to_fall":
			if _current_state == State.JumpToFall: change_state(State.Fall)
		"landing", "fall_to_idle":
			if _current_state == State.Landing or _current_state == State.FallToIdle:
				change_state(State.Idle)

		# Jump finishes → transition to fall if descending
		"jump":
			if _current_state == State.Jump and velocity.y >= 0:
				change_state(State.JumpToFall)
		"double_jump":
			if _current_state == State.DoubleJump and velocity.y >= 0:
				change_state(State.Fall)

		# Attack combo chain
		"attack1":
			if _current_state == State.Attack1:
				change_state(State.Attack2 if _combo_requested else State.Idle)
		"attack2":
			if _current_state == State.Attack2:
				change_state(State.Attack3 if _combo_requested else State.Idle)
		"attack3", "heavy_attack":
			change_state(State.Idle)

		"shuriken":
			if _current_state == State.Throw: change_state(State.Idle)
		"shuriken_air":
			if _current_state == State.AirThrow: change_state(State.Fall)
		"jump_attack":
			if _current_state == State.JumpAttack: change_state(State.Fall)
		"dash":
			if _current_state == State.Dash:
				velocity = Vector2.ZERO
				change_state(State.Idle if is_on_floor() else State.Fall)

#endregion

#region Physics Helpers

func get_move_input() -> float:
	return Input.get_axis("move_left", "move_right")

func apply_gravity(dt: float) -> void:
	if not is_on_floor():
		velocity.y = min(velocity.y + gravity * dt, max_fall_speed)

func apply_movement(dt: float, grounded: bool) -> void:
	var input_dir = get_move_input()
	var accel = acceleration if grounded else air_acceleration
	var fric = friction if grounded else air_friction

	if abs(input_dir) > 0.1:
		velocity.x = move_toward(velocity.x, input_dir * move_speed, accel * dt)
		if grounded: update_facing(input_dir)
	else:
		velocity.x = move_toward(velocity.x, 0, fric * dt)

func apply_friction(dt: float, grounded: bool) -> void:
	var fric = friction if grounded else air_friction
	velocity.x = move_toward(velocity.x, 0, fric * dt)

func update_facing(direction: float) -> void:
	if direction > 0.1:
		_facing_direction = 1
	elif direction < -0.1:
		_facing_direction = -1

	if animated_sprite:
		animated_sprite.flip_h = _facing_direction < 0

func detect_wall_slide() -> void:
	if enable_wall_jump and is_on_wall() and not is_on_floor():
		var wall_normal = get_wall_normal()
		var input_dir = get_move_input()

		# Only wall slide if player is pressing toward the wall
		if (wall_normal.x > 0 and input_dir < -0.1) or (wall_normal.x < 0 and input_dir > 0.1):
			_wall_direction = -1 if wall_normal.x > 0 else 1
			change_state(State.WallSlide)

func try_drop_through_platform() -> bool:
	# Check if standing on a one-way collision platform
	for i in get_slide_collision_count():
		var collision = get_slide_collision(i)
		var collider = collision.get_collider()
		var is_one_way = false

		# StaticBody2D: check children for OneWayCollision shapes
		if collider is StaticBody2D:
			for child in collider.get_children():
				if child is CollisionShape2D and child.one_way_collision:
					is_one_way = true
					break
			# TileMapLayer / TileMap: assume one-way if player intentionally presses down+jump
		elif collider is TileMap or collider is TileMapLayer:
			is_one_way = true

		if is_one_way:
			# Disable floor snap to prevent snapping back onto the platform
			var prev_snap = floor_snap_length
			floor_snap_length = 0
			position.y += 4
			velocity.y = 50

			# Restore floor snap after passing through
			get_tree().create_timer(0.15).timeout.connect(func():
				if is_instance_valid(self):
					floor_snap_length = prev_snap
			)
			return true
	return false

# Handles throw event triggered directly by the on-screen Virtual Direction Button.
func on_virtual_throw_activated(aim_angle: float) -> void:
	if try_flying_thunder_god_teleport():
		return

	# Don't throw if already throwing
	if _current_state == State.Throw or _current_state == State.AirThrow:
		return

	_pending_throw_angle = aim_angle
	change_state(State.Throw if is_on_floor() else State.AirThrow)

	#endregion
