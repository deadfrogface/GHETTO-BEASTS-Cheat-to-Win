shader_type spatial;

uniform vec4 albedo : source_color = vec4(1.0);
uniform vec4 shade_color : source_color = vec4(0.32, 0.22, 0.5, 1.0);
uniform float shade_threshold : hint_range(-1.0, 1.0) = 0.05;
uniform float shade_softness : hint_range(0.0, 0.5) = 0.02;
uniform vec4 rim_color : source_color = vec4(0.3, 1.0, 0.95, 1.0);
uniform float rim_amount : hint_range(0.0, 1.0) = 0.72;
uniform float rim_strength : hint_range(0.0, 4.0) = 1.2;
uniform float specular_size : hint_range(0.0, 1.0) = 0.93;
uniform vec4 emission_color : source_color = vec4(1.0);
uniform float emission_strength : hint_range(0.0, 8.0) = 0.0;

void fragment() {
	ALBEDO = albedo.rgb;
	EMISSION = emission_color.rgb * emission_strength;
}

void light() {
	float ndotl = dot(NORMAL, LIGHT);
	float lit = smoothstep(shade_threshold - shade_softness, shade_threshold + shade_softness, ndotl) * ATTENUATION;
	vec3 light_col = LIGHT_COLOR / PI;

	DIFFUSE_LIGHT += light_col * mix(shade_color.rgb * 0.35, vec3(1.0), lit);

	vec3 half_vec = normalize(LIGHT + VIEW);
	float spec = step(specular_size, dot(NORMAL, half_vec)) * lit;
	SPECULAR_LIGHT += light_col * spec * 0.6;

	float rim = smoothstep(rim_amount - 0.02, rim_amount + 0.02, 1.0 - dot(NORMAL, VIEW));
	SPECULAR_LIGHT += rim_color.rgb * rim * rim_strength * max(lit, 0.25) * light_col;
}
