shader_type canvas_item;

uniform float intensity : hint_range(0.0, 1.5) = 0.0;
uniform vec4 line_color : source_color = vec4(1.0, 1.0, 1.0, 0.55);
uniform float density : hint_range(4.0, 40.0) = 18.0;
uniform float thickness : hint_range(0.001, 0.08) = 0.012;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void fragment() {
	vec2 uv = UV * 2.0 - 1.0;
	float r = length(uv);
	float a = atan(uv.y, uv.x);
	float spokes = density;
	float cell = floor((a / 6.2831853 + 0.5) * spokes);
	float n = hash(vec2(cell, floor(TIME * 12.0)));
	float band = abs(fract((a / 6.2831853 + 0.5) * spokes) - 0.5);
	float line = smoothstep(thickness, 0.0, band) * step(0.45, n);
	float radial = smoothstep(0.15, 0.95, r);
	float alpha = line * radial * intensity * line_color.a;
	COLOR = vec4(line_color.rgb, alpha);
}
