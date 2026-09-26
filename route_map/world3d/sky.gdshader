shader_type sky;
// A North Sea sky: low grey cloud over most of it, darker in the bellies, a thin bright break
// low toward the sun with a warm glow and silver edges. Dreary, but with light coming.
// `drift` moves the clouds with the wind (set by sea_view.gd).

uniform vec3 zenith : source_color = vec3(0.35, 0.4, 0.45);
uniform vec3 horizon : source_color = vec3(0.67, 0.69, 0.68);
uniform vec3 cloud_lit : source_color = vec3(0.74, 0.74, 0.72);
uniform vec3 cloud_dark : source_color = vec3(0.34, 0.37, 0.41);
uniform vec3 sun_glow : source_color = vec3(1.0, 0.8, 0.52);
uniform float coverage : hint_range(0.0, 1.0) = 0.62;
uniform float glow_strength = 0.55;
uniform sampler2D cloud_noise : filter_linear_mipmap, repeat_enable;
uniform vec2 drift;

void sky() {
	vec3 d = EYEDIR;
	vec3 sun = LIGHT0_ENABLED ? LIGHT0_DIRECTION : normalize(vec3(-0.6, 0.35, 0.3));
	float sd = max(dot(d, sun), 0.0);
	float up = clamp(d.y, 0.0, 1.0);
	vec3 col = mix(horizon, zenith, pow(up, 0.55));
	// a warm band low in the sky toward the sun: the hopeful part
	float low_band = exp(-abs(d.y - 0.06) * 9.0);
	col += sun_glow * (pow(sd, 6.0) * 0.45 + pow(sd, 40.0) * 0.5) * glow_strength * (0.4 + low_band);
	if (d.y < 0.0) {
		col = mix(col, horizon * 0.8, clamp(-d.y * 4.0, 0.0, 1.0));
	} else {
		// clouds on a flattened dome, three octaves, thinning toward the sun
		vec2 uv = d.xz / (d.y + 0.14) * 0.22 + drift;
		float n = texture(cloud_noise, uv).r * 0.58 + texture(cloud_noise, uv * 2.3 + drift * 0.6).r * 0.3
			+ texture(cloud_noise, uv * 5.3 - drift * 0.3).r * 0.12;
		float cover = coverage - pow(sd, 3.0) * 0.3;                   // a break where the sun is
		float c = smoothstep(1.0 - cover - 0.14, 1.0 - cover + 0.22, n);
		float thick = smoothstep(0.45, 0.95, n);
		vec3 cc = mix(cloud_lit, cloud_dark, thick * 0.85);
		// light through thin edges: silver toward the sun
		cc += sun_glow * pow(sd, 5.0) * (1.0 - thick) * 0.6 * glow_strength;
		c *= smoothstep(0.0, 0.1, d.y);
		col = mix(col, cc, c);
		// haze over the horizon
		col = mix(col, horizon, (1.0 - smoothstep(0.0, 0.18, d.y)) * 0.55);
	}
	COLOR = col;
}
