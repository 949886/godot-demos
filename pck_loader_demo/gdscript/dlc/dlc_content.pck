GDPC                 �                                                                         4   res://pck_loader_demo/gdscript/dlc/dlc_scene.tscn           �      �Ҷ �p�<�T��"�T    0   res://pck_loader_demo/gdscript/dlc/dlc_scene.gd �      �      �(#���xe+����    <   res://pck_loader_demo/gdscript/extra/extra_functionality.gd �      �      �s3Cx�w��x��        [gd_scene load_steps=4 format=3 uid="uid://bdxslxlgd0hlc"]

[ext_resource type="Script" path="res://pck_loader_demo/gdscript/dlc/dlc_scene.gd" id="1_xfbqn"]
[ext_resource type="Script" path="res://pck_loader_demo/gdscript/extra/extra_functionality.gd" id="2_fyscn"]

[sub_resource type="StyleBoxFlat" id="StyleBoxFlat_rgpus"]
bg_color = Color(0.184314, 0.47451, 0.690196, 1)
border_width_left = 4
border_width_top = 4
border_width_right = 4
border_width_bottom = 4
border_color = Color(0.933333, 0.913725, 0.427451, 1)
corner_radius_top_left = 8
corner_radius_top_right = 8
corner_radius_bottom_right = 8
corner_radius_bottom_left = 8

[node name="DLCScene" type="Control"]
layout_mode = 3
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
script = ExtResource("1_xfbqn")

[node name="Panel" type="Panel" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
theme_override_styles/panel = SubResource("StyleBoxFlat_rgpus")

[node name="VBoxContainer" type="VBoxContainer" parent="Panel"]
layout_mode = 1
anchors_preset = 8
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -239.0
offset_top = -64.0
offset_right = 239.0
offset_bottom = 64.0
grow_horizontal = 2
grow_vertical = 2

[node name="Label" type="Label" parent="Panel/VBoxContainer"]
layout_mode = 2
theme_override_colors/font_color = Color(1, 1, 1, 1)
theme_override_colors/font_outline_color = Color(0, 0, 0, 1)
theme_override_constants/outline_size = 2
theme_override_font_sizes/font_size = 28
text = "DLC Content Loaded Successfully!"
horizontal_alignment = 1

[node name="AnimationButton" type="Button" parent="Panel/VBoxContainer"]
layout_mode = 2
size_flags_vertical = 3
text = "Play Animation"

[node name="InfoLabel" type="Label" parent="Panel/VBoxContainer"]
layout_mode = 2
theme_override_font_sizes/font_size = 16
text = "This scene was loaded from a PCK file"
horizontal_alignment = 1

[node name="Node" type="Node" parent="Panel/VBoxContainer" node_paths=PackedStringArray("label")]
script = ExtResource("2_fyscn")
label = NodePath("../InfoLabel")
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
	tween.finished.connect(func(): animation_playing = false)             extends Node

# This script demonstrates extra functionality that can be loaded dynamically from a PCK
class_name ExtraFunctionality

@export var label: Label

var calculation_history = []
var magic_number = 42

func _ready():
	print("Extra functionality script loaded and ready!")
	print("Magic number initialized to: ", magic_number)
	
	if label != null:
		label.text += "\nExtra Functionality Active!"
		print("Updated with functionality.")
	else:
		print("Label not found, cannot update UI.")

func execute_extra_functionality():
	print("Executing extra functionality...")
	
	# Perform some calculations
	var result = perform_calculations()
	
	# Create some visual feedback
	create_visual_feedback()
	
	# Return a result to show the script worked
	return result

func perform_calculations():
	var numbers = [1, 2, 3, 4, 5]
	var total = 0
	
	for num in numbers:
		var squared = num * num
		total += squared
		calculation_history.append({"number": num, "squared": squared})
		print("Number: %d, Squared: %d" % [num, squared])
	
	var final_result = total + magic_number
	print("Total of squares: ", total)
	print("Final result (with magic number): ", final_result)
	
	return final_result

func create_visual_feedback():
	# Find the main scene to add visual elements
	var main_scene = get_tree().get_first_node_in_group("main")
	if main_scene == null:
		# Try to find the root control node
		main_scene = get_tree().current_scene
	
	if main_scene != null and main_scene.has_node("ExtraScriptContainer"):
		var container = main_scene.get_node("ExtraScriptContainer")
		
		# Create a label to show we're running
		var label = Label.new()
		label.text = "Extra Script Active!\nMagic Number: %d\nCalculations: %d" % [magic_number, calculation_history.size()]
		label.add_theme_color_override("font_color", Color.GREEN)
		label.add_theme_font_size_override("font_size", 16)
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		
		# Position the label
		label.anchors_preset = Control.PRESET_FULL_RECT
		
		container.add_child(label)
		
		# Create a tween to animate the label
		var tween = create_tween()
		tween.set_loops()
		tween.tween_property(label, "modulate", Color.YELLOW, 1.0)
		tween.tween_property(label, "modulate", Color.GREEN, 1.0)
		
		print("Visual feedback created!")
	else:
		print("Could not find container for visual feedback")

func get_calculation_history():
	return calculation_history.duplicate()

func update_magic_number(new_number: int):
	magic_number = new_number
	print("Magic number updated to: ", magic_number)

func perform_custom_calculation(a: float, b: float, operation: String):
	var result = 0.0
	
	match operation:
		"add":
			result = a + b
		"multiply":
			result = a * b
		"power":
			result = pow(a, b)
		"divide":
			if b != 0:
				result = a / b
			else:
				print("Cannot divide by zero!")
				return null
		_:
			print("Unknown operation: ", operation)
			return null
	
	print("Custom calculation: %f %s %f = %f" % [a, operation, b, result])
	return result 
          