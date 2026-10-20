shader_type canvas_item;

uniform float intensity : hint_range(0.0, 1.5) = 0.0;
uniform vec4 flash_color : source_color = vec4(1.0, 0.85, 0.95, 1.0);
uniform float invert_amount : hint_range(0.0, 1.0) = 0.85;

void fragment() {
	vec2 uv = UV * 2.0 - 1.0;
	float r = length(uv);
	float ring = smoothstep(0.15, 0.55, r) * (1.0 - smoothstep(0.7, 1.15, r));
	float pulse = 0.65 + 0.35 * sin(TIME * 28.0);
	float a = intensity * ring * pulse;
	vec3 inverted = mix(flash_color.rgb, vec3(1.0) - flash_color.rgb, invert_amount);
	COLOR = vec4(inverted, a * flash_color.a);
}
