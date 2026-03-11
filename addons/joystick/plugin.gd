@tool
extends EditorPlugin

func _enter_tree() -> void:
	# The C# classes use [GlobalClass] attribute, so they auto-register.
	# This plugin.gd is just a placeholder for the plugin system.
	print("Virtual Joystick plugin enabled")

func _exit_tree() -> void:
	print("Virtual Joystick plugin disabled")
