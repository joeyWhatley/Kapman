shader_type canvas_item;
// Fades a layer by the fog-of-war mask. invert = 0: visible where charted; 1: visible where fogged.
// grain > 0 (the fog wash) adds paper grain and slow drifting cloud so the fog feels alive.

uniform sampler2D fog : filter_linear;
uniform vec2 map_size = vec2(1235.0, 1130.0);
uniform float invert = 0.0;
uniform float strength = 1.0;
uniform float grain = 0.0;
uniform sampler2D clouds : filter_linear, repeat_enable;

varying vec2 map_pos;

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(i), hash(i + vec2(1.0, 0.0)), u.x), mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), u.x), u.y);
}

void vertex() {
	map_pos = VERTEX;
}

void fragment() {
	float e = texture(fog, map_pos / map_size).r;
	float a = mix(e, 1.0 - e, invert);
	if (grain > 0.0) {
		float g = hash(floor(map_pos / 2.5));
		vec2 drift = vec2(TIME * 3.0, TIME * 1.2);
		float cloud = texture(clouds, (map_pos + drift) / 700.0).r * 0.6 + texture(clouds, (map_pos - drift * 0.7) / 260.0).r * 0.4;
		a *= (1.0 - grain + grain * g) * (0.65 + 0.55 * cloud);
	}
	COLOR.a *= a * strength;
}
