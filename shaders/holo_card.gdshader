shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_always;

uniform vec4 frame_color : source_color = vec4(1.0, 0.2, 0.8, 1.0);
uniform vec4 inner_color : source_color = vec4(0.08, 0.04, 0.14, 1.0);
uniform float foil_strength : hint_range(0.0, 1.0) = 0.5;
uniform float glow : hint_range(0.0, 4.0) = 1.6;
uniform float dissolve : hint_range(0.0, 1.0) = 0.0;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void fragment() {
	vec2 uv = UV;
	float bx = 0.055;
	float by = 0.04;
	float edge = max(max(step(uv.x, bx), step(1.0 - bx, uv.x)), max(step(uv.y, by), step(1.0 - by, uv.y)));
	float inner_edge = max(max(step(uv.x, bx + 0.012), step(1.0 - bx - 0.012, uv.x)),
		max(step(uv.y, by + 0.01), step(1.0 - by - 0.01, uv.y))) - edge;

	float band = sin((uv.x * 1.3 + uv.y) * 14.0 - TIME * 3.0) * 0.5 + 0.5;
	vec3 foil = vec3(0.5 + 0.5 * sin(TIME + uv.y * 6.0), 0.5 + 0.5 * sin(TIME * 1.3 + uv.x * 6.0 + 2.0), 1.0);
	vec3 inner = inner_color.rgb + foil * band * foil_strength * 0.22;

	float art_window = step(0.12, uv.x) * step(uv.x, 0.88) * step(0.16, uv.y) * step(uv.y, 0.62);
	inner = mix(inner, inner * 0.6 + frame_color.rgb * 0.25 * (0.6 + 0.4 * band), art_window);

	vec3 col = mix(inner, frame_color.rgb * glow, edge);
	col = mix(col, vec3(1.0), inner_edge * 0.6);

	float noise = hash(floor(uv * 40.0));
	if (noise < dissolve) {
		discard;
	}
	float burn = smoothstep(dissolve, dissolve + 0.08, noise);
	col = mix(frame_color.rgb * 3.0, col, dissolve > 0.0 ? burn : 1.0);

	ALBEDO = col;
}
