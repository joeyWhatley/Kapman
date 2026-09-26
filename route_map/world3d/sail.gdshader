shader_type spatial;
render_mode cull_disabled;
#include "res://route_map/world3d/toon.gdshaderinc"
// Square sail of woven wool: vertical red and cream panels, billowing forward with the wind and
// rippling a little at the leech. UV.x across, UV.y down from the yard.

uniform float fill = 0.6;
uniform float set_amount = 1.0;

void vertex() {
	float x = UV.x * 2.0 - 1.0;
	float y = UV.y;
	// Furling: the sail gathers up toward the yard.
	VERTEX.y *= set_amount;
	// Billow: deepest in the middle, fixed along the yard, the foot pulled in by the sheets.
	float belly = (1.0 - x * x) * sin(min(y, 1.0) * 3.14159 * 0.85) * fill * 1.5;
	float flutter = sin(TIME * 5.3 + y * 7.0 + x * 3.0) * 0.05 * (1.0 - fill * 0.6) * y;
	VERTEX.z -= belly + flutter;
	VERTEX.x *= 1.0 - y * 0.08;
}

void fragment() {
	float band = step(0.5, fract(UV.x * 4.0));
	vec3 red = vec3(0.62, 0.17, 0.12);
	vec3 cream = vec3(0.86, 0.8, 0.66);
	vec3 col = mix(cream, red, band);
	// weave and weathering
	col *= 0.9 + 0.1 * sin(UV.y * 180.0) * sin(UV.x * 150.0);
	col *= 0.85 + 0.15 * (1.0 - UV.y * 0.4);
	ALBEDO = col;
	ROUGHNESS = 0.95;
	BACKLIGHT = vec3(0.35, 0.28, 0.2);   // light glowing through the cloth
}
