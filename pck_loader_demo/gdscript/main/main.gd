extends Control

var loaded_pck_path = ""
var dlc_scene_path = "res://pck_loader_demo/gdscript/dlc/dlc_scene.tscn"
var dlc_instance = null
var pck_file_path = "user://dlc_content.pck"

func _ready():
	$Panel/VBoxContainer/LoadPCKButton.pressed.connect(_on_load_pck_button_pressed)
	$Panel/VBoxContainer/InstantiateDLCButton.pressed.connect(_on_instantiate_dlc_button_pressed)
	$Panel/VBoxContainer/UnloadPCKButton.pressed.connect(_on_unload_pck_button_pressed)
	
	# Check if we need to create the DLC PCK file
	if not FileAccess.file_exists(pck_file_path):
		# In a real application, the PCK would be downloaded or provided with the game
		# For this demo, we'll use a local one in the project folder
		var local_pck = "res://pck_loader_demo/gdscript/dlc/dlc_content.pck"
		if FileAccess.file_exists(local_pck):
			var source_file = FileAccess.open(local_pck, FileAccess.READ)
			var dest_file = FileAccess.open(pck_file_path, FileAccess.WRITE)
			
			var content = source_file.get_buffer(source_file.get_length())
			dest_file.store_buffer(content)
			
			source_file.close()
			dest_file.close()
			
			print("Copied PCK file to user directory")

func _on_load_pck_button_pressed():
	if loaded_pck_path != "":
		_update_status("PCK is already loaded!")
		return
	
	if not FileAccess.file_exists(pck_file_path):
		_update_status("ERROR: PCK file not found!")
		return
	
	# Attempt to load the PCK file
	var success = ProjectSettings.load_resource_pack(pck_file_path)
	
	if success:
		loaded_pck_path = pck_file_path
		_update_status("PCK loaded successfully!")
		$Panel/VBoxContainer/LoadPCKButton.disabled = true
		$Panel/VBoxContainer/InstantiateDLCButton.disabled = false
		$Panel/VBoxContainer/UnloadPCKButton.disabled = false
	else:
		_update_status("ERROR: Failed to load PCK!")

func _on_instantiate_dlc_button_pressed():
	if loaded_pck_path == "":
		_update_status("ERROR: No PCK loaded!")
		return
	
	if dlc_instance != null:
		_update_status("DLC is already instantiated!")
		return
	
	# Check if the scene exists in the loaded PCK
	if ResourceLoader.exists(dlc_scene_path):
		var scene_resource = load(dlc_scene_path)
		if scene_resource != null:
			dlc_instance = scene_resource.instantiate()
			$DLCContainer.add_child(dlc_instance)
			_update_status("DLC scene instantiated!")
		else:
			_update_status("ERROR: Could not load DLC scene!")
	else:
		_update_status("ERROR: DLC scene not found in PCK!")

func _on_unload_pck_button_pressed():
	if loaded_pck_path == "":
		_update_status("No PCK is loaded!")
		return
	
	# Remove any instantiated content
	if dlc_instance != null:
		dlc_instance.queue_free()
		dlc_instance = null
	
	# In Godot 4, there's no direct API to unload a PCK
	# The best practice is to restart the scene/game
	# For this demo, we'll simulate unloading by disabling the buttons
	loaded_pck_path = ""
	$Panel/VBoxContainer/LoadPCKButton.disabled = false
	$Panel/VBoxContainer/InstantiateDLCButton.disabled = true
	$Panel/VBoxContainer/UnloadPCKButton.disabled = true
	_update_status("PCK has been 'unloaded' (simulated)")
	
	# Note: In a real application, you might want to restart the game
	# or reload the current scene to ensure resources are properly unloaded

func _update_status(message: String):
	$Panel/VBoxContainer/StatusLabel.text = "Status: " + message
	print(message) 