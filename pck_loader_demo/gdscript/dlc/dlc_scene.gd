extends Control

var animation_playing = false
var original_position = Vector2.ZERO
var tween = null

func _ready():
	$Panel/VBoxContainer/AnimationButton.pressed.connect(_on_animation_button_pressed)
	original_position = $Panel/VBoxContainer.position
	print("DLC Scene loaded and ready!")

func _on_animation_button_pressed():
	if animation_playing:
		return
		
	animation_playing = true
	
	# Create a new tween for animation
	tween = create_tween().set_trans(Tween.TRANS_ELASTIC).set_ease(Tween.EASE_OUT)
	tween.tween_property($Panel/VBoxContainer, "position", original_position + Vector2(0, -50), 0.5)
	tween.tween_property($Panel/VBoxContainer, "position", original_position, 0.5)
	
	# Change color animation
	var color_tween = create_tween().set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	var panel = $Panel
	var original_color = panel.get_theme_stylebox("panel").bg_color
	var new_color = Color(0.8, 0.2, 0.2, 1.0)
	
	color_tween.tween_method(func(value: Color): 
		var style = panel.get_theme_stylebox("panel").duplicate()
		style.bg_color = value
		panel.add_theme_stylebox_override("panel", style), 
		original_color, new_color, 0.5)
	
	color_tween.tween_method(func(value: Color): 
		var style = panel.get_theme_stylebox("panel").duplicate()
		style.bg_color = value
		panel.add_theme_stylebox_override("panel", style), 
		new_color, original_color, 0.5)
	
	# When the tween is completed, reset the flag
	tween.finished.connect(func(): animation_playing = false) 