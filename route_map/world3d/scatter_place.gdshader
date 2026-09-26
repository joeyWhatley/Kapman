shader_type canvas_item;
render_mode blend_disabled, unshaded;
// Placement pass for one scatter layer: one pixel per grid cell around the camera. Decides from
// the same height and land-cover rules as the ground whether a plant grows in the cell, and writes
// (jitter x + 0.5, jitter z + 0.5, ground height, size + 4 x variant) for scatter.gdshader to read.
// size 0 = nothing grows here.

#include "res://route_map/world3d/world_height.gdshaderinc"
#include "res://route_map/world3d/landcover.gdshaderinc"

uniform int kind = 0;               // 0 tree, 1 bush, 2 grass, 3 reed
uniform int grid_n = 128;
uniform float spacing = 8.0;
uniform vec2 grid_origin;           // local metres of cell (0, 0) (the grid's centre cell)
uniform vec2 grid_key;              // absolute index of that cell, folded to 0..256
uniform float inner_skip = 0.0;     // metres: leave the middle to a finer layer

float rnd(vec2 k, float salt) {
	return hash2(mod(k + vec2(salt * 13.7, salt * 7.3), 256.0));
}

void fragment() {
	vec2 g = floor(UV * float(grid_n)) - float(grid_n / 2);
	vec2 key = grid_key + g;
	vec2 jit = vec2(rnd(key, 1.0), rnd(key, 2.0)) - 0.5;
	vec2 p = grid_origin + (g + 0.5 + jit * 0.9) * spacing;
	float h = land_height(p, 0, 5);
	Cover c = cover_at(p, h);
	float meadow = (1.0 - c.woods) * (1.0 - c.beach) * (1.0 - c.dune) * (1.0 - c.marsh) * (1.0 - c.heath) * (1.0 - c.peat);
	float density = 0.0;
	float variant = 0.0;
	if (kind == 0) {            // trees: woods, and the odd lone tree in the meadows
		float edge = c.woods * (1.0 - c.woods) * 4.0;
		density = c.woods * 0.97 + meadow * 0.012 + c.peat * 0.03 + c.heath * 0.05;
		// alder in the wet, birch on the heath and at wood edges, oak (and beech) on the dry
		variant = c.wet > 0.5 ? 1.0 : ((c.heath > 0.3 || c.peat > 0.3 || (edge > 0.5 && rnd(key, 6.0) < 0.5)) ? 2.0 : 0.0);
	} else if (kind == 1) {     // bushes: wood edges, heath, dunes, hedges of thorn
		float edge = c.woods * (1.0 - c.woods) * 4.0;
		density = edge * 0.7 + c.woods * 0.12 + c.heath * 0.25 + c.dune * 0.15 + meadow * 0.03;
		variant = c.dune > 0.5 ? 1.0 : 0.0;
	} else if (kind == 2) {     // grass tufts
		density = meadow * 0.8 + c.dune * 0.9 + c.heath * 0.8 + c.marsh * 0.9 + c.peat * 0.6 + c.woods * 0.12;
		variant = c.dune > 0.4 ? 1.0 : (c.heath > 0.4 ? 2.0 : (c.marsh > 0.4 || c.peat > 0.5 ? 3.0 : 0.0));
	} else {                    // reeds: salt marsh edges and the wettest ground
		density = c.marsh * 0.7 + c.wet * c.peat * 0.5 + c.wet * 0.08;
	}
	if (h < 0.6 || h < water_level(p) + 0.4 || c.beach > 0.5) {      // nothing grows in the water
		density = 0.0;
	}
	float d_centre = max(abs(g.x), abs(g.y)) * spacing;
	if (d_centre < inner_skip) {
		density = 0.0;
	}
	float size = rnd(key, 3.0) < density ? mix(0.7, 1.3, rnd(key, 4.0)) : 0.0;
	COLOR = vec4(jit.x + 0.5, jit.y + 0.5, h, size > 0.0 ? size + variant * 4.0 : 0.0);
}
