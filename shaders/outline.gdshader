shader_type spatial;
render_mode unshaded, cull_front;

// Fine dark inverted-hull outline (anime game default).
uniform vec4 outline_color : source_color = vec4(0.06, 0.05, 0.08, 1.0);
uniform float outline_width : hint_range(0.0, 0.2) = 0.012;

void vertex() {
	VERTEX += NORMAL * outline_width;
}

void fragment() {
	ALBEDO = outline_color.rgb;
}
