# Frosted Glass UI Demo

A powerful demonstration of translucent frosted glass effects for UI elements in Godot 4 using C#.

## Features

### 🎨 Visual Effects
- **Real-time blur**: Dynamic background blurring behind UI elements
- **Frost pattern**: Procedural noise-based frost texture overlay
- **Glass distortion**: Subtle wave distortion for realistic glass appearance
- **Transparency control**: Adjustable opacity for UI elements
- **Animated parameters**: Smooth parameter animations and transitions

### 🎮 Interactive Controls
- **Blur strength slider**: Real-time adjustment of background blur intensity (0-10)
- **Transparency slider**: Control UI element opacity (0-1)
- **Animation toggle**: Enable/disable background shape animations
- **Frost effect toggle**: Turn frost pattern on/off
- **Preset buttons**: Quick access to different effect configurations

### 🌈 Background Animation
- **Moving shapes**: Colorful animated shapes behind the UI
- **Color cycling**: Dynamic color changes over time
- **Smooth movement**: Sine/cosine-based smooth motion patterns

## How to Use

### Running the Demo
1. Open the scene: `UI/FrostedGlassDemo.tscn`
2. Run the scene to see the frosted glass effects in action
3. Use the interactive controls to experiment with different settings

### Shader Parameters
The frosted glass shader (`frosted_glass.gdshader`) includes these configurable parameters:

- `blur_strength`: Controls background blur intensity
- `transparency`: Sets UI element opacity
- `frost_amount`: Controls frost pattern visibility
- `tint_color`: Color tint applied to the glass effect
- `distortion`: Amount of glass distortion effect

### Applying to Your UI

#### Method 1: Using the Material Resource
```csharp
// Load the pre-configured material
var frostedMaterial = GD.Load<ShaderMaterial>("res://UI/frosted_glass_material.tres");
yourUIElement.Material = frostedMaterial;
```

#### Method 2: Creating Custom Material
```csharp
// Create a new shader material
var shader = GD.Load<Shader>("res://UI/frosted_glass.gdshader");
var material = new ShaderMaterial();
material.Shader = shader;

// Set custom parameters
material.SetShaderParameter("blur_strength", 5.0f);
material.SetShaderParameter("transparency", 0.8f);
material.SetShaderParameter("frost_amount", 0.3f);

yourUIElement.Material = material;
```

## Technical Details

### Shader Implementation
The frosted glass effect is achieved through:

1. **Multi-sample blur**: 3x3 grid sampling for smooth background blur
2. **Procedural noise**: Mathematical noise function for frost patterns
3. **UV distortion**: Sine/cosine waves for glass-like distortion
4. **Color mixing**: Blending original, blurred, and tinted colors

### Performance Considerations
- The shader uses 9 texture samples for blur (optimized for quality vs. performance)
- Noise calculations are done per-fragment (can be optimized if needed)
- Animation updates run at 60fps but can be throttled if required

### Compatibility
- Requires Godot 4.0+
- Works with CanvasItem-based UI elements (Panel, Control, etc.)
- Compatible with both 2D and 3D UI overlays

## Customization Tips

### Adjusting Blur Quality
To change blur quality, modify the sampling pattern in the shader:
```glsl
// Increase samples for higher quality (performance cost)
for (int x = -2; x <= 2; x++) {
    for (int y = -2; y <= 2; y++) {
        // 5x5 sampling grid
    }
}
```

### Creating Different Glass Types
Modify shader parameters for different glass effects:
- **Heavy frost**: High `frost_amount` (0.5-0.8)
- **Clear glass**: Low `frost_amount` (0.0-0.2)
- **Tinted glass**: Adjust `tint_color` with stronger alpha
- **Distorted glass**: Increase `distortion` value

### Animation Ideas
- Pulse effects: Animate blur_strength in a loop
- Fade transitions: Animate transparency on show/hide
- Dynamic frost: Animate frost_amount based on events
- Color shifts: Animate tint_color for atmospheric effects

## Files Structure
```
UI/
├── frosted_glass.gdshader          # Main shader file
├── frosted_glass_material.tres     # Pre-configured material
├── FrostedGlassDemo.tscn          # Demo scene
├── FrostedGlassDemo.cs            # Demo controller script
└── README_FrostedGlass.md         # This documentation
```

Enjoy creating beautiful frosted glass UI effects! 🌟 