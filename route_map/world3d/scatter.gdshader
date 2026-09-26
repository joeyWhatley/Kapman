shader_type spatial;
render_mode cull_disabled;
// Plants on the ground. Each instance reads its cell from the placement texture (scatter_place),
// so the costly land-cover maths runs once per cell, not per vertex. The MultiMesh sits at the
// origin with identity transforms: positions come from the grid. Vertex colours are sRGB.
//
// Layers hand trees over by distance from the camera: a plant is drawn only while its distance
// lies in [band_min, band_max). Layers with the same spacing place the same tree in the same cell
// (the placement is keyed to absolute cells), so the near and mid layers show one tree swapping
// from its detailed to its distant model. The last layer shrinks its trees into the canopy shell.

#include "res://route_map/world3d/clouds.gdshaderinc"
#include "res://route_map/world3d/toon.gdshaderinc"

uniform sampler2D place_tex : filter_nearest;
uniform int grid_n = 128;
uniform float spacing = 8.0;
uniform vec2 grid_origin;
uniform float band_min = 0.0;
uniform float band_max = 1.0e9;
uniform float shrink_start = 1.0e9;  // metres: trees shrink away from here to band_max
uniform float size_mul = 1.0;
uniform bool trees = false;          // shape the variants as species (1 alder, 2 birch)
uniform vec2 wind_dir = vec2(1.0, 0.0);
uniform float wind_strength = 0.4;
uniform float sway = 0.03;           // bend per metre of height
uniform vec4 tint0 : source_color = vec4(1.0);
uniform vec4 tint1 : source_color = vec4(1.0);
uniform vec4 tint2 : source_color = vec4(1.0);
uniform vec4 tint3 : source_color = vec4(1.0);
uniform float translucency = 0.3;
uniform int only_variant = -1;

varying vec3 tint;
varying float birch_bark;
varying vec2 world_xz;

void vertex() {
	ivec2 cell = ivec2(INSTANCE_ID % grid_n, INSTANCE_ID / grid_n);
	vec4 d = texelFetch(place_tex, cell, 0);
	d.xy -= 0.5;
	vec2 g = vec2(cell) - float(grid_n / 2);
	vec2 p = grid_origin + (g + 0.5 + d.xy * 0.9) * spacing;
	float size = mod(d.w, 4.0) * size_mul;
	float variant = floor(d.w / 4.0 + 0.01);
	float cam_d = distance(vec3(p.x, d.z, p.y), CAMERA_POSITION_WORLD);
	if (cam_d < band_min || cam_d >= band_max) {
		size = 0.0;
	}
	if (shrink_start < band_max) {
		size *= 1.0 - smoothstep(shrink_start, band_max, cam_d);
	}
	if (only_variant >= 0 && int(variant + 0.5) != only_variant) {
		size = 0.0;
	}
	birch_bark = 0.0;
	tint = vec3(1.0);
	if (d.w <= 0.0 || size <= 0.01) {
		VERTEX = vec3(0.0);
		NORMAL = vec3(0.0, 1.0, 0.0);
	} else {
		// per-plant randomness from its absolute cell (same in every layer, stable when re-centring)
		float r = fract(sin(dot(d.xy * 97.3 + vec2(d.w, d.z * 0.37), vec2(12.9898, 78.233))) * 43758.5453);
		float a = r * 6.2831853;
		mat2 rot = mat2(vec2(cos(a), sin(a)), vec2(-sin(a), cos(a)));
		vec3 v = VERTEX * size;
		v.xz = rot * v.xz;
		v.y *= mix(0.85, 1.15, fract(r * 7.13));
		v.xz *= mix(0.85, 1.15, fract(r * 3.71));
		if (trees) {
			if (variant > 0.5 && variant < 1.5) {        // alder: taller, narrow, a blunt cone
				v.y *= 1.2;
				v.xz *= mix(0.8, 0.55, clamp(v.y / (14.0 * size), 0.0, 1.0));
			} else if (variant > 1.5) {                  // birch: slender, light, airy
				v.y *= 1.1;
				v.xz *= 0.62;
				birch_bark = step(COLOR.g, COLOR.r + 0.02);   // bark is the brown (red >= green) part
			}
		}
		// wind: the higher up the plant, the more it bends
		float gust = sin(TIME * 1.3 + p.x * 0.05 + p.y * 0.04) * 0.5 + 0.5;
		v.xz += wind_dir * v.y * sway * wind_strength * (0.6 + gust) + wind_dir * v.y * sway * 0.3 * sin(TIME * 3.1 + r * 20.0);
		VERTEX = v + vec3(p.x, d.z - 0.15, p.y);
		NORMAL.xz = rot * NORMAL.xz;
		vec4 t = variant < 0.5 ? tint0 : (variant < 1.5 ? tint1 : (variant < 2.5 ? tint2 : tint3));
		tint = t.rgb * (0.88 + r * 0.24);
	}
	world_xz = VERTEX.xz;
}

void fragment() {
	vec3 base = pow(COLOR.rgb, vec3(2.2));
	vec3 col = mix(base * tint, vec3(0.6, 0.58, 0.53), birch_bark);   // birch: white bark
	col *= cloud_shade(world_xz);
	ALBEDO = col;
	ROUGHNESS = 0.9;
	SPECULAR = 0.2;
	BACKLIGHT = ALBEDO * translucency;
}
