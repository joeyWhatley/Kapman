shader_type spatial;
render_mode cull_disabled;
// Low-poly sea. Faceted swell (flat normals from screen derivatives) that is calm in sheltered
// water and heavier offshore; colour from the real depth; foam and outward ripples at the shore.

#include "res://route_map/terrain3d/water_common.gdshaderinc"

uniform sampler2D height_tex : filter_linear;
uniform vec2 map_size = vec2(7316.0, 3872.0);
uniform sampler2D dist_tex : filter_linear;      // 0 = land, 1 = far out
uniform vec2 dist_size = vec2(7315.0, 3875.0);
uniform float dist_units = 425.0;
uniform vec2 wind = vec2(0.8, 0.35);

varying vec3 wpos;
varying float offshore;

float swell(vec2 p, float t) {
	vec2 w = normalize(wind);
	vec2 bend = vec2(sin(p.y * 0.011 + p.x * 0.004), sin(p.x * 0.013 - p.y * 0.005)) * 1.8;
	float a = sin(dot(p, w) * 0.05 + bend.x - t * 1.1);
	float b = sin(dot(p, vec2(-w.y, w.x)) * 0.037 + dot(p, w) * 0.021 + bend.y - t * 0.8);
	float c = sin(dot(p, w * 0.6 + vec2(0.3, -0.5)) * 0.083 - t * 1.7);
	return a * 0.5 + b * 0.35 + c * 0.2;
}

void vertex() {
	wpos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float du = texture(dist_tex, wpos.xz / dist_size).r * dist_units;
	offshore = smoothstep(2.0, 60.0, du);
	VERTEX.y += swell(wpos.xz, TIME) * mix(0.08, 0.55, offshore);
	wpos.y = VERTEX.y;
}

void fragment() {
	vec2 p = wpos.xz;
	float m = texture(height_tex, p / map_size).r;
	vec3 col = water_colour(clamp(-m, 0.0, 5000.0));
	float du = texture(dist_tex, p / dist_size).r * dist_units;
	// Shore: a foam band, then thin ripples moving outward.
	float band = 1.0 - smoothstep(0.4, 2.4, du);
	float rip = smoothstep(0.82, 0.97, sin(du * 1.4 - TIME * 1.6)) * (1.0 - smoothstep(2.0, 11.0, du));
	col = mix(col, foam.rgb, clamp(band * 0.8 + rip * 0.35, 0.0, 1.0));
	water_surface(col, VERTEX, VIEW, ALBEDO, NORMAL);
	ROUGHNESS = 0.18;
	SPECULAR = 0.6;
	METALLIC = 0.0;
}
