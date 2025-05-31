extends Control

var loaded_pck_path = ""
var extra_script_pck_path = ""
var dlc_scene_path = "res://pck_loader_demo/gdscript/dlc/dlc_scene.tscn"
var extra_script_path = "res://pck_loader_demo/gdscript/extra/extra_functionality.gd"
var dlc_instance = null
var extra_script_instance = null
var pck_file_path = "user://dlc_content.pck"
var extra_pck_file_path = "user://extra_script_content.pck"

func _ready():
	$Panel/VBoxContainer/LoadPCKButton.pressed.connect(_on_load_pck_button_pressed)
	$Panel/VBoxContainer/InstantiateDLCButton.pressed.connect(_on_instantiate_dlc_button_pressed)
	$Panel/VBoxContainer/LoadExtraScriptButton.pressed.connect(_on_load_extra_script_button_pressed)
	$Panel/VBoxContainer/ExecuteExtraScriptButton.pressed.connect(_on_execute_extra_script_button_pressed)
	$Panel/VBoxContainer/UnloadPCKButton.pressed.connect(_on_unload_pck_button_pressed)
	
	# Check if we need to create the DLC PCK file
	if not FileAccess.file_exists(pck_file_path):
		var local_pck = "res://pck_loader_demo/gdscript/dlc/dlc_content.pck"
		if FileAccess.file_exists(local_pck):
			var source_file = FileAccess.open(local_pck, FileAccess.READ)
			var dest_file = FileAccess.open(pck_file_path, FileAccess.WRITE)
			
			var content = source_file.get_buffer(source_file.get_length())
			dest_file.store_buffer(content)
			
			source_file.close()
			dest_file.close()
			
			print("Copied main PCK file to user directory")
	
	# Check if we need to create the extra script PCK file
	if not FileAccess.file_exists(extra_pck_file_path):
		var local_extra_pck = "res://pck_loader_demo/gdscript/extra/extra_content.pck"
		if FileAccess.file_exists(local_extra_pck):
			var source_file = FileAccess.open(local_extra_pck, FileAccess.READ)
			var dest_file = FileAccess.open(extra_pck_file_path, FileAccess.WRITE)
			
			var content = source_file.get_buffer(source_file.get_length())
			dest_file.store_buffer(content)
			
			source_file.close()
			dest_file.close()
			
			print("Copied extra script PCK file to user directory")

func _on_load_pck_button_pressed():
	if loaded_pck_path != "":
		_update_status("Main PCK is already loaded!")
		return
	
	if not FileAccess.file_exists(pck_file_path):
		_update_status("ERROR: Main PCK file not found!")
		return
	
	# Attempt to load the PCK file
	var success = ProjectSettings.load_resource_pack(pck_file_path)
	
	if success:
		loaded_pck_path = pck_file_path
		_update_status("Main PCK loaded successfully!")
		$Panel/VBoxContainer/LoadPCKButton.disabled = true
		$Panel/VBoxContainer/InstantiateDLCButton.disabled = false
		_update_unload_button_state()
	else:
		_update_status("ERROR: Failed to load main PCK!")

func _on_load_extra_script_button_pressed():
	if extra_script_pck_path != "":
		_update_status("Extra script PCK is already loaded!")
		return
	
	if not FileAccess.file_exists(extra_pck_file_path):
		_update_status("ERROR: Extra script PCK file not found!")
		return
	
	# Attempt to load the extra script PCK file
	var success = ProjectSettings.load_resource_pack(extra_pck_file_path)
	
	if success:
		extra_script_pck_path = extra_pck_file_path
		_update_status("Extra script PCK loaded successfully!")
		$Panel/VBoxContainer/LoadExtraScriptButton.disabled = true
		$Panel/VBoxContainer/ExecuteExtraScriptButton.disabled = false
		_update_unload_button_state()
	else:
		_update_status("ERROR: Failed to load extra script PCK!")

func _on_instantiate_dlc_button_pressed():
	if loaded_pck_path == "":
		_update_status("ERROR: No main PCK loaded!")
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

func _on_execute_extra_script_button_pressed():
	if extra_script_pck_path == "":
		_update_status("ERROR: No extra script PCK loaded!")
		return
	
	if extra_script_instance != null:
		_update_status("Extra script is already running!")
		return
	
	# Check if the script exists in the loaded PCK
	if ResourceLoader.exists(extra_script_path):
		var script_resource = load(extra_script_path)
		if script_resource != null:
			# Create a node to attach the script to
			extra_script_instance = Node.new()
			extra_script_instance.name = "ExtraScriptNode"
			extra_script_instance.set_script(script_resource)
			$ExtraScriptContainer.add_child(extra_script_instance)
			
			# Call a method from the loaded script if it exists
			if extra_script_instance.has_method("execute_extra_functionality"):
				var result = extra_script_instance.execute_extra_functionality()
				_update_status("Extra script executed! Result: " + str(result))
			else:
				_update_status("Extra script loaded but no execute_extra_functionality method found!")
		else:
			_update_status("ERROR: Could not load extra script!")
	else:
		_update_status("ERROR: Extra script not found in PCK!")

func _on_unload_pck_button_pressed():
	if loaded_pck_path == "" and extra_script_pck_path == "":
		_update_status("No PCKs are loaded!")
		return
	
	# Remove any instantiated content
	if dlc_instance != null:
		dlc_instance.queue_free()
		dlc_instance = null
	
	if extra_script_instance != null:
		extra_script_instance.queue_free()
		extra_script_instance = null
	
	# Reset all states
	loaded_pck_path = ""
	extra_script_pck_path = ""
	$Panel/VBoxContainer/LoadPCKButton.disabled = false
	$Panel/VBoxContainer/InstantiateDLCButton.disabled = true
	$Panel/VBoxContainer/LoadExtraScriptButton.disabled = false
	$Panel/VBoxContainer/ExecuteExtraScriptButton.disabled = true
	$Panel/VBoxContainer/UnloadPCKButton.disabled = true
	_update_status("All PCKs have been 'unloaded' (simulated)")

func _update_unload_button_state():
	$Panel/VBoxContainer/UnloadPCKButton.disabled = (loaded_pck_path == "" and extra_script_pck_path == "")

func _update_status(message: String):
	$Panel/VBoxContainer/StatusLabel.text = "Status: " + message
	print(message) 