shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float blur_strength : hint_range(0.0, 10.0) = 2.0;
uniform float transparency : hint_range(0.0, 1.0) = 0.8;
uniform float frost_amount : hint_range(0.0, 1.0) = 0.3;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 0.1);
uniform float distortion : hint_range(0.0, 0.1) = 0.02;

// Noise function for frost pattern
float noise(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);
}

void fragment() {
    vec2 blur_offset = vec2(blur_strength) / SCREEN_PIXEL_SIZE;
    vec4 blur_color = vec4(0.0);

    // Multi-sample blur
    int samples = 9;
    float weight = 1.0 / float(samples);

    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            vec2 offset = vec2(float(x), float(y)) * blur_offset;
            blur_color += texture(SCREEN_TEXTURE, SCREEN_UV + offset) * weight;
        }
    }

    // Add distortion for more realistic glass effect
    vec2 distorted_uv = SCREEN_UV + vec2(
        sin(SCREEN_UV.y * 50.0) * distortion,
        cos(SCREEN_UV.x * 50.0) * distortion
    );

    vec4 distorted_color = texture(SCREEN_TEXTURE, distorted_uv);
    blur_color = mix(blur_color, distorted_color, 0.3);

    // Add frost noise pattern
    float frost_noise = noise(UV * 200.0);
    frost_noise = smoothstep(0.4, 0.6, frost_noise);

    // Combine effects
    vec4 final_color = mix(blur_color, tint_color, frost_amount * frost_noise);

    COLOR = vec4(final_color.rgb, transparency);
}