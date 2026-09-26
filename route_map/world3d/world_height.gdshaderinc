// Terrain at a local position (metres around the floating origin), streamed from the baked tiles
// (terrain_stream.gd): Copernicus DEM (30 m on the coasts, 90 m inland), EMODnet seabed, the chart's
// coastline, lakes and river channels. Nine windows, finest first; each fades out toward its edge
// into the next coarser one; outside all of them, the whole-map ETOPO1 grid.
// Positions: map units = origin_map + local / m_per_unit.

uniform sampler2DArray height_tiles : filter_linear, repeat_disable;
uniform vec4 lvl_rect[9];                // per level: window top-left (local m), window size (m), valid
uniform sampler2D coarse_tex : filter_linear, repeat_disable;
uniform vec2 coarse_size;                // map units
uniform vec2 origin_map;                 // integer map units
uniform float m_per_unit = 1112.0;
uniform vec2 noise_off[9];               // origin offsets per noise scale, folded on the CPU: 0-5 detail octaves, 6: 900 m, 7: 140 m, 8: 23 m

vec2 to_map(vec2 local) {
	return origin_map + local / m_per_unit;
}

// (height m, water surface m, distance to the shore m), finest data first
vec3 terrain_at(vec2 local, int first) {
	vec3 acc = vec3(0.0);
	float remain = 1.0;
	for (int k = first; k < 9; k++) {
		vec4 r = lvl_rect[k];
		if (r.w < 0.5) {
			continue;
		}
		vec2 uv = (local - r.xy) / r.z;
		float e = min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y));
		if (e <= 0.0) {
			continue;
		}
		vec3 v = textureLod(height_tiles, vec3(uv, float(k)), 0.0).rgb;
		if (v.r < -20000.0) {
			continue;                    // not baked at this level (open sea): use a coarser one
		}
		float w = smoothstep(0.015, 0.06, e);
		acc += v * w * remain;
		remain *= 1.0 - w;
		if (remain < 0.002) {
			return acc;
		}
	}
	float coarse = textureLod(coarse_tex, to_map(local) / coarse_size, 0.0).r;
	return acc + vec3(coarse, 0.0, 2550.0) * remain;
}

float base_height(vec2 local) {
	return terrain_at(local, 0).r;
}

// Water surface height (m): 0 for the sea, a lake's or river's own level.
float water_level(vec2 local) {
	return terrain_at(local, 0).g;
}

// Distance to the shore, 0..1 = 0..2.55 km (from 120 m data up, which carries it that far).
float sea_distance(vec2 local) {
	return clamp(terrain_at(local, 2).b / 2550.0, 0.0, 1.0);
}
// Value noise with a 256-cell period, so origin offsets can be folded exactly.
float hash2(vec2 p) {
	p = mod(p, 256.0);
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float vnoise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	vec2 u = f * f * (3.0 - 2.0 * f);
	float a = hash2(i);
	float b = hash2(i + vec2(1.0, 0.0));
	float c = hash2(i + vec2(0.0, 1.0));
	float d = hash2(i + vec2(1.0, 1.0));
	return mix(mix(a, b, u.x), mix(c, d, u.x), u.y) * 2.0 - 1.0;
}

// Octave o has wavelength DETAIL_LAMBDA / 2^o metres.
const float DETAIL_LAMBDA = 160.0;

float detail_noise(vec2 local, int first, int last) {
	float s = 0.0;
	float a = 1.0;
	for (int o = 0; o < 6; o++) {
		if (o >= first && o <= last) {
			float lam = DETAIL_LAMBDA / pow(2.0, float(o));
			s += vnoise(local / lam + noise_off[o]) * a;
		}
		a *= 0.5;
	}
	return s;
}

// How much made-up relief to add on top of the real data (none at the shore, so the coast stays true).
float detail_amp(float h) {
	return smoothstep(1.2, 4.0, h) * mix(0.9, 7.0, smoothstep(8.0, 140.0, h));
}

// Coastal dunes: ridges running along the shore, built on the belt behind the beach.
float dune_relief(vec2 local, float h) {
	float sea_m = sea_distance(local) * 2550.0;
	float belt = smoothstep(90.0, 260.0, sea_m) * (1.0 - smoothstep(1300.0, 2600.0, sea_m)) * smoothstep(1.0, 3.0, h);
	if (belt <= 0.0) {
		return 0.0;
	}
	float r = 1.0 - abs(vnoise(local / 80.0 + noise_off[1]));        // ridges (80 m = DETAIL_LAMBDA / 2)
	float r2 = vnoise(local / 40.0 + noise_off[2]) * 0.5 + 0.5;
	return belt * (r * r * 6.0 + r2 * 1.5);
}

float land_height(vec2 local, int first, int last) {
	float h = base_height(local);
	return h + detail_noise(local, first, last) * detail_amp(h) + dune_relief(local, h);
}
