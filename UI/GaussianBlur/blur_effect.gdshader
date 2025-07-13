shader_type canvas_item;

uniform float blur_amount : hint_range(0.0, 10.0, 0.1) = 2.0;
uniform float brightness : hint_range(0.0, 2.0, 0.1) = 1.2;
uniform float saturation : hint_range(0.0, 2.0, 0.1) = 1.1;

varying vec2 vertex_pos;

void vertex() {
    vertex_pos = VERTEX;
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void fragment() {
    vec2 tex_size = 1.0 / TEXTURE_PIXEL_SIZE;
    vec2 blur_size = blur_amount / tex_size;
    
    vec4 sum = vec4(0.0);
    int samples = 9;
    
    // Gaussian blur
    for (int x = -4; x <= 4; x++) {
        for (int y = -4; y <= 4; y++) {
            vec2 offset = vec2(float(x), float(y)) * blur_size;
            float weight = exp(-float(x*x + y*y) / 8.0);
            sum += texture(TEXTURE, UV + offset) * weight;
        }
    }
    
    sum /= 81.0; // Normalize weights
    
    // Apply brightness and saturation adjustments
    vec3 hsv = rgb2hsv(sum.rgb);
    hsv.y *= saturation; // Saturation
    hsv.z *= brightness; // Brightness
    
    vec3 final_color = hsv2rgb(hsv);
    
    COLOR = vec4(final_color, sum.a);
} 