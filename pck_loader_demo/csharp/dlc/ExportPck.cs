using Godot;
using System.Collections.Generic;

#if TOOLS
[Tool]
public partial class ExportPck : EditorScript
{
    public override void _Run()
    {
        GD.Print("Exporting DLC content to PCK file (C# version)...");
        
        // Define the path where to save the PCK file
        string outputPath = "res://pck_loader_demo/csharp/dlc/dlc_content_cs.pck";
        
        // Define files to include in the PCK
        List<string> files = new List<string>
        {
            "res://pck_loader_demo/csharp/dlc/DlcScene.tscn",
            "res://pck_loader_demo/csharp/dlc/DlcScene.cs"
        };
        
        // Create a new PCKPacker instance
        var packer = new PckPacker();
        
        // Begin the packing process
        Error result = packer.PckStart(outputPath);
        if (result != Error.Ok)
        {
            GD.PushError($"Failed to start PCK packing! Error code: {result}");
            return;
        }
        
        // Add each file to the PCK
        foreach (string filePath in files)
        {
            GD.Print($"Adding file to PCK: {filePath}");
            result = packer.AddFile(filePath, filePath);
            if (result != Error.Ok)
            {
                GD.PushError($"Failed to add file to PCK: {filePath} Error code: {result}");
                packer.Flush(true);  // Close the packer with abort flag
                return;
            }
        }
        
        // Finish the packing process
        packer.Flush(false);
        
        GD.Print($"PCK file created successfully: {outputPath}");
        GD.Print("To use this PCK file, run the C# main scene and click 'Load PCK'");
        GD.Print("Note: You may need to manually update the PCK if you make changes to the DLC scene");
    }
}
#endif 