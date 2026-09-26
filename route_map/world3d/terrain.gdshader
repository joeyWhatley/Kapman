shader_type spatial;
render_mode cull_back, world_vertex_coords;
// Land (and the seabed seen through clear water). Heights from world_height.gdshaderinc, what
// grows where from landcover.gdshaderinc. The clipmap grid is morphed onto the coarser level at
// each ring's rim so levels meet without cracks. Colours are written in sRGB and converted.

#include "res://route_map/world3d/world_height.gdshaderinc"
#include "res://route_map/world3d/landcover.gdshaderinc"
#include "res://route_map/world3d/clouds.gdshaderinc"
#include "res://route_map/world3d/toon.gdshaderinc"

instance uniform float cell = 2.0;
instance uniform float half_extent = 64.0;
instance uniform float level_bias = 0.0;

uniform float height_scale = 1.0;
uniform float relief = 1.8;
uniform vec2 micro_off[2];          // origin offsets for the 4 m and 1.1 m grain (period 256)
// Canopy shell: beyond the drawn trees, woodland ground rises to treetop height and is painted
// as crowns, so a forest reads as a forest at any distance. The real trees shrink into it across
// the same band (scatter.gdshader fade), so nothing pops.
uniform float shell_start = 1300.0; // metres from the camera (where the far tree layer shrinks away)
uniform float shell_end = 1500.0;
const float CANOPY_H = 12.0;

varying vec2 local_xz;
varying float view_dist;
varying float shell_v;

void vertex() {
	vec3 w = VERTEX;
	vec2 rel = w.xz - NODE_POSITION_WORLD.xz;
	float d = max(abs(rel.x), abs(rel.y)) / half_extent;
	float morph = smoothstep(0.72, 0.94, d);
	vec2 odd = mod(round(w.xz / cell), 2.0);
	w.xz -= odd * cell * morph;
	// Only octaves longer than two grid squares can be shown at this level.
	int last = int(clamp(floor(log2(DETAIL_LAMBDA / (cell * 2.0))), -1.0, 5.0));
	float h = last >= 0 ? land_height(w.xz, 0, last) : base_height(w.xz);
	// Near the sea, sink the low land a little so coarse triangles never poke through the water:
	// the shoreline itself is drawn per pixel (the sea discards over land).
	float shore_sink = (1.0 - smoothstep(0.7, 2.5, h)) * (1.0 - smoothstep(150.0, 500.0, sea_distance(w.xz) * 2550.0)) * 1.3;
	// Coarse rings: along the coast take the lowest ground around the vertex, so big triangles never
	// stand above the water out at sea (the water decides the shoreline per pixel).
	if (sea_distance(w.xz) * 2550.0 < cell * 2.5) {
		float s = cell;
		h = min(h, min(min(base_height(w.xz + vec2(s, 0.0)), base_height(w.xz - vec2(s, 0.0))),
			min(base_height(w.xz + vec2(0.0, s)), base_height(w.xz - vec2(0.0, s)))));
	}
	w.y = (h - shore_sink) * height_scale + level_bias;
	shell_v = smoothstep(shell_start, shell_end, distance(w, CAMERA_POSITION_WORLD));
	if (shell_v > 0.0 && h > 1.0) {
		Cover cv = cover_at(w.xz, h);
		if (cv.woods > 0.0) {
			// crown lumps where the grid is fine enough to show them (10 m noise octave)
			float lumps = cell < 4.0 ? vnoise(w.xz / 10.0 + noise_off[4]) * 2.0 : 0.0;
			w.y += cv.woods * shell_v * (CANOPY_H * mix(0.85, 1.1, cv.fine) + lumps);
		}
	}
	local_xz = w.xz;
	VERTEX = w;
	view_dist = length((VIEW_MATRIX * vec4(w, 1.0)).xyz);
}

float h_full(vec2 p, int last) {
	return land_height(p, 0, last) * height_scale;
}

vec3 palette(float h, float slope, Cover c, float grain, float grain2, float dist) {
	vec3 sand = vec3(0.84, 0.77, 0.6);
	vec3 wet_sand = vec3(0.6, 0.56, 0.45);
	vec3 mud = vec3(0.4, 0.38, 0.31);
	vec3 marsh = vec3(0.47, 0.52, 0.36);
	vec3 meadow = vec3(0.34, 0.47, 0.22);
	vec3 meadow2 = vec3(0.44, 0.52, 0.26);
	vec3 peat = mix(vec3(0.55, 0.52, 0.33), vec3(0.47, 0.5, 0.3), grain);   // bog: sphagnum and cotton-grass
	vec3 heath = mix(vec3(0.56, 0.42, 0.44), vec3(0.5, 0.47, 0.3), grain * 0.6 + c.fine * 0.4); // heather, purple-brown with green
	vec3 litter = vec3(0.24, 0.32, 0.15);   // the floor under trees: moss and leaves
	vec3 earth = vec3(0.43, 0.35, 0.26);
	if (h < 0.3) {                          // under the sea: sand banks and mud
		return mix(wet_sand, mud, clamp(smoothstep(-2.0, -12.0, h) * 0.7 + c.mid * 0.3, 0.0, 1.0)) * (0.94 + grain * 0.12);
	}
	vec3 col = mix(meadow, meadow2, c.mid);
	// hay meadow, grazed sward, wet hollows with rush, flowering patches
	col = mix(col, vec3(0.58, 0.6, 0.34), smoothstep(0.55, 0.9, c.fine) * 0.35);
	col = mix(col, vec3(0.34, 0.46, 0.25), smoothstep(0.45, 0.1, c.fine) * 0.3);
	col = mix(col, vec3(0.66, 0.6, 0.38), smoothstep(0.72, 0.9, grain) * 0.3);
	col *= 0.9 + grain2 * 0.16;                                   // tussocks and bare patches
	col = mix(col, peat, c.peat);
	col = mix(col, heath * (0.9 + grain * 0.2), c.heath);
	col = mix(col, litter, c.woods);
	col = mix(col, marsh, c.marsh);
	// dunes: grey-green marram and dune grass, with bare sand only in the blow-outs
	vec3 dune_grass = mix(vec3(0.58, 0.6, 0.42), vec3(0.5, 0.55, 0.36), grain);
	col = mix(col, mix(dune_grass, sand, smoothstep(0.7, 0.8, c.fine + grain * 0.1)), c.dune);
	col = mix(col, sand * (0.95 + grain * 0.08), c.beach);
	col = mix(col, earth, smoothstep(0.35, 0.7, slope));
	// a darker rim where woods meet open ground, like an ink edge on a painted map
	float rim = c.woods * (1.0 - c.woods) * 4.0;
	col *= 1.0 - rim * 0.18;
	return col;
}

void fragment() {
	vec2 p = local_xz;
	// Fewer octaves where one pixel covers more ground.
	int last = int(clamp(floor(log2(DETAIL_LAMBDA / max(view_dist * 0.004, 2.0))), 0.0, 5.0));
	float e = clamp(view_dist * 0.0015, 0.3, 40.0);
	float h0 = h_full(p, last);
	float hx = h_full(p + vec2(e, 0.0), last);
	float hz = h_full(p + vec2(0.0, e), last);
	float h = h0 / height_scale;
	Cover c = cover_at(p, h);
	// Close-range grain (sand ripples, grass, leaf litter), fading out with distance.
	float near = 1.0 - smoothstep(60.0, 450.0, view_dist);
	float grain = mix(0.5, vnoise(p / 4.0 + micro_off[0]) * 0.5 + 0.5, near);
	float grain2 = mix(0.5, vnoise(p / 1.1 + micro_off[1]) * 0.5 + 0.5, near);
	vec3 n_true = normalize(vec3(h0 - hx, e, h0 - hz));
	float slope = 1.0 - n_true.y;
	// Stylised relief: light the land as if it were steeper than it is, so low Dutch ridges still read.
	vec3 n_world = normalize(vec3((h0 - hx) * relief, e, (h0 - hz) * relief));
	vec3 col = palette(h, slope * 3.0, c, grain, grain2, view_dist);
	// Canopy: crowns in light, shade in the gaps; alder darker, birch lighter, oak between.
	float cam3 = distance(vec3(p.x, h0, p.y), CAMERA_POSITION_WORLD);
	// between distant trees the eye sees shade, not a lit forest floor
	col = mix(col, vec3(0.13, 0.19, 0.1), c.woods * smoothstep(220.0, 450.0, cam3) * 0.75);
	float shell = smoothstep(shell_start, shell_end, cam3) * c.woods;
	if (shell > 0.0) {
		float detail = 1.0 - smoothstep(1500.0, 6000.0, view_dist);        // no crown pattern once it would shimmer
		vec2 q = p / 10.0 + noise_off[4];
		float cr = vnoise(q) * 0.65 + vnoise(p / 5.0 + noise_off[5]) * 0.35;
		float crowns = mix(0.55, smoothstep(-0.65, 0.6, cr), detail);
		vec3 lit = mix(vec3(0.3, 0.43, 0.19), vec3(0.36, 0.47, 0.22), c.mid);
		lit = mix(lit, vec3(0.2, 0.34, 0.16), c.wet);                           // alder carr
		lit = mix(lit, vec3(0.44, 0.53, 0.26), c.heath * 0.7);                  // birch on the heath
		vec3 canopy = mix(vec3(0.15, 0.24, 0.11), lit, crowns);
		canopy *= 1.0 - c.woods * (1.0 - c.woods) * 4.0 * 0.25;                 // wood edge
		col = mix(col, canopy, shell);
		// crowns bulge: tilt the normal by the crown pattern
		float gx = vnoise(q + vec2(0.15, 0.0)) - vnoise(q - vec2(0.15, 0.0));
		float gz = vnoise(q + vec2(0.0, 0.15)) - vnoise(q - vec2(0.0, 0.15));
		n_world = normalize(n_world + vec3(-gx, 0.0, -gz) * 1.4 * shell * detail);
	}
	ALBEDO = pow(col, vec3(2.2)) * cloud_shade(p);
	NORMAL = normalize((VIEW_MATRIX * vec4(n_world, 0.0)).xyz);
	ROUGHNESS = mix(0.9, 0.55, c.beach * (1.0 - smoothstep(0.3, 1.2, h)));   // wet sand at the tideline shines
	SPECULAR = 0.25;
}
