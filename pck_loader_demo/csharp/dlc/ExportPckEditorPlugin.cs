using Godot;
using System;
using System.Collections.Generic;

#if TOOLS
[Tool]
public partial class ExportPckEditorPlugin : EditorPlugin
{
    private Button _exportButton;
    
    public override void _EnterTree()
    {
        // Create a button for the editor
        _exportButton = new Button();
        _exportButton.Text = "Export C# PCK";
        _exportButton.TooltipText = "Export DLC content to a PCK file";
        _exportButton.Pressed += ExportPck;
        
        // Add the button to the editor
        AddControlToContainer(CustomControlContainer.Toolbar, _exportButton);
    }
    
    public override void _ExitTree()
    {
        // Clean up the button when the plugin is disabled
        if (_exportButton != null)
        {
            RemoveControlFromContainer(CustomControlContainer.Toolbar, _exportButton);
            _exportButton.QueueFree();
        }
    }
    
    private void ExportPck()
    {
        GD.Print("Exporting DLC content to PCK file (C# version)...");
        
        // Define the path where to save the PCK file
        string outputPath = "res://pck_loader_demo/csharp/dlc/dlc_content_cs.pck";
        
        // For C# we need to use GDScript to access PCKPacker
        // Create a temporary GDScript file to handle the packing
        string gdScript = @"
        tool
        extends Node

        func pack_files(output_path, files):
            print('Packing files via GDScript bridge...')
            var packer = PCKPacker.new()
            var result = packer.pck_start(output_path)
            if result != OK:
                push_error('Failed to start PCK packing! Error code: ' + str(result))
                return false
                
            for file_path in files:
                print('Adding file to PCK: ' + file_path)
                result = packer.add_file(file_path, file_path)
                if result != OK:
                    push_error('Failed to add file to PCK: ' + file_path + ' Error code: ' + str(result))
                    packer.flush(true)
                    return false
                    
            packer.flush(false)
            print('PCK file created successfully: ' + output_path)
            return true
        ";
        
        // Save the temporary script
        string tempScriptPath = "res://pck_loader_demo/csharp/dlc/temp_packer.gd";
        var file = FileAccess.Open(tempScriptPath, FileAccess.ModeFlags.Write);
        file.StoreString(gdScript);
        file.Close();
        
        // Make sure the script is loaded
        ResourceLoader.Load(tempScriptPath);
        
        // Create a temporary node to run the GDScript
        var tempNode = new Node();
        AddChild(tempNode);
        tempNode.SetScript(GD.Load(tempScriptPath));
        
        // Define files to include in the PCK
        Godot.Collections.Array<string> files = new Godot.Collections.Array<string>();
        files.Add("res://pck_loader_demo/csharp/dlc/DlcScene.tscn");
        files.Add("res://pck_loader_demo/csharp/dlc/DlcScene.cs");
        
        // Call the GDScript method to pack the files
        bool success = (bool)tempNode.Call("pack_files", outputPath, files);
        
        // Clean up
        tempNode.QueueFree();
        
        // Delete the temporary script
        var dir = DirAccess.Open("res://pck_loader_demo/csharp/dlc");
        if (dir != null && dir.FileExists("temp_packer.gd"))
        {
            dir.Remove("temp_packer.gd");
        }
        
        // Report results
        if (success)
        {
            GD.Print("PCK creation completed successfully.");
            GD.Print("To use this PCK file, run the C# main scene and click 'Load PCK'");
        }
        else
        {
            GD.PrintErr("Failed to create PCK file. Check the output log for errors.");
        }
    }
}
#endif 