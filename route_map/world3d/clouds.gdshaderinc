// Shadows of the clouds drifting over land and sea (the sky shader draws the clouds themselves).
// cloud_off is folded on the CPU from the origin and the drift, so it never grows large.

uniform sampler2D cloud_tex : filter_linear_mipmap, repeat_enable;
uniform vec2 cloud_off;                // for the 4.2 km pattern
uniform vec2 cloud_off2;               // for the 1.3 km pattern
uniform float cloud_cover = 0.55;       // 0 clear .. 1 overcast
uniform float cloud_shadow = 0.5;       // how dark a cloud's shadow is

float cloud_shade(vec2 local) {
	float n = texture(cloud_tex, local / 4200.0 + cloud_off).r * 0.7 + texture(cloud_tex, local / 1300.0 + cloud_off2).r * 0.3;
	float c = smoothstep(1.0 - cloud_cover - 0.04, 1.0 - cloud_cover + 0.04, n);   // crisp painted shadows
	return 1.0 - c * cloud_shadow;
}
