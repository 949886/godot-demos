shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float blur_radius : hint_range(0.0, 50.0) = 5.0;
uniform int blur_samples : hint_range(3, 25) = 9;
uniform float transparency : hint_range(0.0, 1.0) = 0.9;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 0.05);
uniform bool horizontal_pass = false;

// Gaussian weight calculation
float gaussian(float x, float sigma) {
    return exp(-(x * x) / (2.0 * sigma * sigma)) / (sigma * sqrt(2.0 * 3.14159265));
}

void fragment() {
    vec2 pixel_size = 1.0 / SCREEN_PIXEL_SIZE;
    vec4 blur_color = vec4(0.0);
    float total_weight = 0.0;

    // Calculate sigma based on blur radius for proper Gaussian distribution
    float sigma = blur_radius / 3.0;

    if (horizontal_pass) {
        // Horizontal pass
        for (int i = -blur_samples; i <= blur_samples; i++) {
            float offset = float(i) * blur_radius / float(blur_samples);
            vec2 sample_pos = SCREEN_UV + vec2(offset / pixel_size.x, 0.0);

            float weight = gaussian(offset, sigma);
            blur_color += texture(SCREEN_TEXTURE, sample_pos) * weight;
            total_weight += weight;
        }
    } else {
        // Vertical pass (or single pass if not using two-pass)
        for (int i = -blur_samples; i <= blur_samples; i++) {
            float offset = float(i) * blur_radius / float(blur_samples);
            vec2 sample_pos = SCREEN_UV + vec2(0.0, offset / pixel_size.y);

            float weight = gaussian(offset, sigma);
            blur_color += texture(SCREEN_TEXTURE, sample_pos) * weight;
            total_weight += weight;
        }
    }

    // Normalize by total weight
    blur_color /= total_weight;

    // Apply tint
    vec4 final_color = mix(blur_color, tint_color, tint_color.a);

    COLOR = vec4(final_color.rgb, transparency);
}