shader_type spatial;
render_mode cull_disabled;
// Vertex-coloured models (the cog's hull, deck, cargo and crew) with the shared cel lighting.

#include "res://route_map/world3d/toon.gdshaderinc"

uniform float roughness = 0.85;

void fragment() {
	ALBEDO = pow(COLOR.rgb, vec3(2.2));
	ROUGHNESS = roughness;
	SPECULAR = 0.2;
}
