# Dynamic PCK Loader Demo

This demo showcases how to dynamically load content from PCK (Godot's packed scene format) files at runtime.

## Features

- Load PCK files during runtime
- Instantiate scenes from loaded PCK files
- **Load and execute extra GDScript functionality from separate PCK files**
- Unload PCK content (simulated)
- Simple UI for demonstration
- Both GDScript and C# implementations

## GDScript Version (Enhanced)

### How to Use

1. **Generate the main PCK file**:
   - Open Godot Editor
   - Open `res://pck_loader_demo/gdscript/dlc/export_pck.gd` in the script editor
   - Click Script Editor's `File > Run` menu option (or by pressing Ctrl + Shift + X) to generate the PCK file

2. **Generate the extra script PCK file**:
   - Open `res://pck_loader_demo/gdscript/extra/export_extra_pck.gd` in the script editor
   - Click Script Editor's `File > Run` menu option to generate the extra script PCK file

3. **Run the Demo**:
   - Open the main scene at `res://pck_loader_demo/gdscript/main/main.tscn`
   - Run the scene
   - Click "Load Main PCK" to load the main PCK file
   - Click "Instantiate DLC Scene" to display the content from the main PCK
   - Click "Load Extra Script PCK" to load additional GDScript functionality
   - Click "Execute Extra Script" to run the dynamically loaded script
   - Click "Unload All PCKs" to simulate unloading all PCK content

### Extra Script Features

The extra script PCK demonstrates:
- Dynamic script loading and execution
- Mathematical calculations with history tracking
- Visual feedback creation
- Custom methods that can be called from the main application
- Real-time visual indicators showing the script is active

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

- The PCK files contain scenes and scripts
- When loaded, the PCK's content becomes available through normal resource loading
- **The GDScript version demonstrates dynamic script execution from loaded PCK files**
- The demo includes code to handle error cases and status updates
- In a real application, PCK files might be downloaded from a server or included as separate files

## Extending the Demo

To add more content to the PCK:

### For GDScript
1. Add your files to the `pck_loader_demo/gdscript/dlc/` directory (for main content)
2. Add extra scripts to the `pck_loader_demo/gdscript/extra/` directory (for additional functionality)
3. Update the respective export scripts to include your files in the PCK
4. Run the scripts again to regenerate the PCK files

### For C#
1. Add your files to the `pck_loader_demo/csharp/dlc/` directory
2. Update the `ExportPckEditorPlugin.cs` script to include your files in the PCK
3. Use the plugin button to regenerate the PCK

## Notes

- In Godot 4, there is no direct API to unload a PCK once loaded
- For production use, consider having separate scenes for each module/DLC
- PCK files can contain any resource that Godot can load (scenes, scripts, textures, etc.)
- The C# version uses a GDScript bridge to access the PCKPacker functionality since it's not directly exposed to C#
- **The GDScript version can load and execute scripts dynamically, enabling powerful plugin-like functionality**
