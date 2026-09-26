shader_type canvas_item;
// Soft cloud shadows sliding over land and sea, darker and thicker under storm systems.

uniform sampler2D noise_tex : filter_linear, repeat_enable;
uniform vec2 offset = vec2(0.0);   // accumulated drift, fed by the script
uniform vec4 storms[4];
uniform vec4 shade : source_color = vec4(0.1, 0.11, 0.14, 1.0);

varying vec2 p;

void vertex() {
	p = VERTEX;
}

void fragment() {
	float n = texture(noise_tex, (p - offset) / 1100.0).r;
	float n2 = texture(noise_tex, (p - offset * 1.4) / 420.0 + vec2(0.37, 0.11)).r;
	float cloud = smoothstep(0.56, 0.82, n * 0.75 + n2 * 0.25);
	float storm = 0.0;
	for (int i = 0; i < 4; i++) {
		vec4 s = storms[i];
		if (s.z > 0.0) {
			storm = max(storm, s.w * (1.0 - smoothstep(s.z * 0.2, s.z * 1.1, distance(p, s.xy))));
		}
	}
	float a = cloud * 0.06 + storm * (0.05 + 0.1 * smoothstep(0.35, 0.7, n));
	COLOR = vec4(shade.rgb, a);
}