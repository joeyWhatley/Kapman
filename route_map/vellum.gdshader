shader_type canvas_item;
// Aged vellum: soft mottling, a few foxing spots, and edges browned by handling.
// Outside the chart the same shader gives the leather it lies on a subtle grain.

uniform sampler2D noise_tex : filter_linear, repeat_enable;
uniform vec4 chart = vec4(0.0, 0.0, 4946.0, 3139.0);   // x, y, w, h in map units

varying vec2 p;

void vertex() {
	p = VERTEX;
}

void fragment() {
	vec4 c = COLOR;
	float n = texture(noise_tex, p / 900.0).r;
	float n2 = texture(noise_tex, p / 140.0 + vec2(0.31, 0.77)).r;
	bool inside = p.x > chart.x && p.y > chart.y && p.x < chart.x + chart.z && p.y < chart.y + chart.w;
	if (inside) {
		float mottle = (n - 0.5) * 0.06 + (n2 - 0.5) * 0.03;
		float fox = smoothstep(0.8, 0.88, texture(noise_tex, p / 55.0 + vec2(0.7, 0.2)).r);
		vec2 d = min(p - chart.xy, chart.xy + chart.zw - p);
		float edge = (1.0 - smoothstep(0.0, 300.0, min(d.x, d.y))) * 0.1;
		c.rgb *= 1.0 + mottle - edge;
		c.rgb = mix(c.rgb, vec3(0.62, 0.46, 0.3), fox * 0.12);
	} else {
		c.rgb *= 1.0 + (n - 0.5) * 0.18 + (n2 - 0.5) * 0.08;
	}
	COLOR = c;
}