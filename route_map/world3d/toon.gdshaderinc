// Cel (toon) lighting shared by every material in the 3D world, so land, trees, the ship and
// the sea read as one painted picture. Direct light falls into three soft-edged bands (shade,
// half-light, full light); shadows cut in cleanly; the lit side leans warm. Ambient sky light
// (the cool fill in the shade) comes from the environment as usual.
// Include AFTER the shader's own uniforms; it defines light().

uniform float toon_mid = 0.42;          // where the half-light band starts (N·L)
uniform float toon_full = 0.72;         // where full light starts
uniform float toon_soft = 0.05;         // edge softness of the bands
uniform float toon_half_level = 0.55;   // brightness of the half-light band
uniform vec3 toon_warm : source_color = vec3(1.0, 0.95, 0.86);
uniform float toon_spec = 0.0;          // stepped highlight strength (0 = none)
uniform float toon_spec_size = 0.965;

void light() {
	float ndl = dot(NORMAL, LIGHT);
	float band = smoothstep(0.0, toon_soft, ndl) * toon_half_level * 0.55
		+ smoothstep(toon_mid, toon_mid + toon_soft, ndl) * (toon_half_level - toon_half_level * 0.55)
		+ smoothstep(toon_full, toon_full + toon_soft, ndl) * (1.0 - toon_half_level);
	float shade = smoothstep(0.35, 0.65, ATTENUATION);
	DIFFUSE_LIGHT += LIGHT_COLOR * toon_warm * band * shade / PI;
	if (toon_spec > 0.0) {
		float r = dot(reflect(-LIGHT, NORMAL), VIEW);
		SPECULAR_LIGHT += LIGHT_COLOR * smoothstep(toon_spec_size, toon_spec_size + 0.01, r) * toon_spec * shade;
	}
}
