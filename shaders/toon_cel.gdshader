shader_type spatial;

// Soft stylized toon (Palworld / Scarlet-like): smooth light→shadow ramp, muted shade.
uniform vec4 albedo : source_color = vec4(0.55, 0.52, 0.58, 1.0);
uniform vec4 shade_color : source_color = vec4(0.28, 0.26, 0.34, 1.0);
uniform float shade_threshold : hint_range(-1.0, 1.0) = 0.08;
uniform float shade_softness : hint_range(0.0, 0.8) = 0.28;
uniform vec4 rim_color : source_color = vec4(0.55, 0.72, 0.82, 1.0);
uniform float rim_amount : hint_range(0.0, 1.0) = 0.78;
uniform float rim_strength : hint_range(0.0, 4.0) = 0.45;
uniform float specular_size : hint_range(0.0, 1.0) = 0.96;
uniform float specular_strength : hint_range(0.0, 2.0) = 0.25;
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

	// Soft two-tone with gentle mid-ramp (not hard cel bands).
	vec3 lit_tone = mix(shade_color.rgb * 0.55, vec3(1.0), lit);
	DIFFUSE_LIGHT += light_col * lit_tone;

	vec3 half_vec = normalize(LIGHT + VIEW);
	float spec = smoothstep(specular_size - 0.04, specular_size + 0.02, dot(NORMAL, half_vec)) * lit;
	SPECULAR_LIGHT += light_col * spec * specular_strength;

	float rim = smoothstep(rim_amount - 0.08, rim_amount + 0.08, 1.0 - dot(NORMAL, VIEW));
	SPECULAR_LIGHT += rim_color.rgb * rim * rim_strength * max(lit, 0.2) * light_col;
}
