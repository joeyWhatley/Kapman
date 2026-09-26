shader_type spatial;
render_mode cull_disabled;
// River and lake water, the same material as the sea (water_common). Rivers: UV.x runs downstream
// in map units, UV.y across the channel. A gentle faceted chop drifts downstream; foam at the banks
// matches the sea's shore foam.

#include "res://route_map/terrain3d/water_common.gdshaderinc"

uniform bool lake = false;
uniform float river_depth = 18.0;   // metres: rivers sit in the shallow end of the sea's palette
uniform float lake_depth = 30.0;

varying vec3 wpos;

void vertex() {
	wpos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float t = TIME;
	if (lake) {
		VERTEX.y += sin(wpos.x * 0.09 + t * 0.9) * cos(wpos.z * 0.08 - t * 0.7) * 0.06;
	} else {
		VERTEX.y += sin(UV.x * 0.55 - t * 2.0 + UV.y * 3.0) * 0.05;
	}
}

void fragment() {
	vec3 col;
	if (lake) {
		col = water_colour(lake_depth);
	} else {
		float across = abs(UV.y - 0.5) * 2.0;
		col = water_colour(river_depth * (1.0 - across * 0.7));
		// foam at the banks, like the sea's shore band
		col = mix(col, foam.rgb, smoothstep(0.78, 1.0, across) * 0.55);
		// faint ripple lines drifting downstream
		float rip = smoothstep(0.9, 0.99, sin(UV.x * 0.8 - TIME * 2.0 + sin(UV.y * 5.0) * 1.2));
		col = mix(col, foam.rgb, rip * (1.0 - across) * 0.12);
	}
	water_surface(col, VERTEX, VIEW, ALBEDO, NORMAL);
	ROUGHNESS = 0.18;
	SPECULAR = 0.6;
	METALLIC = 0.0;
}
