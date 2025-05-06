@tool
extends EditorScript

func _run():
	print("Exporting DLC content to PCK file...")
	
	# Define the path where to save the PCK file
	var output_path = "res://pck_loader_demo/dlc/dlc_content.pck"
	
	# Define files to include in the PCK
	var files = [
		"res://pck_loader_demo/dlc/dlc_scene.tscn",
		"res://pck_loader_demo/dlc/dlc_scene.gd"
	]
	
	# Create a new PCKPacker instance
	var packer = PCKPacker.new()
	
	# Begin the packing process
	var result = packer.pck_start(output_path)
	if result != OK:
		push_error("Failed to start PCK packing! Error code: " + str(result))
		return
	
	# Add each file to the PCK
	for file_path in files:
		print("Adding file to PCK: " + file_path)
		result = packer.add_file(file_path, file_path)
		if result != OK:
			push_error("Failed to add file to PCK: " + file_path + " Error code: " + str(result))
			packer.flush(true)  # Close the packer with abort flag
			return
	
	# Finish the packing process
	packer.flush(false)
	
	print("PCK file created successfully: " + output_path)
	print("To use this PCK file, run the main scene and click 'Load PCK'")
	print("Note: You may need to manually update the PCK if you make changes to the DLC scene") 
