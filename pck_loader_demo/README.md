# Dynamic PCK Loader Demo

This demo showcases how to dynamically load content from PCK (Godot's packed scene format) files at runtime.

## Features

- Load PCK files during runtime
- Instantiate scenes from loaded PCK files
- Unload PCK content (simulated)
- Simple UI for demonstration
- Both GDScript and C# implementations

## GDScript Version

### How to Use

1. **Generate the PCK file**:
   - Open Godot Editor
   - Open `res://pck_loader_demo/gdscript/dlc/export_pck.gd` in the script editor
   - Click Script Editor's `File > Run` menu option (or by pressing Ctrl + Shift + X) to generate the PCK file

2. **Run the Demo**:
   - Open the main scene at `res://pck_loader_demo/main/main.tscn`
   - Run the scene
   - Click "Load PCK" to load the PCK file
   - Click "Instantiate DLC Scene" to display the content from the PCK
   - Click "Unload PCK" to simulate unloading the PCK

## C# Version

### How to Use

1. **Generate the PCK file**:
   - Open Godot Editor
   - Go to `Project > Project Settings > Plugins` tab
   - Enable the "C# PCK Exporter" plugin
   - Click the "Export C# PCK" button in the editor toolbar

2. **Run the Demo**:
   - Open the main scene at `res://pck_loader_demo/csharp/main/main.tscn`
   - Run the scene
   - Click "Load PCK" to load the PCK file
   - Click "Instantiate DLC Scene" to display the content from the PCK
   - Click "Unload PCK" to simulate unloading the PCK

## Technical Details

- The PCK file contains a scene and its script
- When loaded, the PCK's content becomes available through normal resource loading
- The demo includes code to handle error cases and status updates
- In a real application, PCK files might be downloaded from a server or included as separate files

## Extending the Demo

To add more content to the PCK:

### For GDScript
1. Add your files to the `pck_loader_demo/dlc/` directory
2. Update the `export_pck.gd` script to include your files in the PCK
3. Run the script again to regenerate the PCK

### For C#
1. Add your files to the `pck_loader_demo/csharp/dlc/` directory
2. Update the `ExportPckEditorPlugin.cs` script to include your files in the PCK
3. Use the plugin button to regenerate the PCK

## Notes

- In Godot 4, there is no direct API to unload a PCK once loaded
- For production use, consider having separate scenes for each module/DLC
- PCK files can contain any resource that Godot can load (scenes, scripts, textures, etc.)
- The C# version uses a GDScript bridge to access the PCKPacker functionality since it's not directly exposed to C# 
