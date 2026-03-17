shader_type canvas_item;

// --- 参数控制 ---
uniform sampler2D noise_img : repeat_enable; // 强制开启 repeat 模式
uniform float speed : hint_range(0.0, 0.5) = 0.05;
uniform float strength : hint_range(0.0, 0.1) = 0.02;
uniform float scale = 1.0;
uniform float pixel_size = 1.0;

void fragment() {
    // 获取纹理分辨率
    vec2 tex_size = vec2(textureSize(TEXTURE, 0));
    
    // 1. 像素化 UV 坐标
    // 使用 floor 代替 round 可以减少某些设备上的抖动，并使用更加稳定的像素对齐
    vec2 grid_uv = floor(UV * (tex_size / pixel_size)) / (tex_size / pixel_size);
    
    // 2. 循环时间处理
    // 使用 mod 将时间限制在 0-100 之间，防止运行时间过长导致的精度丢失
    // 100 只是一个随机的较大常数，由于噪声是循环的，这不会引起跳变
    float t = mod(TIME, 100.0);
    
    // 3. 计算翻涌位移
    // 使用 fract 确保采样坐标始终落在 [0, 1] 范围内
    vec2 move1 = vec2(t * speed, t * speed * 0.5);
    vec2 move2 = vec2(t * -speed * 0.3, t * speed * 0.8);
    
    vec2 noise_uv1 = fract(grid_uv * scale + move1);
    vec2 noise_uv2 = fract(grid_uv * scale * 1.5 + move2);
    
    float n1 = texture(noise_img, noise_uv1).r;
    float n2 = texture(noise_img, noise_uv2).g;
    
    // 合成位移量 (减去0.5是为了让位移有正有负，产生往复感)
    vec2 offset = vec2(n1 - 0.5, n2 - 0.5) * strength;
    
    // 4. 区域限制 (仅天空部分)
    // 如果效果慢慢消失，请检查这里的 mask 是否被背景的其他移动物体影响
    float mask = smoothstep(0.5, 0.4, UV.y); 
    
    // 5. 最终采样
    // 这里的 UV 不需要 fract，因为它是背景图的坐标
    COLOR = texture(TEXTURE, UV + offset * mask);
}