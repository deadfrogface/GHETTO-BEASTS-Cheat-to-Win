shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.12;
uniform float scanline_density = 720.0;
uniform float vignette : hint_range(0.0, 2.0) = 0.55;
uniform float aberration : hint_range(0.0, 0.01) = 0.0012;
uniform float glitch : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	vec2 uv = SCREEN_UV;
	float shift = glitch * 0.02 * sin(uv.y * 90.0 + TIME * 40.0) * step(0.6, fract(sin(floor(uv.y * 24.0) + TIME * 13.0) * 437.5));
	uv.x += shift;

	float ab = aberration + glitch * 0.006;
	vec3 col;
	col.r = texture(screen_texture, uv + vec2(ab, 0.0)).r;
	col.g = texture(screen_texture, uv).g;
	col.b = texture(screen_texture, uv - vec2(ab, 0.0)).b;

	float scan = sin(uv.y * scanline_density * 3.14159) * 0.5 + 0.5;
	col *= 1.0 - scanline_strength * scan;

	vec2 d = uv - 0.5;
	col *= clamp(1.0 - dot(d, d) * vignette * 2.0, 0.0, 1.0);

	COLOR = vec4(col, 1.0);
}
