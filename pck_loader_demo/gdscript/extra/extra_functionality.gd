extends Node

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
